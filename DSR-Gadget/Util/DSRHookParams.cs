using PropertyHook;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSR_Gadget
{
    /// <summary>
    /// One param table as loaded in the game's memory.
    /// </summary>
    internal class LiveParam
    {
        public string Name;          // e.g. EquipParamWeapon
        public string NamesFile;     // Paramdex names file, e.g. EquipParamWeapon.txt
        public int SlotOffset;       // offset in SoloParamMan
        public string ParamType;     // from the param's header, e.g. EQUIP_PARAM_WEAPON_ST
        public ParamDef Def;
        public IntPtr Base;          // start of the param data in memory
        public List<LiveParamRow> Rows = new List<LiveParamRow>();
        public string Problem;       // why it can't be edited, if it can't
        public string DisplayName;   // friendly name for the dropdown

        private Dictionary<int, LiveParamRow> byId;

        public LiveParamRow FindRow(int id)
        {
            if (byId == null)
            {
                byId = new Dictionary<int, LiveParamRow>();
                foreach (LiveParamRow row in Rows)
                    if (!byId.ContainsKey(row.ID))
                        byId[row.ID] = row;
            }
            return byId.TryGetValue(id, out LiveParamRow found) ? found : null;
        }

        public override string ToString() => (DisplayName ?? Name) + (Problem == null ? "" : " (unavailable)");
    }

    internal class LiveParamRow
    {
        public int ID;
        public int DataOffset;       // from the param's base
        public string Name;

        public override string ToString() => string.IsNullOrEmpty(Name) ? ID.ToString() : ID + "  " + Name;
    }

    public partial class DSRHook
    {
        private PHPointer SoloParamManPtr;

        // Param slots in SoloParamMan, in the game's order (offsets match Phokz's table and DSROffsets.SoloParamMan)
        private static readonly (int Offset, string Name, string NamesFile)[] paramSlots =
        {
            (0x18, "EquipParamWeapon", "EquipParamWeapon"),
            (0x60, "EquipParamProtector", "EquipParamProtector"),
            (0xA8, "EquipParamAccessory", "EquipParamAccessory"),
            (0xF0, "EquipParamGoods", "EquipParamGoods"),
            (0x138, "ReinforceParamWeapon", "ReinforceParamWeapon"),
            (0x180, "ReinforceParamProtector", "ReinforceParamProtector"),
            (0x1C8, "NpcParam", "NpcParam"),
            (0x210, "AtkParam_Npc", "AtkParam_Npc"),
            (0x258, "AtkParam_Pc", "AtkParam_Pc"),
            (0x2A0, "NpcThinkParam", "NpcThinkParam"),
            (0x2E8, "ObjectParam", "ObjectParam"),
            (0x330, "Bullet", "Bullet"),
            (0x378, "BehaviorParam", "BehaviorParam"),
            (0x3C0, "BehaviorParam_PC", "BehaviorParam_PC"),
            (0x408, "Magic", "Magic"),
            (0x450, "SpEffectParam", "SpEffectParam"),
            (0x498, "SpEffectVfxParam", "SpEffectVfxParam"),
            (0x4E0, "TalkParam", "TalkParam"),
            (0x528, "MenuColorTableParam", "MenuColorTableParam"),
            (0x570, "ItemLotParam", "ItemLotParam"),
            (0x5B8, "MoveParam", "MoveParam"),
            (0x600, "CharaInitParam", "CharaInitParam"),
            (0x648, "EquipMtrlSetParam", "EquipMtrlSetParam"),
            (0x690, "FaceGenParam", "FaceGenParam"),
            (0x6D8, "RagdollParam", "RagdollParam"),
            (0x720, "ShopLineupParam", "ShopLineupParam"),
            (0x768, "QwcChange", "QwcChange"),
            (0x7B0, "QwcJudge", "QwcJudge"),
            (0x7F8, "GameAreaParam", "GameAreaParam"),
            (0x840, "SkeletonParam", "SkeletonParam"),
            (0x888, "CalcCorrectGraph", "CalcCorrectGraph"),
            (0x8D0, "LockCamParam", "LockCamParam"),
            (0x918, "ObjActParam", "ObjActParam"),
            (0x960, "HitMtrlParam", "HitMtrlParam"),
            (0x9A8, "KnockBackParam", "KnockBackParam"),
            (0x9F0, "LevelSyncParam", "LevelSyncParam"),
            (0xA38, "CoolTimeParam", "CoolTimeParam"),
            (0xA80, "WhiteCoolTimeParam", "WhiteCoolTimeParam"),
        };

        // In memory each param is its .param file: a 0x30-byte header (row count at 0xA, param type at 0xC),
        // then one 0xC-byte entry per row: ID, data offset, name offset (offsets from the start of the param)
        private const int ParamResCapData = 0x38;
        private const int ParamHeaderRowCount = 0xA;
        private const int ParamHeaderType = 0xC;
        private const int ParamRowTable = 0x30;
        private const int ParamRowEntrySize = 0xC;

        private void initParams()
        {
            SoloParamManPtr = RegisterRelativeAOB(DSROffsets.SoloParamManAOB, 3, 7, 0);
        }

        /// <summary>
        /// Finds every param table in the game's memory and reads its row list.
        /// </summary>
        internal List<LiveParam> LoadParams(Func<string, Dictionary<int, string>> rowNames)
        {
            List<LiveParam> result = new List<LiveParam>();
            IntPtr manager = SoloParamManPtr.Resolve();

            foreach (var slot in paramSlots)
            {
                LiveParam param = new LiveParam() { Name = slot.Name, NamesFile = slot.NamesFile, SlotOffset = slot.Offset };
                result.Add(param);

                if (manager == IntPtr.Zero)
                {
                    param.Problem = "The game's param manager wasn't found.";
                    continue;
                }

                IntPtr resCap = Kernel32.ReadIntPtr(Handle, manager + slot.Offset, true);
                IntPtr data = resCap == IntPtr.Zero ? IntPtr.Zero : Kernel32.ReadIntPtr(Handle, resCap + ParamResCapData, true);
                if (data == IntPtr.Zero)
                {
                    param.Problem = "This param isn't loaded right now.";
                    continue;
                }
                param.Base = data;

                byte[] header = Kernel32.ReadBytes(Handle, data, ParamRowTable);
                int rowCount = BitConverter.ToUInt16(header, ParamHeaderRowCount);
                param.ParamType = Encoding.ASCII.GetString(header, ParamHeaderType, 0x20).Split('\0')[0].Trim();

                if (!ParamDef.All.TryGetValue(param.ParamType, out param.Def))
                {
                    param.Problem = "No definition for " + param.ParamType + ".";
                    continue;
                }

                byte[] table = Kernel32.ReadBytes(Handle, data + ParamRowTable, (uint)(rowCount * ParamRowEntrySize));
                Dictionary<int, string> names = rowNames(param.NamesFile);
                for (int i = 0; i < rowCount; i++)
                {
                    int id = BitConverter.ToInt32(table, i * ParamRowEntrySize);
                    int offset = BitConverter.ToInt32(table, i * ParamRowEntrySize + 4);
                    names.TryGetValue(id, out string name);
                    param.Rows.Add(new LiveParamRow() { ID = id, DataOffset = offset, Name = name });
                }

                // Safety check: rows must sit exactly one definition-sized row apart, or the definition doesn't fit
                for (int i = 0; i + 1 < param.Rows.Count; i++)
                {
                    int gap = param.Rows[i + 1].DataOffset - param.Rows[i].DataOffset;
                    if (gap != param.Def.RowSize)
                    {
                        param.Problem = $"Row layout doesn't match its definition ({gap} vs {param.Def.RowSize} bytes).";
                        param.Rows.Clear();
                        break;
                    }
                }
            }

            return result;
        }

        internal byte[] ReadParamRow(LiveParam param, LiveParamRow row)
        {
            return Kernel32.ReadBytes(Handle, param.Base + row.DataOffset, (uint)param.Def.RowSize);
        }

        internal bool WriteParamRow(LiveParam param, LiveParamRow row, byte[] bytes)
        {
            if (bytes.Length != param.Def.RowSize)
                return false;
            return Kernel32.WriteBytes(Handle, param.Base + row.DataOffset, bytes);
        }

        /// <summary>
        /// Checks the param is still where it was when loaded (it moves if the game reloads its params).
        /// </summary>
        internal bool ParamStillValid(LiveParam param)
        {
            IntPtr manager = SoloParamManPtr.Resolve();
            if (manager == IntPtr.Zero)
                return false;
            IntPtr resCap = Kernel32.ReadIntPtr(Handle, manager + param.SlotOffset, true);
            if (resCap == IntPtr.Zero)
                return false;
            return Kernel32.ReadIntPtr(Handle, resCap + ParamResCapData, true) == param.Base;
        }
    }
}
