using DSR_Gadget.List_Items;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DSR_Gadget
{
    public partial class DSRHook : PHook
    {
        private DSROffsets Offsets;

        private PHPointer GroupMaskAddr;
        private PHPointer ChrDbgAddr;
        private PHPointer GameDataManBasePtr;
        private PHPointer ItemGetAddr;
        private PHPointer BonfireWarpAddr;

        private PHPointer CamMan;
        private PHPointer ChrClassWarp;
        private PHPointer ChrFollowCam;
        private PHPointer WorldChrBase;
        private PHPointer ChrData1;
        private PHPointer PlayerCtrl;

        private PHPointer RecentPlayersPtr;
        private PHPointer[] RecentPlayerPtrs;
        private PHPointer CurrentPlayersPtr;
        private PHPointer[] CurrentPlayerPtrs;
        private PHPointer EquipMagicDataPtr;
        private PHPointer LastBloodstainPos;
        private PHPointer SettingsPtr;
        private PHPointer GraphicsData;
        private PHPointer MenuMan;
        private PHPointer EventFlags;
        private PHPointer GameDataManPtr;

        private PHPointer DurabilityAddr;
        private PHPointer DurabilitySpecialAddr;

        private PHPointer FrpgNetManImpBase;
        private PHPointer SosDbListAddr;
        private PHPointer NitoDbListAddr;

        private PHPointer WorldChrManDbgImpAddr;

        private PHPointer LastTargetEntityAOB;
        private PHPointer LastTargetEntityPtr;

        private PHPointer LastHitEntityAOB;
        private PHPointer LastHitEntityPtr;

        private PHPointer BaseCARAOB;
        private PHPointer SosSignMan;

        private PHPointer FuncLevelUp;

        private PHPointer PosLock;


        public DSRHook(int refreshInterval, int minLifetime) :
            base(refreshInterval, minLifetime, p => p.MainWindowTitle == "DARK SOULS™: REMASTERED")
        {
            Offsets = new DSROffsets();
            CamMan = RegisterRelativeAOB(DSROffsets.CamManBaseAOB, 3, 7, DSROffsets.CamManOffset);
            ChrFollowCam = RegisterRelativeAOB(DSROffsets.ChrFollowCamAOB, 3, 7, DSROffsets.ChrFollowCamOffset1, DSROffsets.ChrFollowCamOffset2, DSROffsets.ChrFollowCamOffset3);
            GroupMaskAddr = RegisterRelativeAOB(DSROffsets.GroupMaskAOB, 2, 7);
            GraphicsData = RegisterRelativeAOB(DSROffsets.GraphicsDataAOB, 3, 7, DSROffsets.GraphicsDataOffset1, DSROffsets.GraphicsDataOffset2);
            ChrClassWarp = RegisterRelativeAOB(DSROffsets.ChrClassWarpAOB, 3, 7, DSROffsets.ChrClassWarpOffset1);
            WorldChrBase = RegisterRelativeAOB(DSROffsets.WorldChrManImpBaseAOB, 3, 7, DSROffsets.WorldChrManImpBaseOffset1);
            ChrDbgAddr = RegisterRelativeAOB(DSROffsets.ChrDbgAOB, 2, 7);
            MenuMan = RegisterRelativeAOB(DSROffsets.MenuManAOB, 3, 7, DSROffsets.MenuManOffset1);
            GameDataManBasePtr = RegisterRelativeAOB(DSROffsets.GameDataManAOB, 3, 7);
            EventFlags = RegisterRelativeAOB(DSROffsets.EventFlagsAOB, 3, 7, DSROffsets.EventFlagsOffset1, DSROffsets.EventFlagsOffset2);
            ItemGetAddr = RegisterAbsoluteAOB(DSROffsets.ItemGetAOB);
            BonfireWarpAddr = RegisterAbsoluteAOB(DSROffsets.BonfireWarpAOB);
            PosLock = RegisterAbsoluteAOB(DSROffsets.PosLockAoB);

            ChrData1 = CreateChildPointer(WorldChrBase, (int)DSROffsets.WorldChrManImp.PlayerIns);
            GameDataManPtr = CreateChildPointer(GameDataManBasePtr, DSROffsets.GameDataManOffset1);
            EquipMagicDataPtr = CreateChildPointer(GameDataManBasePtr, DSROffsets.GameDataManOffset1, (int)DSROffsets.GameDataMan.PlayerGameData, (int)DSROffsets.PlayerGameData.EquipMagicData);
            LastBloodstainPos = CreateChildPointer(GameDataManBasePtr, DSROffsets.GameDataManOffset1, (int)DSROffsets.GameDataMan.LastBloodstainPos);
            SettingsPtr = CreateChildPointer(GameDataManBasePtr, DSROffsets.GameDataManOffset1, (int)DSROffsets.GameDataMan.Settings);

            RecentPlayersPtr = CreateChildPointer(GameDataManBasePtr, DSROffsets.GameDataManOffset1, (int)DSROffsets.GameDataMan.PlayerGameDataRecent);
            RecentPlayerPtrs = new PHPointer[5];
            for (int i = 0; i < 5; i++)
            {
                RecentPlayerPtrs[i] = CreateChildPointer(RecentPlayersPtr, (int)DSROffsets.PlayerGameDataRecent.RecentPlayer1 + i * (int)DSROffsets.PlayerGameDataRecentOffset);
            }

            CurrentPlayersPtr = CreateChildPointer(ChrData1, (int)DSROffsets.PlayerIns.CurrentPlayers);
            CurrentPlayerPtrs = new PHPointer[5];
            for (int i = 0; i < 5; i++)
            {
                CurrentPlayerPtrs[i] = CreateChildPointer(CurrentPlayersPtr, (int)DSROffsets.CurrentPlayers.CurrentPlayerIns1 + i * (int)DSROffsets.CurrentPlayerOffset);
            }

            DurabilityAddr = RegisterAbsoluteAOB(DSROffsets.DurabilityAOB);
            DurabilitySpecialAddr = RegisterAbsoluteAOB(DSROffsets.DurabilitySpecialAOB);

            FrpgNetManImpBase = RegisterRelativeAOB(DSROffsets.FrpgNetManImpAOB, 3, 7, DSROffsets.FrpgNetManImpOffset1);

            SosDbListAddr = CreateChildPointer(FrpgNetManImpBase, (int)DSROffsets.FrpgNetManImp.FrpgNetSosDb, (int)DSROffsets.FrpgNetDb.NetDbList);
            NitoDbListAddr = CreateChildPointer(FrpgNetManImpBase, (int)DSROffsets.FrpgNetManImp.FrpgNetNitoInvitationDb, (int)DSROffsets.FrpgNetDb.NetDbList);

            WorldChrManDbgImpAddr = RegisterRelativeAOB(DSROffsets.WorldChrManDbgImpAOB, 3, 7, DSROffsets.WorldChrManDbgImpOffset1);

            LastTargetEntityAOB = RegisterAbsoluteAOB(DSROffsets.LastTargetEntityAOB);
            LastTargetEntityPtr = CreateBasePointer(IntPtr.Zero);

            LastHitEntityAOB = RegisterAbsoluteAOB(DSROffsets.LastHitEntityAOB);
            LastHitEntityPtr = CreateBasePointer(IntPtr.Zero);

            BaseCARAOB = RegisterRelativeAOB(DSROffsets.BaseCARAOB, 3, 7, DSROffsets.SosSignManOffset0);

            SosSignMan = CreateChildPointer(BaseCARAOB, DSROffsets.SosSignManOffset1);
            
            FuncLevelUp = RegisterAbsoluteAOB(DSROffsets.FuncLevelUpAOB);
            initExtras();
            initParams();
                
            OnHooked += DSRHook_OnHooked;
        }

        public PHPointer GetPosLock()
        {
            return PosLock;
        } 
        private void DSRHook_OnHooked(object sender, PHEventArgs e)
        {
            Offsets = DSROffsets.GetOffsets(Process.MainModule.ModuleMemorySize);
            PlayerCtrl = CreateChildPointer(ChrData1, (int)DSROffsets.PlayerIns.PlayerCtrl);
        }

        private static readonly Dictionary<int, string> VersionStrings = new Dictionary<int, string>
        {
            [0x4869400] = "1.01",
            [0x496BE00] = "1.01.1",
            [0x37CB400] = "1.01.2",
            [0x3817800] = "1.03",
        };

        public string Version
        {
            get
            {
                if (Hooked)
                {
                    int size = Process.MainModule.ModuleMemorySize;
                    if (VersionStrings.TryGetValue(size, out string version))
                        return version;
                    else
                        return $"0x{size:X8}";
                }
                else
                {
                    return "N/A";
                }
            }
        }

        public bool Loaded => ChrFollowCam.Resolve() != IntPtr.Zero;

        public bool Focused => Hooked && User32.GetForegroundProcessID() == Process.Id;

        #region Player

        public DSRPlayer GetPlayer()
        {
            return new DSRPlayer(ChrData1, DSRPlayer.PlayerDataType.PlayerIns, this);
        }

        public DSRPlayer GetEmptyPlayer()
        {
            return new DSRPlayer(CreateBasePointer(IntPtr.Zero), DSRPlayer.PlayerDataType.PlayerIns, this);
        }

        public int[][] EquipMagicData
        {
            get
            {
                int[] magicData = new int[12];
                int[] magicQuantity = new int[12];
                for (int i = 0; i < 12; i++)
                {
                    magicData[i] = EquipMagicDataPtr.ReadInt32((int)DSROffsets.EquipMagicData.AttunementSlot1 + i * 8);
                    magicQuantity[i] = EquipMagicDataPtr.ReadInt32((int)DSROffsets.EquipMagicData.Quantity1 + i * 8);
                }

                int[][] equipMagicData = new int[2][];
                equipMagicData[0] = magicData;
                equipMagicData[1] = magicQuantity;
                return equipMagicData;
            }

            set
            {
                for (int i = 0; i < 12; i++)
                {
                    int id = EquipMagicDataPtr.ReadInt32((int)DSROffsets.EquipMagicData.AttunementSlot1 + i * 8);
                    if (id == value[0][i])
                    {
                        EquipMagicDataPtr.WriteInt32((int)DSROffsets.EquipMagicData.Quantity1 + i * 8, value[1][i]);
                    }
                    /*
                    EquipMagicDataPtr.WriteInt32((int)DSROffsets.EquipMagicData.AttunementSlot1 + i * 8, value[0][i]);
                    */
                }
            }
        }

        public void ResetMagicQuantity()
        {
            for (int i = 0; i < 12; i++)
            {
                int id = EquipMagicDataPtr.ReadInt32((int)DSROffsets.EquipMagicData.AttunementSlot1 + i * 8);
                EquipMagicDataPtr.WriteInt32((int)DSROffsets.EquipMagicData.Quantity1 + i * 8, DSRMagic.Dictionary[id]);
            }
        }

        public DSRPlayer.Position GetStablePosition()
        {
            DSRPlayer.Position pos;
            pos.X = ChrClassWarp.ReadSingle((int)DSROffsets.ChrClassWarp.StableX);
            pos.Y = ChrClassWarp.ReadSingle((int)DSROffsets.ChrClassWarp.StableY);
            pos.Z = ChrClassWarp.ReadSingle((int)DSROffsets.ChrClassWarp.StableZ);
            pos.Angle = ChrClassWarp.ReadSingle((int)DSROffsets.ChrClassWarp.StableAngle);
            return pos;
        }

        public DSRPlayer.Position GetInitialPosition()
        {
            DSRPlayer.Position pos;
            pos.X = ChrClassWarp.ReadSingle((int)DSROffsets.ChrClassWarp.InitialX);
            pos.Y = ChrClassWarp.ReadSingle((int)DSROffsets.ChrClassWarp.InitialY);
            pos.Z = ChrClassWarp.ReadSingle((int)DSROffsets.ChrClassWarp.InitialZ);
            pos.Angle = ChrClassWarp.ReadSingle((int)DSROffsets.ChrClassWarp.InitialAngle);
            return pos;
        }

        public DSRPlayer.Position GetLastBloodstainPosition()
        {
            DSRPlayer.Position pos;
            pos.X = LastBloodstainPos.ReadSingle((int)DSROffsets.LastBloodstainPos.PosX);
            pos.Y = LastBloodstainPos.ReadSingle((int)DSROffsets.LastBloodstainPos.PosY);
            pos.Z = LastBloodstainPos.ReadSingle((int)DSROffsets.LastBloodstainPos.PosZ);
            pos.Angle = LastBloodstainPos.ReadSingle((int)DSROffsets.LastBloodstainPos.PosAngle);
            return pos;
        }

        public bool NoGravity
        {
            set => ChrData1.WriteFlag32((int)DSROffsets.PlayerIns.ChrFlags1, (uint)DSROffsets.ChrFlags1.NoGravity, value);
        }

        public bool NoCollision
        {
            set => PlayerCtrl.WriteFlag32((int)DSROffsets.PlayerCtrl.ChrMapFlags, (uint)DSROffsets.ChrMapFlags.DisableMapHit, value);
        }

        public bool AutoSave
        {
            get => ChrClassWarp.ReadBoolean((int)DSROffsets.ChrClassWarp.AutoSave);
            set => ChrClassWarp.WriteBoolean((int)DSROffsets.ChrClassWarp.AutoSave, value);
        }

        public bool DeathCam
        {
            get => WorldChrBase.ReadBoolean((int)DSROffsets.WorldChrManImp.DeathCam);
            set => WorldChrBase.WriteBoolean((int)DSROffsets.WorldChrManImp.DeathCam, value);
        }

        public int LastBonfire
        {
            get => ChrClassWarp.ReadInt32((int)DSROffsets.ChrClassWarp.LastBonfire);
            set => ChrClassWarp.WriteInt32((int)DSROffsets.ChrClassWarp.LastBonfire, value);
        }

        public void BonfireWarp()
        {
            byte[] asm = (byte[])DSRAssembly.BonfireWarp.Clone();
            byte[] bytes = BitConverter.GetBytes(GameDataManBasePtr.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x2, 8);
            bytes = BitConverter.GetBytes(BonfireWarpAddr.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x18, 8);
            Execute(asm);
        }

        public byte[] DumpFollowCam()
        {
            return ChrFollowCam.ReadBytes(0, 512);
        }

        public void UndumpFollowCam(byte[] value)
        {
            ChrFollowCam.WriteBytes(0, value);
        }
        #endregion

        #region Stats

        public byte ClearCount
        {
            get => GameDataManPtr.ReadByte((int)DSROffsets.GameDataMan.ClearCount);
            set => GameDataManPtr.WriteByte((int)DSROffsets.GameDataMan.ClearCount, value);
        }

        public void LevelUp(int level, int vit, int att, int end, int str, int dex, int res, int intel, int fth, int humanity, int souls)
        {
            IntPtr stats = Allocate(0x300);
            
            Kernel32.WriteBytes(Handle, stats + 0x270 + 0x0, BitConverter.GetBytes(vit));
            Kernel32.WriteBytes(Handle, stats + 0x270 + 0x4, BitConverter.GetBytes(att));
            Kernel32.WriteBytes(Handle, stats + 0x270 + 0x8, BitConverter.GetBytes(end));
            Kernel32.WriteBytes(Handle, stats + 0x270 + 0xC, BitConverter.GetBytes(str));
            Kernel32.WriteBytes(Handle, stats + 0x270 + 0x10, BitConverter.GetBytes(dex));
            Kernel32.WriteBytes(Handle, stats + 0x270 + 0x14, BitConverter.GetBytes(res));
            Kernel32.WriteBytes(Handle, stats + 0x270 + 0x18, BitConverter.GetBytes(intel));
            Kernel32.WriteBytes(Handle, stats + 0x270 + 0x1C, BitConverter.GetBytes(fth));
            Kernel32.WriteBytes(Handle, stats + 0x270 + 0x20, BitConverter.GetBytes(humanity));
            Kernel32.WriteBytes(Handle, stats + 0x270 + 0x24, BitConverter.GetBytes(level));
            Kernel32.WriteBytes(Handle, stats + 0x270 + 0x28, BitConverter.GetBytes(level)); // should be level as well?
            Kernel32.WriteBytes(Handle, stats + 0x270 + 0x2C, BitConverter.GetBytes(0));
            Kernel32.WriteBytes(Handle, stats + 0x270 + 0x30, BitConverter.GetBytes(souls));

            byte[] asm = (byte[])DSRAssembly.LevelUp.Clone();
            byte[] bytes = BitConverter.GetBytes(stats.ToInt64() + 0x270);
            Array.Copy(bytes, 0, asm, 0x2, 8);
            bytes = BitConverter.GetBytes(stats.ToInt64());
            Array.Copy(bytes, 0, asm, 0xC, 8);
            bytes = BitConverter.GetBytes(FuncLevelUp.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x1A, 8);

            Execute(asm);
            Free(stats);
        }

        #endregion

        #region Items
        public void GetItem(int category, int id, int quantity)
        {
            byte[] asm = (byte[])DSRAssembly.GetItem.Clone();

            byte[] bytes = BitConverter.GetBytes(category);
            Array.Copy(bytes, 0, asm, 0x1, 4);
            bytes = BitConverter.GetBytes(quantity);
            Array.Copy(bytes, 0, asm, 0x7, 4);
            bytes = BitConverter.GetBytes(id);
            Array.Copy(bytes, 0, asm, 0xD, 4);
            bytes = BitConverter.GetBytes((ulong)GameDataManBasePtr.Resolve());
            Array.Copy(bytes, 0, asm, 0x19, 8);
            bytes = BitConverter.GetBytes((ulong)ItemGetAddr.Resolve());
            Array.Copy(bytes, 0, asm, 0x46, 8);

            Execute(asm);
        }
        #endregion

        #region Cheats
        public bool PlayerDeadMode
        {
            get => ChrData1.ReadFlag32((int)DSROffsets.PlayerIns.ChrFlags1, (uint)DSROffsets.ChrFlags1.SetDeadMode);
            set => ChrData1.WriteFlag32((int)DSROffsets.PlayerIns.ChrFlags1, (uint)DSROffsets.ChrFlags1.SetDeadMode, value);
        }

        public bool PlayerNoDead
        {
            set => ChrDbgAddr.WriteBoolean((int)DSROffsets.ChrDbg.PlayerNoDead, value);
        }

        public bool PlayerDisableDamage
        {
            set => ChrData1.WriteFlag32((int)DSROffsets.PlayerIns.ChrFlags1, (uint)DSROffsets.ChrFlags1.DisableDamage, value);
        }

        public bool PlayerNoHit
        {
            set => ChrData1.WriteFlag32((int)DSROffsets.PlayerIns.ChrFlags2, (uint)DSROffsets.ChrFlags2.NoHit, value);
        }

        public bool PlayerNoStamina
        {
            set => ChrData1.WriteFlag32((int)DSROffsets.PlayerIns.ChrFlags2, (uint)DSROffsets.ChrFlags2.NoStaminaConsumption, value);
        }

        public bool PlayerSuperArmor
        {
            set => ChrData1.WriteFlag32((int)DSROffsets.PlayerIns.ChrFlags1, (uint)DSROffsets.ChrFlags1.SetSuperArmor, value);
        }

        public bool PlayerHide
        {
            set => ChrDbgAddr.WriteBoolean((int)DSROffsets.ChrDbg.PlayerHide, value);
        }

        public bool PlayerSilence
        {
            set => ChrDbgAddr.WriteBoolean((int)DSROffsets.ChrDbg.PlayerSilence, value);
        }

        public bool PlayerExterminate
        {
            set => ChrDbgAddr.WriteBoolean((int)DSROffsets.ChrDbg.PlayerExterminate, value);
        }

        public bool PlayerNoGoods
        {
            set => ChrData1.WriteFlag32((int)DSROffsets.PlayerIns.ChrFlags2, (uint)DSROffsets.ChrFlags2.NoGoodsConsume, value);
        }

        public bool AllNoArrow
        {
            set => ChrDbgAddr.WriteBoolean((int)DSROffsets.ChrDbg.AllNoArrowConsume, value);
        }

        public bool AllNoMagicQty
        {
            set => ChrDbgAddr.WriteBoolean((int)DSROffsets.ChrDbg.AllNoMagicQtyConsume, value);
        }

        public bool AllNoDead
        {
            set => ChrDbgAddr.WriteBoolean((int)DSROffsets.ChrDbg.AllNoDead, value);
        }

        public bool AllNoDamage
        {
            set => ChrDbgAddr.WriteBoolean((int)DSROffsets.ChrDbg.AllNoDamage, value);
        }

        public bool AllNoHit
        {
            set => ChrDbgAddr.WriteBoolean((int)DSROffsets.ChrDbg.AllNoHit, value);
        }

        public bool AllNoStamina
        {
            set => ChrDbgAddr.WriteBoolean((int)DSROffsets.ChrDbg.AllNoStaminaConsume, value);
        }

        public bool AllNoAttack
        {
            set => ChrDbgAddr.WriteBoolean((int)DSROffsets.ChrDbg.AllNoAttack, value);
        }

        public bool AllNoMove
        {
            set => ChrDbgAddr.WriteBoolean((int)DSROffsets.ChrDbg.AllNoMove, value);
        }

        public bool AllNoUpdateAI
        {
            set => ChrDbgAddr.WriteBoolean((int)DSROffsets.ChrDbg.AllNoUpdateAI, value);
        }

        private byte[] DurabilityOnBytes = { 0x41, 0x8D, 0x01, 0x90, 0x90, 0x90, 0x90};
        private byte[] DurabilityOffBytes = { 0x41, 0x8D, 0x41, 0xFF, 0x45, 0x8B, 0xC3 };
        public bool Durability
        {
            set
            {
                if (value)
                {
                    DurabilityAddr.WriteBytes(0x0, DurabilityOnBytes);
                }
                else
                {
                    DurabilityAddr.WriteBytes(0x0, DurabilityOffBytes);
                }

            }

            get
            {
                return DurabilityAddr.ReadBytes(0, 7).Equals(DurabilityOnBytes);
            }
        }

        private byte[] DurabilitySpecialOnBytes = { 0x49, 0x03, 0x49, 0x38, 0x90, 0x90, 0x90, 0x90};
        private byte[] DurabilitySpecialOffBytes = { 0x49, 0x03, 0x49, 0x38, 0x44, 0x89, 0x41, 0x14 };
        public bool DurabilitySpecial
        {
            set
            {
                if (value)
                {
                    DurabilitySpecialAddr.WriteBytes(0x0, DurabilitySpecialOnBytes);
                }
                else
                {
                    DurabilitySpecialAddr.WriteBytes(0x0, DurabilitySpecialOffBytes);
                }

            }

            get
            {
                return DurabilitySpecialAddr.ReadBytes(0, 8).Equals(DurabilitySpecialOnBytes);
            }
        }


        #endregion

        #region Graphics
        public bool DrawMap
        {
            set => GroupMaskAddr.WriteBoolean((int)DSROffsets.GroupMask.Map, value);
        }

        public bool DrawObjects
        {
            set => GroupMaskAddr.WriteBoolean((int)DSROffsets.GroupMask.Objects, value);
        }

        public bool DrawCharacters
        {
            set => GroupMaskAddr.WriteBoolean((int)DSROffsets.GroupMask.Characters, value);
        }

        public bool DrawSFX
        {
            set => GroupMaskAddr.WriteBoolean((int)DSROffsets.GroupMask.SFX, value);
        }

        public bool DrawCutscenes
        {
            set => GroupMaskAddr.WriteBoolean((int)DSROffsets.GroupMask.Cutscenes, value);
        }

        public bool FilterOverride
        {
            set => GraphicsData.WriteBoolean((int)DSROffsets.GraphicsData.FilterOverride, value);
        }

        public void SetFilterValues(float brightR, float brightG, float brightB, float contR, float contG, float contB, float saturation, float hue)
        {
            GraphicsData.WriteSingle((int)DSROffsets.GraphicsData.FilterBrightnessR, brightR);
            GraphicsData.WriteSingle((int)DSROffsets.GraphicsData.FilterBrightnessG, brightG);
            GraphicsData.WriteSingle((int)DSROffsets.GraphicsData.FilterBrightnessB, brightB);
            GraphicsData.WriteSingle((int)DSROffsets.GraphicsData.FilterContrastR, contR);
            GraphicsData.WriteSingle((int)DSROffsets.GraphicsData.FilterContrastG, contG);
            GraphicsData.WriteSingle((int)DSROffsets.GraphicsData.FilterContrastB, contB);
            GraphicsData.WriteSingle((int)DSROffsets.GraphicsData.FilterSaturation, saturation);
            GraphicsData.WriteSingle((int)DSROffsets.GraphicsData.FilterHue, hue);
        }
        #endregion

        #region Misc
        private static Dictionary<string, int> eventFlagGroups = new Dictionary<string, int>()
        {
            {"0", 0x00000},
            {"1", 0x00500},
            {"5", 0x05F00},
            {"6", 0x0B900},
            {"7", 0x11300},
        };

        private static Dictionary<string, int> eventFlagAreas = new Dictionary<string, int>()
        {
            {"000", 00},
            {"100", 01},
            {"101", 02},
            {"102", 03},
            {"110", 04},
            {"120", 05},
            {"121", 06},
            {"130", 07},
            {"131", 08},
            {"132", 09},
            {"140", 10},
            {"141", 11},
            {"150", 12},
            {"151", 13},
            {"160", 14},
            {"170", 15},
            {"180", 16},
            {"181", 17},
        };

        private int getEventFlagOffset(int ID, out uint mask)
        {
            string idString = ID.ToString("D8");
            if (idString.Length == 8)
            {
                string group = idString.Substring(0, 1);
                string area = idString.Substring(1, 3);
                int section = Int32.Parse(idString.Substring(4, 1));
                int number = Int32.Parse(idString.Substring(5, 3));

                if (eventFlagGroups.ContainsKey(group) && eventFlagAreas.ContainsKey(area))
                {
                    int offset = eventFlagGroups[group];
                    offset += eventFlagAreas[area] * 0x500;
                    offset += section * 128;
                    offset += (number - (number % 32)) / 8;

                    mask = 0x80000000 >> (number % 32);
                    return offset;
                }
            }
            throw new ArgumentException("Unknown event flag ID: " + ID);
        }

        public bool ReadEventFlag(int ID)
        {
            int offset = getEventFlagOffset(ID, out uint mask);
            return EventFlags.ReadFlag32(offset, mask);
        }

        public void WriteEventFlag(int ID, bool state)
        {
            int offset = getEventFlagOffset(ID, out uint mask);
            EventFlags.WriteFlag32(offset, mask, state);
        }

        public bool LastTargetEntity
        {
            set
            {
                if (value)
                    EnableTargetEntity();
                else
                    DisableTargetEntity();
            }
            get
            {
                return LastTargetEntityPtr.Resolve() != IntPtr.Zero;
            }
        }     
        
        public bool LastHitEntity
        {
            set
            {
                if (value)
                    EnableHitEntity();
                else
                    DisableHitEntity();
            }
            get
            {
                return LastHitEntityPtr.Resolve() != IntPtr.Zero;
            }
        }

        public PHPointer GetLastTargetEntity()
        {
            return CreateBasePointer(LastTargetEntityPtr.Resolve() + DSROffsets.LastTargetEntityAsmOffset, DSROffsets.EnemyInsOffset1);
        }

        private void DisableTargetEntity()
        {
            if (LastTargetEntityAOB.Resolve() != IntPtr.Zero && LastTargetEntityPtr.Resolve() != IntPtr.Zero)
            {
                RemoveHook(DSROffsets.LastTargetEntityAOBBytes, LastTargetEntityAOB.Resolve(), LastTargetEntityPtr.Resolve());
                LastTargetEntityPtr = CreateBasePointer(IntPtr.Zero);
            }
        }

        private void EnableTargetEntity()
        {
            if (LastTargetEntityAOB.Resolve() != IntPtr.Zero)
            {
                byte[] asm = (byte[])DSRAssembly.TargetedEntity.Clone();
                IntPtr asmAddr = InjectHook(asm, LastTargetEntityAOB.Resolve(), 0xD, (IntPtr)asm.Length + 16);
                if (asmAddr != IntPtr.Zero)
                {
                    LastTargetEntityAOB.WriteByte(5, 0x90);
                    LastTargetEntityPtr = CreateBasePointer(asmAddr);
                }
            }
        }

        public PHPointer GetLastHitEntity()
        {
            return CreateBasePointer(LastHitEntityPtr.Resolve() + DSROffsets.LastHitEntityAsmOffset, DSROffsets.EnemyInsOffset1);
        }

        private void DisableHitEntity()
        {
            if (LastHitEntityAOB.Resolve() != IntPtr.Zero && LastHitEntityPtr.Resolve() != IntPtr.Zero)
            {
                RemoveHook(DSROffsets.LastHitEntityAOBBytes, LastHitEntityAOB.Resolve(), LastHitEntityPtr.Resolve());
                LastTargetEntityPtr = CreateBasePointer(IntPtr.Zero);
            }
        }

        private void EnableHitEntity()
        {
            if (LastHitEntityAOB.Resolve() != IntPtr.Zero)
            {
                byte[] asm = (byte[])DSRAssembly.TargetedEntity.Clone();
                IntPtr asmAddr = InjectHook(asm, LastHitEntityAOB.Resolve(), 0xD, (IntPtr)asm.Length + 16);
                if (asmAddr != IntPtr.Zero)
                {
                    LastHitEntityAOB.WriteByte(5, 0x90);
                    LastHitEntityPtr = CreateBasePointer(asmAddr);
                }
            }
        }

        public void PerformGesture(int gesture)
        {
            MenuMan.WriteInt32((int)DSROffsets.MenuMan.PerformGesture, gesture);
        }

        #endregion

        #region Hotkeys
        public void MenuKick()
        {
            MenuMan.WriteInt32((int)DSROffsets.MenuMan.MenuKick, 2);
        }

        public void StandUp()
        {
            ChrData1.WriteInt32((int)DSROffsets.PlayerIns.ForcePlayAnimation1,0); // -1 or w/e
        }

        public bool HUD
        {
            get => SettingsPtr.ReadBoolean((int)DSROffsets.Settings.HUD);
            set => SettingsPtr.WriteBoolean((int)DSROffsets.Settings.HUD, value);
        }

#if DEBUG
        public void HotkeyTest1()
        {

        }

        public void HotkeyTest2()
        {

        }
#endif
        #endregion

        #region Net

        public bool[] GetRecentPlayers()
        {
            bool[] recentPlayers = new bool[5];
            for (int i = 0; i < RecentPlayerPtrs.Length; i++)
            {
                //TODO: more robust check
                if (RecentPlayerPtrs[i].ReadInt32((int)DSROffsets.PlayerGameData.SoulLevel) != 0)
                    recentPlayers[i] = true;
                else
                    recentPlayers[i] = false;
            }

            return recentPlayers;
        }

        public bool[] GetCurrentPlayers()
        {
            bool[] currentPlayers = new bool[5];
            for (int i = 0; i < RecentPlayerPtrs.Length; i++)
            {
                //TODO: more robust check
                if (CurrentPlayerPtrs[i].Resolve() != IntPtr.Zero)
                    currentPlayers[i] = true;
                else
                    currentPlayers[i] = false;
            }

            return currentPlayers;
        }

/*
        public Dictionary<int, DSRSign> GetSigns()
        {
            Dictionary<int, DSRSign> signs = new Dictionary<int, DSRSign>();

            foreach (IDSRSosSignManItem item in GetSosSignManItems())
            {
                signs.Add(item.ID, new DSRSign() { DSRSosSignManItem = item });
            }

            DSRSign sign;
            foreach (IDSRNetDbItem item in GetDSRNetDbItems())
            {
                if (signs.TryGetValue(item.ID, out sign))
                    sign.DSRNetDbItem = item;
            }

            return signs;
        }

        public List<IDSRSosSignManItem> GetSosSignManItems()
        {
            List<IDSRSosSignManItem> dsrSosSignManItems = new List<IDSRSosSignManItem>();

            PHPointer SosSignManList = CreateChildPointer(SosSignMan, (int)DSROffsets.SosSignMan.SosListEntry);
            Dictionary<IntPtr, PHPointer> sosPtrs = new Dictionary<IntPtr, PHPointer>();

            sosPtrs = GetSignSfx(SosSignManList, SosSignManList, sosPtrs);

            foreach (KeyValuePair<IntPtr, PHPointer> keyValuePair in sosPtrs)
                dsrSosSignManItems.Add(new DSRSosSignManItem(keyValuePair.Value, this));

            return dsrSosSignManItems;
        }

        public List<IDSRNetDbItem> GetDSRNetDbItems()
        {
            List<IDSRNetDbItem> items = new List<IDSRNetDbItem>();
            items.AddRange(GetDSRSosDbItems());
            items.AddRange(GetDSRNitoDbItems());

            return items;
        }

        public List<DSRSosDbItem> GetDSRSosDbItems()
        {
            List<DSRSosDbItem> dsrSosDbItems = new List<DSRSosDbItem>();

            PHPointer frpgSosDbListItem = CreateChildPointer(SosDbListAddr, (int)DSROffsets.NetDbList.NetDbListItem);

            bool loop = true;
            while (loop)
            {
                PHPointer frpgSosDbItem = CreateChildPointer(frpgSosDbListItem, (int)DSROffsets.NetDbListItem.FrpgNetDbItem);

                if (frpgSosDbListItem.Resolve() != SosDbListAddr.Resolve())
                {
                    dsrSosDbItems.Add(new DSRSosDbItem(frpgSosDbItem, this));
                    frpgSosDbListItem = CreateChildPointer(frpgSosDbListItem, (int)DSROffsets.NetDbListItem.NetDbListItemNext);
                }
                else
                    loop = false;
            }

            return dsrSosDbItems;
        }

        public List<DSRNitoDbItem> GetDSRNitoDbItems()
        {
            List<DSRNitoDbItem> dsrNitoDbItems = new List<DSRNitoDbItem>();

            PHPointer frpgNitoDbListItem = CreateChildPointer(NitoDbListAddr, (int)DSROffsets.NetDbList.NetDbListItem);

            bool loop = true;
            while (loop)
            {
                PHPointer frpgNitoDbItem = CreateChildPointer(frpgNitoDbListItem, (int)DSROffsets.NetDbListItem.FrpgNetDbItem);

                if (frpgNitoDbListItem.Resolve() != SosDbListAddr.Resolve())
                {
                    dsrNitoDbItems.Add(new DSRNitoDbItem(frpgNitoDbItem, this));
                    frpgNitoDbListItem = CreateChildPointer(frpgNitoDbListItem, (int)DSROffsets.NetDbListItem.NetDbListItemNext);
                }
                else
                    loop = false;
            }

            return dsrNitoDbItems;
        }*/

        public List<DSRSummonSign> GetSummonSigns()
        {
            List<DSRSummonSign> summonSignList = new List<DSRSummonSign>();

            PHPointer frpgSosDbListItem = CreateChildPointer(SosDbListAddr, (int)DSROffsets.NetDbList.NetDbListItem);

            bool loop = true;
            while (loop)
            {
                PHPointer frpgSosDbItem = CreateChildPointer(frpgSosDbListItem, (int)DSROffsets.NetDbListItem.FrpgNetDbItem);

                if (frpgSosDbListItem.Resolve() != SosDbListAddr.Resolve())
                {
                    summonSignList.Add(new DSRSummonSign(frpgSosDbItem, this));
                    frpgSosDbListItem = CreateChildPointer(frpgSosDbListItem, (int)DSROffsets.NetDbListItem.NetDbListItemNext);
                }
                else
                    loop = false;
            }

            return summonSignList;
        }

        // seems to be stored in some kind of strange tree or graph structure
        public List<DSRSummonSignSfx> GetSummonSignsSfx()
        {
            List<DSRSummonSignSfx> summonSignList = new List<DSRSummonSignSfx>();

            PHPointer SosSignManList = CreateChildPointer(SosSignMan, (int)DSROffsets.SosSignMan.SosListEntry);
            Dictionary<IntPtr, PHPointer> sosPtrs = new Dictionary<IntPtr, PHPointer>();

            sosPtrs = GetSignSfx(SosSignManList, SosSignManList, sosPtrs);

            foreach (KeyValuePair<IntPtr, PHPointer> keyValuePair in sosPtrs)
                summonSignList.Add(new DSRSummonSignSfx(keyValuePair.Value, this));

            return summonSignList;
        }

        private Dictionary<IntPtr, PHPointer> GetSignSfx(PHPointer sosListStartEntry, PHPointer sosListEntry, Dictionary<IntPtr, PHPointer> dict)
        {


            PHPointer sos = CreateChildPointer(sosListEntry, (int)DSROffsets.SosListEntry.SosSignManSign);
            IntPtr sosPtr = sos.Resolve();

            if (dict.ContainsKey(sosPtr))
                return dict;
            else if (sosPtr != IntPtr.Zero && sosListStartEntry.Resolve() != sosListEntry.Resolve())
                dict.Add(sosPtr, sos);

            PHPointer item1 = CreateChildPointer(sosListEntry, (int)DSROffsets.SosListEntry.Item1);
            PHPointer item2 = CreateChildPointer(sosListEntry, (int)DSROffsets.SosListEntry.Item2);
            PHPointer item3 = CreateChildPointer(sosListEntry, (int)DSROffsets.SosListEntry.Item3);

            if (item1.Resolve() != sosListStartEntry.Resolve())
                dict = GetSignSfx(sosListStartEntry, item1, dict);
            if (item2.Resolve() != sosListStartEntry.Resolve())
                dict = GetSignSfx(sosListStartEntry, item2, dict);
            if (item3.Resolve() != sosListStartEntry.Resolve())
                dict = GetSignSfx(sosListStartEntry, item3, dict);



            return dict;
        }

        public DSRPlayer GetCurrentPlayer(int index)
        {
            return new DSRPlayer(CurrentPlayerPtrs[index], DSRPlayer.PlayerDataType.PlayerIns, this);
        }

        public DSRPlayer GetRecentPlayer(int index)
        {
            return new DSRPlayer(RecentPlayerPtrs[index], DSRPlayer.PlayerDataType.PlayerGameData, this);
        }

        public DSRSummonSign GetEmptySummonSign()
        {
            return new DSRSummonSign(CreateBasePointer(IntPtr.Zero), this);
        }

        public DSRSummonSignSfx GetEmptySummonSignSfx()
        {
            return new DSRSummonSignSfx(CreateBasePointer(IntPtr.Zero), this);
        }

        public void LeaveSession()
        {
            byte[] asm = (byte[])DSRAssembly.LeaveSession.Clone();
            Execute(asm);
        }
        
        public void KickPlayer(DSRPlayer player, byte index)
        {
            index += 1;
            byte[] asm = (byte[])DSRAssembly.KickPlayer.Clone();
            byte[] bytes = BitConverter.GetBytes(0x10044000 + index);
            Array.Copy(bytes, 0, asm, 0x1, 4);
            Execute(asm);
        }

        public void TriggerSign(DSRSummonSignSfx sign)
        {
            if (SosSignMan.Resolve() == IntPtr.Zero || sign.SummonSignPtr.Resolve() == IntPtr.Zero)
                return;

            byte[] asm = (byte[])DSRAssembly.TriggerSign.Clone();
            byte[] sosSignManAddr = BitConverter.GetBytes(SosSignMan.Resolve().ToInt64());
            byte[] signAddr = BitConverter.GetBytes(sign.SummonSignPtr.Resolve().ToInt64());
            Array.Copy(signAddr, 0, asm, 0x2, 8);
            Array.Copy(sosSignManAddr, 0, asm, 0xC, 8);
            Execute(asm);
        }

        public void SetCamera(IntPtr ptr)
        {
            WorldChrManDbgImpAddr.WriteUInt64((int)DSROffsets.WorldChrManDbgImp.Camera, (ulong)ptr);
        }

        #endregion
    }
}
