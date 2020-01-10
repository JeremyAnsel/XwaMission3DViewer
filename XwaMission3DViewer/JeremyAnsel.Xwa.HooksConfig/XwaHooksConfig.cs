using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace JeremyAnsel.Xwa.HooksConfig
{
    public static class XwaHooksConfig
    {
        private static readonly TypeConverter Int32Converter = TypeDescriptor.GetConverter(typeof(int));

        public static int ToInt32(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException(nameof(text));
            }

            text = text.Trim();

            bool isNegative = text.StartsWith("-", StringComparison.Ordinal);
            if (isNegative)
            {
                text = text.Substring(1).TrimStart();
            }

            int index = text.IndexOf('.');
            if (index != -1)
            {
                text = text.Substring(0, index);
            }

            int value = (int)Int32Converter.ConvertFromInvariantString(text);
            if (isNegative)
            {
                value = -value;
            }

            return value;
        }

        public static string GetStringWithoutExtension(string str)
        {
            return Path.ChangeExtension(str, null);
        }

        public static IList<string> GetFileLines(string path, string section = null)
        {
            section = section ?? string.Empty;

            var values = new List<string>();

            if (!File.Exists(path))
            {
                return values;
            }

            using (var reader = new StreamReader(path))
            {
                string line;
                bool readSection = string.IsNullOrEmpty(section);

                while ((line = reader.ReadLine()) != null)
                {
                    line = RemoveWhitespaces(line);

                    if (line.Length == 0)
                    {
                        continue;
                    }

                    if (line.StartsWith("#", StringComparison.Ordinal)
                        || line.StartsWith(";", StringComparison.Ordinal)
                        || line.StartsWith("//", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (line.StartsWith("[", StringComparison.Ordinal)
                        && line.EndsWith("]", StringComparison.Ordinal))
                    {
                        string name = line.Substring(1, line.Length - 2);

                        if (string.Equals(name, section, StringComparison.OrdinalIgnoreCase))
                        {
                            readSection = true;
                        }
                        else
                        {
                            readSection = false;
                        }
                    }
                    else
                    {
                        if (readSection)
                        {
                            values.Add(line);
                        }
                    }
                }
            }

            return values;
        }

        public static string GetFileKeyValue(IList<string> lines, string key)
        {
            if (lines == null)
            {
                throw new ArgumentNullException(nameof(lines));
            }

            foreach (string line in lines)
            {
                int pos = line.IndexOf('=');

                if (pos == -1)
                {
                    continue;
                }

                string name = line.Substring(0, pos);

                if (name.Length == 0)
                {
                    continue;
                }

                if (string.Equals(name, key, StringComparison.OrdinalIgnoreCase))
                {
                    string value = line.Substring(pos + 1);
                    return value;
                }
            }

            return string.Empty;
        }

        public static int GetFileKeyValueInt(IList<string> lines, string key, int defaultValue = 0)
        {
            string value = GetFileKeyValue(lines, key);

            if (value.Length == 0)
            {
                return defaultValue;
            }

            return ToInt32(value);
        }

        public static IList<string> Tokennize(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            string[] tokens = str.Split(',', ';');
            return tokens.ToList();
        }

        public static IList<IList<string>> GetFileListValues(IList<string> lines)
        {
            if (lines == null)
            {
                throw new ArgumentNullException(nameof(lines));
            }

            var values = new List<IList<string>>();

            foreach (string line in lines)
            {
                values.Add(Tokennize(line));
            }

            return values;
        }

        public static IList<int> GetFileListIntValues(IList<string> lines)
        {
            if (lines == null)
            {
                throw new ArgumentNullException(nameof(lines));
            }

            var values = new List<int>();

            foreach (string line in lines)
            {
                int value = int.Parse(line, CultureInfo.InvariantCulture);
                values.Add(value);
            }

            return values;
        }

        public static IList<ushort> GetFileListUnsignedShortValues(IList<string> lines)
        {
            if (lines == null)
            {
                throw new ArgumentNullException(nameof(lines));
            }

            var values = new List<ushort>();

            foreach (string line in lines)
            {
                ushort value = ushort.Parse(line, CultureInfo.InvariantCulture);
                values.Add(value);
            }

            return values;
        }

        private static string RemoveWhitespaces(string str)
        {
            var sb = new StringBuilder(str.Length);
            int c;

            using (var reader = new StringReader(str))
            {
                while ((c = reader.Read()) != -1)
                {
                    if (!char.IsWhiteSpace((char)c))
                    {
                        sb.Append((char)c);
                    }
                }
            }

            return sb.ToString();
        }
    }
}
