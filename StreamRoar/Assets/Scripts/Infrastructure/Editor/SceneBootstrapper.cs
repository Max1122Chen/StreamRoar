#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace StreamRoar.Editor
{
    /// <summary>
    /// 从任意场景进入 Play Mode 时先加载启动场景。
    /// </summary>
    [InitializeOnLoad]
    public static class SceneBootstrapper
    {
        const string k_PreviousSceneKey = "StreamRoar.SceneBootstrapper.PreviousScene";
        const string k_EnabledKey = "StreamRoar.SceneBootstrapper.Enabled";
        const string k_EnableMenu = "StreamRoar/Play/Load Launch Scene On Play";
        const string k_DisableMenu = "StreamRoar/Play/Don't Load Launch Scene On Play";

        static string BootstrapScene => EditorBuildSettings.scenes[0].path;

        static string PreviousScene
        {
            get => EditorPrefs.GetString(k_PreviousSceneKey);
            set => EditorPrefs.SetString(k_PreviousSceneKey, value);
        }

        static bool IsEnabled
        {
            get => EditorPrefs.GetBool(k_EnabledKey, true);
            set => EditorPrefs.SetBool(k_EnabledKey, value);
        }

        static SceneBootstrapper()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!IsEnabled)
                return;

            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    PreviousScene = EditorSceneManager.GetActiveScene().path;
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        EditorSceneManager.OpenScene(BootstrapScene);
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    if (!string.IsNullOrEmpty(PreviousScene))
                        EditorSceneManager.OpenScene(PreviousScene);
                    break;
            }
        }

        [MenuItem(k_EnableMenu)]
        static void Enable()
        {
            IsEnabled = true;
        }

        [MenuItem(k_EnableMenu, true)]
        static bool ValidateEnable()
        {
            return !IsEnabled;
        }

        [MenuItem(k_DisableMenu)]
        static void Disable()
        {
            IsEnabled = false;
        }

        [MenuItem(k_DisableMenu, true)]
        static bool ValidateDisable()
        {
            return IsEnabled;
        }
    }
}
#endif
