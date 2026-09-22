using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace StreamRoar.Editor
{
    /// <summary>打开项目时接入仓库的提交检查，不覆盖其他 Git hooks。</summary>
    public static class GitHookInstaller
    {
        const string AutomaticCheckKey = "StreamRoar.GitHookInstaller.AutomaticCheck";
        const string LegacyForwarder = "#!/bin/sh\n# StreamRoar commit-msg forwarder\n" +
            "root=$(git rev-parse --show-toplevel) || exit 1\n" +
            "exec sh \"$root/.githooks/commit-msg\" \"$@\"\n";
        const string Forwarder = "#!/bin/sh\n# StreamRoar commit-msg forwarder\n" +
            "root=$(git rev-parse --show-toplevel) || exit 1\n" +
            "[ -f \"$root/.githooks/commit-msg\" ] || exit 0\n" +
            "exec sh \"$root/.githooks/commit-msg\" \"$@\"\n";

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            if (Application.isBatchMode || SessionState.GetBool(AutomaticCheckKey, false))
                return;

            // SessionState 跨脚本重载保留，退出 Unity 后清空。
            SessionState.SetBool(AutomaticCheckKey, true);
            EditorApplication.delayCall += Install;
        }

        /// <summary>按需安装提交钩子；已有自定义配置或钩子时保留原样并提示。</summary>
        [MenuItem("StreamRoar/Git/Install Commit Hook")]
        public static void Install()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
            string gitPath = Path.Combine(root, ".git");
            if (!Directory.Exists(gitPath) && !File.Exists(gitPath))
                return; // 下载的 ZIP 没有 Git 元数据。

            try
            {
                string source = Path.Combine(root, ".githooks", "commit-msg");
                if (!File.Exists(source))
                    throw new FileNotFoundException("缺少仓库提交检查脚本。", source);

                int status = Run("git", "config --get core.hooksPath", root, out string configured);
                if (status != 0 && status != 1)
                    throw new InvalidOperationException(configured);

                if (Run("git", "rev-parse --git-path hooks", root, out string hooks) != 0)
                    throw new InvalidOperationException(hooks);
                hooks = Path.GetFullPath(Path.Combine(root, hooks));
                string target = Path.Combine(hooks, "commit-msg");
                var comparison = Application.platform == RuntimePlatform.WindowsEditor
                    ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                bool installed = false;

                // 已经直接使用仓库的 .githooks 时，只确保脚本可执行。
                if (!string.Equals(target, source, comparison))
                {
                    if (status == 0)
                    {
                        Debug.LogWarning($"[Git hooks] 保留已有 core.hooksPath={configured}；" +
                            "请在现有 commit-msg 中接入 .githooks/commit-msg。");
                        return;
                    }
                    string existing = File.Exists(target) ? File.ReadAllText(target) : null;
                    // 只升级内容完全匹配的旧版转发器，保留用户自定义钩子。
                    if (existing != null && existing != Forwarder && existing != LegacyForwarder)
                    {
                        Debug.LogWarning($"[Git hooks] 保留已有钩子 {target}；" +
                            "请在其中接入 .githooks/commit-msg。");
                        return;
                    }
                    if (existing != Forwarder)
                    {
                        Directory.CreateDirectory(hooks);
                        File.WriteAllText(target, Forwarder, new UTF8Encoding(false));
                        installed = existing == null;
                    }
                }

                if (Application.platform != RuntimePlatform.WindowsEditor &&
                    Run("chmod", "+x commit-msg", hooks, out string error) != 0)
                    throw new InvalidOperationException(error);

                if (installed)
                    Debug.Log("[Git hooks] 已安装提交信息检查。");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Git hooks] 提交检查初始化失败：{exception.Message}");
            }
        }

        static int Run(string executable, string arguments, string directory, out string output)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(executable, arguments)
                {
                    WorkingDirectory = directory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(5000))
            {
                process.Kill();
                throw new TimeoutException($"{executable} {arguments} 执行超时。");
            }
            output = (process.ExitCode == 0 ? stdout.Result : stderr.Result).Trim();
            return process.ExitCode;
        }
    }
}
