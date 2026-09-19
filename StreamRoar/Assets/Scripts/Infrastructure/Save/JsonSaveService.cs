using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 将每份存档保存为独立 JSON 文件。同步调用，不支持并发访问。
    /// </summary>
    public sealed class JsonSaveService : ISaveService
    {
        readonly string m_Directory;
        readonly JsonSerializer m_Serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.None,
            CheckAdditionalContent = true
        });

        /// <summary>
        /// 指定存档目录，首次保存时创建目录。
        /// </summary>
        public JsonSaveService(string directory)
        {
            m_Directory = Path.GetFullPath(directory ?? throw new ArgumentNullException(nameof(directory)));
        }

        /// <summary>
        /// 读取指定类型的数据。缺失文件返回 false，损坏或不兼容的数据直接暴露异常。
        /// </summary>
        public bool TryLoad<T>(string saveId, out T data)
        {
            string path = GetPath(saveId);
            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (FileNotFoundException)
            {
                data = default;
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                data = default;
                return false;
            }

            using var reader = new JsonTextReader(new StringReader(json));
            data = m_Serializer.Deserialize<T>(reader);
            if (data is null)
                throw new InvalidDataException($"存档数据为空：{path}");

            return true;
        }

        /// <summary>
        /// 先完成序列化和临时文件写入，再替换正式文件。
        /// </summary>
        public void Save<T>(string saveId, T data)
        {
            string path = GetPath(saveId);
            using var writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
            m_Serializer.Serialize(writer, data ?? throw new ArgumentNullException(nameof(data)));
            Directory.CreateDirectory(m_Directory);
            string temporaryPath = path + ".tmp";
            File.WriteAllText(temporaryPath, writer.ToString());
            if (File.Exists(path))
                File.Replace(temporaryPath, path, null);
            else
                File.Move(temporaryPath, path);
        }

        /// <summary>
        /// 检查正式存档文件是否存在，不包含临时文件。
        /// </summary>
        public bool Exists(string saveId)
        {
            return File.Exists(GetPath(saveId));
        }

        /// <summary>
        /// 删除正式存档文件。目录或文件不存在时不执行操作。
        /// </summary>
        public void Delete(string saveId)
        {
            string path = GetPath(saveId);
            if (File.Exists(path))
                File.Delete(path);
        }

        string GetPath(string saveId)
        {
            if (string.IsNullOrEmpty(saveId) || !Regex.IsMatch(saveId, @"\A[a-z0-9][a-z0-9_-]*\z"))
                throw new ArgumentException("存档标识须以小写字母或数字开头，仅包含小写字母、数字、下划线或连字符。", nameof(saveId));

            return Path.Combine(m_Directory, saveId + ".json");
        }
    }
}
