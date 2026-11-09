using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace DSR_Gadget
{
    /// <summary>
    /// One field of a param row, with its position worked out from a Paramdex paramdef.
    /// </summary>
    internal class ParamField
    {
        public string Name;
        public string Type;          // s8 u8 s16 u16 s32 u32 f32 dummy8 fixstr fixstrW
        public int Offset;           // byte offset in the row (for bitfields, offset of the containing unit)
        public int Size;             // bytes (for bitfields, size of the containing unit)
        public int BitOffset = -1;   // -1 when not a bitfield
        public int BitSize = -1;
        public string Description;

        public bool IsBitfield => BitSize > 0;
        public bool IsPadding => Type == "dummy8" && !IsBitfield;
        public bool IsText => Type == "fixstr" || Type == "fixstrW";

        public string TypeLabel => IsBitfield ? (BitSize == 1 ? "flag" : $"{BitSize}-bit") : Type;

        /// <summary>Reads this field's value from a row's bytes, as display text.</summary>
        public string Read(byte[] row)
        {
            if (IsBitfield)
            {
                ulong unit = readUnsigned(row, Offset, Size);
                ulong value = (unit >> BitOffset) & ((1UL << BitSize) - 1);
                return value.ToString(CultureInfo.InvariantCulture);
            }

            switch (Type)
            {
                case "s8": return ((sbyte)row[Offset]).ToString(CultureInfo.InvariantCulture);
                case "u8": return row[Offset].ToString(CultureInfo.InvariantCulture);
                case "s16": return BitConverter.ToInt16(row, Offset).ToString(CultureInfo.InvariantCulture);
                case "u16": return BitConverter.ToUInt16(row, Offset).ToString(CultureInfo.InvariantCulture);
                case "s32": return BitConverter.ToInt32(row, Offset).ToString(CultureInfo.InvariantCulture);
                case "u32": return BitConverter.ToUInt32(row, Offset).ToString(CultureInfo.InvariantCulture);
                case "f32": return BitConverter.ToSingle(row, Offset).ToString("R", CultureInfo.InvariantCulture);
                case "fixstr": return Encoding.GetEncoding("shift_jis").GetString(row, Offset, Size).TrimEnd('\0');
                case "fixstrW": return Encoding.Unicode.GetString(row, Offset, Size).TrimEnd('\0');
                default: return BitConverter.ToString(row, Offset, Size).Replace("-", " ");
            }
        }

        /// <summary>
        /// Writes a value typed by the user into a row's bytes. Returns an error message, or null on success.
        /// </summary>
        public string Write(byte[] row, string text)
        {
            text = (text ?? "").Trim();
            NumberStyles ints = NumberStyles.Integer;
            CultureInfo inv = CultureInfo.InvariantCulture;

            if (IsBitfield)
            {
                if (!ulong.TryParse(text, ints, inv, out ulong value))
                    return "Enter a whole number.";
                ulong max = (1UL << BitSize) - 1;
                if (value > max)
                    return $"Must be between 0 and {max}.";
                ulong unit = readUnsigned(row, Offset, Size);
                ulong mask = max << BitOffset;
                unit = (unit & ~mask) | (value << BitOffset);
                writeUnsigned(row, Offset, Size, unit);
                return null;
            }

            switch (Type)
            {
                case "s8":
                    if (!sbyte.TryParse(text, ints, inv, out sbyte s8)) return "Enter a whole number from -128 to 127.";
                    row[Offset] = (byte)s8; return null;
                case "u8":
                    if (!byte.TryParse(text, ints, inv, out byte u8)) return "Enter a whole number from 0 to 255.";
                    row[Offset] = u8; return null;
                case "s16":
                    if (!short.TryParse(text, ints, inv, out short s16)) return "Enter a whole number from -32768 to 32767.";
                    Array.Copy(BitConverter.GetBytes(s16), 0, row, Offset, 2); return null;
                case "u16":
                    if (!ushort.TryParse(text, ints, inv, out ushort u16)) return "Enter a whole number from 0 to 65535.";
                    Array.Copy(BitConverter.GetBytes(u16), 0, row, Offset, 2); return null;
                case "s32":
                    if (!int.TryParse(text, ints, inv, out int s32)) return "Enter a whole number.";
                    Array.Copy(BitConverter.GetBytes(s32), 0, row, Offset, 4); return null;
                case "u32":
                    if (!uint.TryParse(text, ints, inv, out uint u32)) return "Enter a whole number of 0 or more.";
                    Array.Copy(BitConverter.GetBytes(u32), 0, row, Offset, 4); return null;
                case "f32":
                    if (!float.TryParse(text.Replace(',', '.'), NumberStyles.Float, inv, out float f32)) return "Enter a number, like 1.5";
                    Array.Copy(BitConverter.GetBytes(f32), 0, row, Offset, 4); return null;
                default:
                    return "This field can't be edited here.";
            }
        }

        private static ulong readUnsigned(byte[] row, int offset, int size)
        {
            ulong value = 0;
            for (int i = 0; i < size; i++)
                value |= (ulong)row[offset + i] << (8 * i);
            return value;
        }

        private static void writeUnsigned(byte[] row, int offset, int size, ulong value)
        {
            for (int i = 0; i < size; i++)
                row[offset + i] = (byte)(value >> (8 * i));
        }
    }

    /// <summary>
    /// A paramdef from Paramdex. Field layout follows SoulsFormats' rules, including how
    /// consecutive bitfields of the same type share one storage unit.
    /// </summary>
    internal class ParamDef
    {
        public string ParamType;
        public List<ParamField> Fields = new List<ParamField>();
        public int RowSize;

        private static readonly Regex fieldRx = new Regex(@"^\s*(\w+)\s+(\w+)\s*(?:\[\s*(\d+)\s*\])?\s*(?::\s*(\d+))?\s*(?:=\s*(.+?))?\s*$");

        private static readonly Dictionary<string, int> typeSizes = new Dictionary<string, int>
        {
            ["s8"] = 1, ["u8"] = 1, ["dummy8"] = 1,
            ["s16"] = 2, ["u16"] = 2,
            ["s32"] = 4, ["u32"] = 4, ["f32"] = 4,
            ["fixstr"] = 1, ["fixstrW"] = 2,
        };

        public static ParamDef Load(string path)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            ParamDef def = new ParamDef();
            def.ParamType = xml.SelectSingleNode("/PARAMDEF/ParamType")?.InnerText?.Trim();

            int offset = 0;
            int bitOffset = -1;
            string bitType = null;
            int unitOffset = 0;

            foreach (XmlNode node in xml.SelectNodes("/PARAMDEF/Fields/Field"))
            {
                string defText = node.Attributes["Def"]?.Value ?? "";
                Match m = fieldRx.Match(defText);
                if (!m.Success || !typeSizes.ContainsKey(m.Groups[1].Value))
                    throw new FormatException($"Unsupported field \"{defText}\" in {Path.GetFileName(path)}");

                string type = m.Groups[1].Value;
                string name = m.Groups[2].Value;
                int arrayLength = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 1;
                int bitSize = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : -1;
                string description = node.SelectSingleNode("Description")?.InnerText;
                string displayName = node.SelectSingleNode("DisplayName")?.InnerText;
                if (!string.IsNullOrWhiteSpace(displayName))
                    description = string.IsNullOrWhiteSpace(description) ? displayName : displayName + "\n" + description;

                ParamField field = new ParamField() { Name = name, Type = type, Description = description };

                if (bitSize <= 0)
                {
                    field.Offset = offset;
                    field.Size = typeSizes[type] * arrayLength;
                    offset += field.Size;
                    bitOffset = -1;
                }
                else
                {
                    string unitType = type == "dummy8" ? "u8" : type;
                    int unitSize = typeSizes[unitType];
                    if (unitType != bitType || bitOffset == -1 || bitOffset + bitSize > unitSize * 8)
                    {
                        bitOffset = 0;
                        bitType = unitType;
                        unitOffset = offset;
                        offset += unitSize;
                    }
                    field.Type = unitType;
                    field.Offset = unitOffset;
                    field.Size = unitSize;
                    field.BitOffset = bitOffset;
                    field.BitSize = bitSize;
                    bitOffset += bitSize;
                    if (type == "dummy8")
                        field.Type = "dummy8";
                }

                def.Fields.Add(field);
            }

            def.RowSize = offset;
            return def;
        }

        private static Dictionary<string, ParamDef> byType;

        /// <summary>
        /// All paramdefs in Resources/Params/Defs, by ParamType (e.g. EQUIP_PARAM_WEAPON_ST).
        /// </summary>
        public static Dictionary<string, ParamDef> All
        {
            get
            {
                if (byType == null)
                {
                    byType = new Dictionary<string, ParamDef>(StringComparer.OrdinalIgnoreCase);
                    string dir = Path.Combine(GetTxtResourceClass.ExeDir, "Resources", "Params", "Defs");
                    if (Directory.Exists(dir))
                    {
                        foreach (string file in Directory.GetFiles(dir, "*.xml"))
                        {
                            try
                            {
                                ParamDef def = Load(file);
                                if (!string.IsNullOrEmpty(def.ParamType))
                                    byType[def.ParamType] = def;
                            }
                            catch (Exception ex) when (ex is FormatException || ex is XmlException || ex is IOException)
                            {
                                GadgetLogger.Log("Couldn't load paramdef " + file + ": " + ex.Message);
                            }
                        }
                    }
                }
                return byType;
            }
        }
    }
}
