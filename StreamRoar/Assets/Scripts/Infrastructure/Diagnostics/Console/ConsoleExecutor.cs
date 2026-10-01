using System;
using System.Collections.Generic;
using System.Globalization;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 查找命令、校验签名并同步执行。
    /// </summary>
    public sealed class ConsoleExecutor
    {
        readonly ConsoleCommandRegistry m_Registry;

        public ConsoleExecutor(ConsoleCommandRegistry registry)
        {
            m_Registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public bool TryExecute(string line, IConsoleWriter writer, out string error)
        {
            error = null;
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (!ConsoleTokenizer.TryTokenize(line, out List<string> tokens, out string tokenizeError))
            {
                error = tokenizeError;
                writer.WriteError(error);
                return false;
            }

            if (tokens == null || tokens.Count == 0)
                return true;

            string commandName = tokens[0];
            if (!m_Registry.TryGet(commandName, out IConsoleCommand command))
            {
                error = $"Unknown command: {commandName}";
                writer.WriteError(error);
                return false;
            }

            var args = new List<string>(tokens.Count - 1);
            for (int i = 1; i < tokens.Count; i++)
                args.Add(tokens[i]);

            if (!TryValidate(command.Schema, args, out error))
            {
                writer.WriteError(error);
                writer.WriteError($"Usage: {command.Name} {command.Schema.Format()}");
                return false;
            }

            try
            {
                command.Execute(args, writer);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                writer.WriteError(error);
                return false;
            }
        }

        static bool TryValidate(ConsoleArgSchema schema, List<string> args, out string error)
        {
            error = null;
            if (args.Count < schema.MinCount || args.Count > schema.MaxCount)
            {
                error = $"Invalid argument count: got {args.Count}, expected {schema.Format()}.";
                return false;
            }

            for (int i = 0; i < args.Count; i++)
            {
                if (!TryParse(schema.ElementType, args[i], out _))
                {
                    error = $"Invalid argument[{i}]: expected {schema.ElementType}, got '{args[i]}'.";
                    return false;
                }
            }

            return true;
        }

        static bool TryParse(ConsoleArgType type, string raw, out object value)
        {
            value = null;
            switch (type)
            {
                case ConsoleArgType.String:
                    value = raw;
                    return true;
                case ConsoleArgType.Int:
                    if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i))
                    {
                        value = i;
                        return true;
                    }

                    return false;
                case ConsoleArgType.Float:
                    if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float f))
                    {
                        value = f;
                        return true;
                    }

                    return false;
                case ConsoleArgType.Bool:
                    if (bool.TryParse(raw, out bool b))
                    {
                        value = b;
                        return true;
                    }

                    if (raw == "0" || raw == "1")
                    {
                        value = raw == "1";
                        return true;
                    }

                    return false;
                default:
                    return false;
            }
        }
    }
}
