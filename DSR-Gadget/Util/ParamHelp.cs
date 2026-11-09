using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml;

namespace DSR_Gadget
{
    /// <summary>
    /// English help for one field: friendly name, description, and hints from Smithbox's param meta.
    /// </summary>
    internal class ParamFieldHelp
    {
        public string Name;          // e.g. "Weight"
        public string Description;   // plain-English explanation
        public string Enum;          // name of a value list, e.g. WEAPON_CATEGORY
        public string Refs;          // other params this field points at, e.g. "SpEffectParam"
        public bool IsBool;
        public bool IsPadding;
        public bool IsObsolete;
    }

    internal class ParamHelpInfo
    {
        public string Description;
        public Dictionary<string, ParamFieldHelp> Fields = new Dictionary<string, ParamFieldHelp>(StringComparer.OrdinalIgnoreCase);

        public ParamFieldHelp For(ParamField field)
        {
            return Fields.TryGetValue(field.Name, out ParamFieldHelp help) ? help : null;
        }
    }

    /// <summary>
    /// Loads Smithbox's English annotations, meta and enums from Resources/Params/Smithbox.
    /// Anything missing simply means less help is shown; the editor still works.
    /// </summary>
    internal static class ParamHelp
    {
        private static readonly string root = Path.Combine(GetTxtResourceClass.ExeDir, "Resources", "Params", "Smithbox");
        private static readonly Dictionary<string, ParamHelpInfo> cache = new Dictionary<string, ParamHelpInfo>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, List<KeyValuePair<long, string>>> enums;

        public static ParamHelpInfo For(ParamDef def)
        {
            string key = def.ParamType + "|" + def.DefFile;
            if (cache.TryGetValue(key, out ParamHelpInfo info))
                return info;

            info = new ParamHelpInfo();
            cache[key] = info;

            // Annotations: English names and descriptions, by param type
            string annotations = Path.Combine(root, "Annotations", def.ParamType + ".json");
            if (File.Exists(annotations))
            {
                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(File.ReadAllText(annotations)))
                    {
                        JsonElement json = doc.RootElement;
                        info.Description = getString(json, "Description");
                        if (json.TryGetProperty("Fields", out JsonElement fields) && fields.ValueKind == JsonValueKind.Array)
                        {
                            foreach (JsonElement field in fields.EnumerateArray())
                            {
                                string internalName = getString(field, "Field");
                                if (string.IsNullOrEmpty(internalName))
                                    continue;
                                ParamFieldHelp help = getOrAdd(info, internalName);
                                help.Name = getString(field, "Name");
                                help.Description = getString(field, "Description");
                            }
                        }
                    }
                }
                catch (Exception ex) when (ex is JsonException || ex is IOException)
                {
                    GadgetLogger.Log("Couldn't read " + annotations + ": " + ex.Message);
                }
            }

            // Meta: value lists, references, yes/no flags, padding; by paramdef file name
            string meta = Path.Combine(root, "Meta", def.DefFile + ".xml");
            if (File.Exists(meta))
            {
                try
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(meta);
                    XmlNode fieldsNode = xml.SelectSingleNode("/PARAMMETA/Field");
                    if (fieldsNode != null)
                    {
                        foreach (XmlNode node in fieldsNode.ChildNodes)
                        {
                            if (node.NodeType != XmlNodeType.Element)
                                continue;
                            ParamFieldHelp help = getOrAdd(info, node.Name);
                            help.Enum = node.Attributes["Enum"]?.Value;
                            help.Refs = node.Attributes["Refs"]?.Value;
                            help.IsBool = node.Attributes["IsBool"] != null;
                            help.IsPadding = node.Attributes["Padding"] != null;
                            help.IsObsolete = node.Attributes["Obsolete"] != null;
                        }
                    }
                }
                catch (Exception ex) when (ex is XmlException || ex is IOException)
                {
                    GadgetLogger.Log("Couldn't read " + meta + ": " + ex.Message);
                }
            }

            return info;
        }

        /// <summary>The choices for a value list, as (number, English text). Null if the list isn't known.</summary>
        public static List<KeyValuePair<long, string>> Enum(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            if (enums == null)
                loadEnums();
            return enums.TryGetValue(name, out var options) ? options : null;
        }

        private static void loadEnums()
        {
            enums = new Dictionary<string, List<KeyValuePair<long, string>>>(StringComparer.OrdinalIgnoreCase);
            string dir = Path.Combine(root, "Enums");
            if (!Directory.Exists(dir))
                return;

            foreach (string file in Directory.GetFiles(dir, "*.json"))
            {
                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(File.ReadAllText(file)))
                    {
                        JsonElement json = doc.RootElement;
                        string key = getString(json, "Key") ?? Path.GetFileNameWithoutExtension(file);
                        List<KeyValuePair<long, string>> options = new List<KeyValuePair<long, string>>();
                        if (json.TryGetProperty("Options", out JsonElement list) && list.ValueKind == JsonValueKind.Array)
                        {
                            foreach (JsonElement option in list.EnumerateArray())
                            {
                                if (!long.TryParse(getString(option, "Key"), out long value))
                                    continue;
                                options.Add(new KeyValuePair<long, string>(value, englishName(option) ?? value.ToString()));
                            }
                        }
                        enums[key] = options;
                    }
                }
                catch (Exception ex) when (ex is JsonException || ex is IOException)
                {
                    GadgetLogger.Log("Couldn't read " + file + ": " + ex.Message);
                }
            }
        }

        private static string englishName(JsonElement element)
        {
            if (!element.TryGetProperty("Names", out JsonElement names) || names.ValueKind != JsonValueKind.Array)
                return null;
            string first = null;
            foreach (JsonElement name in names.EnumerateArray())
            {
                string text = getString(name, "Text");
                if (first == null)
                    first = text;
                if (string.Equals(getString(name, "Language"), "English", StringComparison.OrdinalIgnoreCase))
                    return text;
            }
            return first;
        }

        private static ParamFieldHelp getOrAdd(ParamHelpInfo info, string field)
        {
            if (!info.Fields.TryGetValue(field, out ParamFieldHelp help))
                info.Fields[field] = help = new ParamFieldHelp();
            return help;
        }

        private static string getString(JsonElement element, string property)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out JsonElement value))
            {
                if (value.ValueKind == JsonValueKind.String)
                    return value.GetString();
                if (value.ValueKind == JsonValueKind.Number)
                    return value.GetRawText();
            }
            return null;
        }
    }
}
