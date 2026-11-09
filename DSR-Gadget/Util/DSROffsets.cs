using System;
using System.Collections.Generic;

namespace DSR_Gadget
{
    internal class DSROffsets
    {
        // BaseE, network stuff
        public const string FrpgNetManImpAOB = "48 8B 05 ? ? ? ? 48 8B 88 50 0B 00 00 0F B6 41 4A C3 48 C7 45 48 01 00 00 00";
        public const int FrpgNetManImpOffset1 = 0x0;

        public enum FrpgNetManImp
        {
            FrpgNetSosDb = 0xB48,
            FrpgNetNitoInvitationDb = 0xB98,
        }
        
        public enum FrpgNetDb
        {
            NetDbList = 0x28,
        }

        public enum NetDbList
        {
            NetDbListItem = 0x0,
        }

        public enum NetDbListItem
        {
            NetDbListItemNext = 0x0,
            NetDbListItemPrevious = 0x8,
            FrpgNetDbItem = 0x10,
        }

        public enum FrpgNetSosDbItem
        {
            SummonType = 0x8,
            SteamID = 0xC,
            AreaID1 = 0x30,
            PosX = 0x34,
            PosY = 0x38,
            PosZ = 0x3C,
            PosAngle = 0x40,
            AreaID2 = 0x44,
            SoulLevel = 0x48,
            Covenant = 0x4C,
            MultiplayerCount = 0x50,

            LeftWep1 = 0x54,
            RightWep1 = 0x58,
            LeftWep2 = 0x5C,
            RightWep2 = 0x60,
            Arrow1 = 0x64,
            Bolt1 = 0x68,
            Arrow2 = 0x6C,
            Bolt2 = 0x70,
            ArmorHead = 0x74,
            ArmorChest = 0x78,
            ArmorHands = 0x7C,
            ArmorLegs = 0x80,
            Hair = 0x84,

            HairRed = 0x88,
            HairGreen = 0x89,
            HairBlue = 0x8A,
            HairAlpha = 0x8B,

            Name = 0x94,

        }

        // BaseX in public ce table
        public const string WorldChrManImpBaseAOB = "48 8B 05 ? ? ? ? 48 8B 48 68 48 85 C9 0F 84 ? ? ? ? 48 39 5E 10 0F 84 ? ? ? ? 48";
        public const int WorldChrManImpBaseOffset1 = 0x0;
        public enum WorldChrManImp
        {
            PlayerIns = 0x68, // Main Player data
            DeathCam = 0x70,
        }

        public enum PlayerIns
        {
            CurrentPlayers = 0x18, // List of PlayerIns of current players
            ChrRes = 0x10,
            PlayerCtrl = 0x68,
            ChrType = 0xD4, //74
            TeamType = 0xD8, //78
            ForcePlayAnimation1 = 0x16C,
            ChrFlags1 = 0x2A4, //1FC
            Opactiy = 0x328,
            MPAreaID = 0x354,
            AreaID = 0x358,
            Health = 0x3E8,
            MaxHealth = 0x3EC,
            Stamina = 0x3F8,
            MaxStamina = 0x3FC,
            ThrowData = 0x448,
            ChrFlags2 = 0x524,

            PlayerGameData = 0x578,
            SteamPlayerData = 0x590,
        }

        public enum ChrRes
        {
            Timer = 0x20,
        }

        public enum SteamPlayerData
        {
            SteamOnlineIDData = 0x18,
            Name = 0x58,
        }

        public enum SteamOnlineIDData
        {
            SteamID64 = 0x50,
        }

        public enum PlayerCtrl
        {
            ChrAnimData = 0x18,
            ChrPosData = 0x28,
            ActionCtrl = 0x48,
            ChrMapFlags = 0x104,
            Warp = 0x108,
            WarpX = 0x110,
            WarpY = 0x114,
            WarpZ = 0x118,
            WarpAngle = 0x124,
        }

        public enum ActionCtrl
        {
            CurrentAnimation = 0x80,
        }

        public enum ChrAnimData
        {
            AnimSpeed = 0xA8,
        }

        public enum ChrPosData
        {
            PosAngle = 0x4,
            PosX = 0x10,
            PosY = 0x14,
            PosZ = 0x18,
        }

        [Flags]
        public enum ChrMapFlags : uint
        {
            DisableMapHit = 0x00000010,
        }

        [Flags]
        public enum ChrFlags1
        {
            ForceSetOmission = 0x00000001,
            DisableHPGauge = 0x00000008,
            SetEventGenerate = 0x00000010,
            ForcePlayAnimation = 0x00000100,
            ForceUpdateNextFrame = 0x00000200,
            NoGravity = 0x00004000,
            SetSuperArmor = 0x00010000,
            FirstPerson = 0x00100000,
            ToggleDraw = 0x00800000,
            // Can't die, but still take damage from killboxes and trigger deathcams
            SetDeadMode = 0x02000000,
            // Doesn't prevent healing
            DisableDamage = 0x04000000,
            // Super armor and disable damage, still die to killbox
            EnableInvincible = 0x08000000,
        }

        [Flags]
        public enum ChrFlags2
        {
            DrawHit = 0x00000004,
            // Can't die, killboxes and death cams do nothing
            NoDead = 0x00000020,
            // Also prevents healing
            NoDamage = 0x00000040,
            NoHit = 0x00000080,
            NoAttack = 0x00000100,
            NoMove = 0x00000200,
            NoStaminaConsumption = 0x00000400,
            NoMpConsumption = 0x00000800,
            DrawDirection = 0x00004000,
            NoUpdate = 0x00008000,
            DrawCounter = 0x00200000,
            NoGoodsConsume = 0x01000000,
        }

        public const int CurrentPlayerOffset = 0x38;
        public enum CurrentPlayers
        {
            CurrentPlayerIns1 = 0x38,
            CurrentPlayerIns2 = 0x70,
            CurrentPlayerIns3 = 0xA8,
            CurrentPlayerIns4 = 0xE0,
            CurrentPlayerIns5 = 0x118,
        }



        public const string CamManBaseAOB = "48 8B 05 ? ? ? ? 48 63 D1 48 8B 44 D0 08 C3";
        public const int CamManOffset = 0x10;

        public const string ChrFollowCamAOB = "48 8B 0D ? ? ? ? E8 ? ? ? ? 48 8B 4E 68 48 8B 05 ? ? ? ? 48 89 48 60";
        public const int ChrFollowCamOffset1 = 0;
        public const int ChrFollowCamOffset2 = 0x60;
        public const int ChrFollowCamOffset3 = 0x60;

        public const string GroupMaskAOB = "80 3D ? ? ? ? 00 BE 00 00 00 80";
        public enum GroupMask
        {
            Map = 0x0,
            Objects = 0x1,
            Characters = 0x2,
            SFX = 0x3,
            Cutscenes = 0x4,
        }

        public const string GraphicsDataAOB = "48 8B 05 ? ? ? ? 48 8B 48 08 48 8B 01 48 8B 40 58";
        public const int GraphicsDataOffset1 = 0;
        public const int GraphicsDataOffset2 = 0x738;
        public enum GraphicsData
        {
            FilterOverride = 0x34D,
            FilterBrightnessR = 0x350,
            FilterBrightnessG = 0x354,
            FilterBrightnessB = 0x358,
            FilterSaturation = 0x35C,
            FilterContrastR = 0x360,
            FilterContrastG = 0x364,
            FilterContrastB = 0x368,
            FilterHue = 0x36C,
        }

        // NS_FRPG::GameMan
        public const string ChrClassWarpAOB = "48 8B 05 ? ? ? ? 66 0F 7F 80 ? ? ? ? 0F 28 02 66 0F 7F 80 ? ? ? ? C6 80";
        public const int ChrClassWarpOffset1 = 0;
        public enum ChrClassWarp
        {
            LastBonfire = 0xB34,
            StableX = 0xBA0,
            StableY = 0xBA4,
            StableZ = 0xBA8,
            StableAngle = 0xBB4,
            InitialX = 0xA80,
            InitialY = 0xA84,
            InitialZ = 0xA88,
            InitialAngle = 0xA94,
            SaveSlot = 0xAA0,
            AutoSave = 0xB70,
        }



        public const string ChrDbgAOB = "80 3D ? ? ? ? 00 48 8B 8F ? ? ? ? 0F B6 DB";
        public enum ChrDbg
        {
            PlayerNoDead = 0x0,
            PlayerExterminate = 0x1,
            AllNoStaminaConsume = 0x2,
            AllNoMpConsume = 0x3,
            AllNoArrowConsume = 0x4,
            AllNoMagicQtyConsume = 0x5,
            PlayerHide = 0x6,
            PlayerSilence = 0x7,
            AllNoDead = 0x8,
            AllNoDamage = 0x9,
            AllNoHit = 0xA,
            AllNoAttack = 0xB,
            AllNoMove = 0xC,
            AllNoUpdateAI = 0xD,
            PlayerReload = 0x12,
        }

        public const string MenuManAOB = "48 8B 05 ? ? ? ? 89 88 28 08 00 00 85 C9 ? ? C7 80 34 08 00 00 FF FF FF FF C3";
        public const int MenuManOffset1 = 0;
        public enum MenuMan
        {
            Repair = 0x60,
            LevelUp = 0x8C,
            AttuneMagic = 0x94,
            BottomlessBox = 0x98,
            Warp = 0xC0,
            MenuKick = 0x24C,
            PerformGesture = 0x2B8,
            Covenants = 0x300,
        }

        public enum Gesture
        {
            PointForward = 1,
            PointUp = 2,
            PointDown = 3,
            Beckon = 4,
            Wave = 5,
            Bow = 6,
            ProperBow = 7,
            Hurrah = 8,
            Joy = 9,
            Shrug = 10,
            LookSkyward = 11,
            WellWhatIsIt = 12,
            Prostration = 13,
            Prayer = 14,
            PraiseTheSun = 15,
        }

        // BaseB in public ce table
        public const string GameDataManAOB = "48 8B 05 ? ? ? ? 48 85 C0 ? ? F3 0F 58 80 AC 00 00 00";
        public const int GameDataManOffset1 = 0;
        public enum GameDataMan
        {
            PlayerGameData = 0x10,
            PlayerGameDataRecent = 0x18,
            LastBloodstainPos = 0x40,
            Settings = 0x58,
            ClearCount = 0x78,
            PlayTime = 0xA4,
        }

        public enum PlayerGameData
        {
            Hp = 0x14,
            MaxHp = 0x18,
            BaseMaxHp = 0x1C,

            Stamina = 0x30,
            MaxStamina = 0x34,
            BaseMaxStamina = 0x38,

            Vitality = 0x40,
            Attunement = 0x48,
            Endurance = 0x50,
            Strength = 0x58,
            Dexterity = 0x60,
            Intelligence = 0x68,
            Faith = 0x70,
            HumanityLvlMenu = 0x80,
            Humanity = 0x84,
            Resistance = 0x88,
            SoulLevel = 0x90,
            Souls = 0x94,

            NameString1 = 0xA8,

            Gender = 0xCA,
            Class = 0xCE,
            Physique = 0xCF,

            StartingGift = 0xD0,

            MultiplayerCount = 0xD4,
            CoopSuccessCount = 0xD8,

            ChaosServantContribution = 0xE8,
            WarriorOfSunlight = 0xED,
            Darkwraith = 0xEE,
            PathOfTheDragon = 0xEF,
            GravelordServant = 0xF0,
            ForestHunter = 0xF1,
            DarkmoonBlade = 0xF2,
            ChaosServant = 0xF3,
            Indictments = 0xF4,

            CurrentCovenant = 0x113,
            FaceType = 0x114,
            HairType = 0x115,
            HairColor = 0x116,
            InvadeType = 0x118,
            WeaponMemory = 0x119,
            EstusLevel = 0x11A,

            NameString2 = 0x12C, //(used to display name to other players)

            LeftWep1 = 0x324,
            RightWep1 = 0x328,
            LeftWep2 = 0x32C,
            RightWep2 = 0x330,
            Arrow1 = 0x334,
            Bolt1 = 0x338,
            Arrow2 = 0x33C,
            Bolt2 = 0x340,
            ArmorHead = 0x344,
            ArmorChest = 0x348,
            ArmorHands = 0x34C,
            ArmorLegs = 0x350,
            Hair = 0x354,
            Ring1 = 0x358,
            Ring2 = 0x35C,
            Quickbar1 = 0x360,
            Quickbar2 = 0x364,
            Quickbar3 = 0x368,
            Quickbar4 = 0x36C,
            Quickbar5 = 0x370,

            HeadSize = 0x388,
            ChestSize = 0x38C,
            AbdomenSize = 0x390,
            ArmSize = 0x394,
            LegSize = 0x398,

            EquipMagicData = 0x418,
            GestureEquipData = 0x450,

            HairRed = 0x4C0,
            HairGreen = 0x4C4,
            HairBlue = 0x4C8,
            HairAlpha = 0x4CC,

            EyeRed = 0x4D0,
            EyeGreen = 0x4D4,
            EyeBlue = 0x4D8,

            FaceDataStart = 0x4E0,
            SkinColorStart = 0x512,

            GestureGameData = 0x568,
        }

        public enum Settings
        {
            CameraSpeedx = 0x8,
            Vibration = 0x9,
            Brightness = 0xA,

            Music = 0xC,
            Sounds = 0xD,
            Voice = 0xE,
            ShowBlood = 0xF,
            Subtitles = 0x10,
            HUD = 0x11,
            UIScale = 0x12,
            XAxis = 0x13,
            YAxis = 0x14,
            AutoLockOn = 0x15,
            AutoWallRecovery = 0x16,

            RegionMatchmaking = 0x19,
            MatchmakingPassword = 0x1A,

            StartOnline1 = 0x3F,
            StartOnline2 = 0x40,

            RankingRegistration = 0x44,
            CameraSpeedY = 0x48,

            SummonSignVisibility = 0x50,

        }

        public enum EquipMagicData
        {
            AttunementSlot1 = 0x18,
            Quantity1 = 0x1C,
        }

        public enum GestureGameData
        {
            PointForward = 0x10,
            PointUp = 0x14,
            PointDown = 0x18,
            Beckon = 0x1C,
            Wave = 0x20,
            Bow = 0x24,
            ProperBow = 0x28,
            Hurrah = 0x2C,
            Joy = 0x30,
            Shrug = 0x34,
            LookSkyward = 0x38,
            WellWhatIsIt = 0x3C,
            Prostration = 0x40,
            Prayer = 0x44,
            PraiseTheSun = 0x48,
        }

        public const int GestureEquipDataOffset = 0x4;
        public enum GestureEquipData
        {
            Slot1 = 0x10,
            Slot2 = 0x14,
            Slot3 = 0x18,
            Slot4 = 0x1C,
            Slot5 = 0x20,
            Slot6 = 0x24,
            Slot7 = 0x28,
        }

        public enum LastBloodstainPos
        {
            PosX = 0x0,
            PosY = 0x4,
            PosZ = 0x8,
            PosAngle = 0x10,
        }

        public const int PlayerGameDataRecentOffset = 0x660;
        public enum PlayerGameDataRecent
        {
            // RecentPlayer is instance of PlayerGameData
            RecentPlayer1 = 0x460,
            RecentPlayer2 = 0x460 + PlayerGameDataRecentOffset,
            RecentPlayer3 = 0x460 + PlayerGameDataRecentOffset * 2,
            RecentPlayer4 = 0x460 + PlayerGameDataRecentOffset * 3,
            RecentPlayer5 = 0x460 + PlayerGameDataRecentOffset * 4,

        }

        public const string SoloParamManAOB = "4C 8B 05 ? ? ? ? 48 63 C9 48 8D 04 C9 41 3B 54 C0 10 7D ? 48 63 C2 48 8D 0C C9 48 03 C8 49 8B 44 C8 18";
        public const int SoloParamManOffset1 = 0x0;

        public enum SoloParamMan
        {
            EquipParamWeapon = 0x18,
            EquipParamProtector = 0x60,
            EquipParamAccessory = 0xA8,
            EquipParamGoods = 0xF0,
            ReinforceParamWeapon = 0x138,
            ReinforceParamProtector = 0x180,
            NpcParam = 0x1C8,
            AtkParam = 0x210, // AtkParamNpc??
            AtkParamPC = 0x258,
            NpcThinkParam = 0x2A0,
            ObjectParam = 0x2E8,
            BulletParam = 0x330,
            BehaviourParam = 0x378, // contains pointer to ParamResCap for ThrowInfoBank at 0x10
            BehaviourParam2 = 0x3C0, // One of these is BehaviourParam, the other is BehaviourParam_Pc
            MagicParam = 0x408,
            SpEffectParam = 0x450,
            SpEffectVfxParam = 0x498,
            TalkParam = 0x4E0,
            MenuParamColorTable = 0x528,
            ItemLotParam = 0x570,
            MoveParam = 0x5B8, // contains pointer to ParamResCap for ReinforceParamWeapon at 0x10
            CharacterInitParam = 0x600,
            EquipMtrlSetParam = 0x648,
            FaceParam = 0x690,
            RagdollParam = 0x6D8,
            ShopLineupParam = 0x720, // contains pointer to ParamResCap for LightBank at 0x10
            QwcChangeParam = 0x768,
            QwcJudgeParam = 0x7B0, // contains pointer to ParamResCap for DofBank at 0x10
            GameAreaParam = 0x7F8, // contains pointer to ParamResCap for TalkParam at 0x10
            SkeletonParam = 0x840,
            CalcCorrectGraph = 0x888,
            LockCamParam = 0x8D0, // contains pointer to ParamResCap for BulletParam at 0x10
            ObjActParam = 0x918,
            HitMtrlParam = 0x960,
            KnockBackParam = 0x9A8,
            LevelSyncParam = 0x9F0, // contains pointer to ParamResCap for QwcChangeParam at 0x10
            CoolTimeParam = 0xA38,
            WhiteCoolTimeParam = 0xA80, // contains pointer to ParamResCap for LevelSyncParam at 0x10
        }

        public const int ParamOffset1 = 0x38;
        public const int ParamOffset2 = 0x10;

        public enum EquipParamWeapon
        {
            BehaviourVariationID = 0x0,
            SortID = 0x4,
            WanderingEquipmentID = 0x8,
            Weight = 0xC,
            WeaponWeightRate = 0x10,
            FixPrice = 0x14,
            BasicPrice = 0x18,
            SellValue = 0x1C,
            CorrectStrength = 0x20,
            CorrectAgility = 0x24,
            CorrectMagic = 0x28,
            CorrectFaith = 0x2C,
            PhysGuardCutRate = 0x30,
            MagGuardCutRate = 0x34,
            FireGuardCutRate = 0x38,
            ThunGuardCutRate = 0x3C,
            SpEffectBehaviourID0 = 0x40,
            SpEffectBehaviourID1 = 0x44,
            SpEffectBehaviourID2 = 0x48,
            ResidentSpEffectID0 = 0x4C,
            ResidentSpEffectID1 = 0x50,
            ResidentSpEffectID2 = 0x54,
        }

        //public const string WorldChrManDbgImpAOB = "48 8B 05 ? ? ? ? 44 38 78 19 55 48 BD ? ? ? ? ? ? ? ? 48 87 2C 24 53 50 48 8B 5C 24 10";
        public const string WorldChrManDbgImpAOB = "48 8B 05 ? ? ? ? E9 ? ? ? ? 49 8B DC E9 ? ? ? ? 40 56 57 41 54 41 56 41 57 48 83 EC 30";
        public const int WorldChrManDbgImpOffset1 = 0x0;

        public enum WorldChrManDbgImp
        {
            Camera = 0xF0,
        }


        public const string EventFlagsAOB = "48 8B 0D ? ? ? ? 99 33 C2 45 33 C0 2B C2 8D 50 F6";
        public const int EventFlagsOffset1 = 0;
        public const int EventFlagsOffset2 = 0;

        public const string ItemGetAOB = "48 89 5C 24 18 89 54 24 10 55 56 57 41 54 41 55 41 56 41 57 48 8D 6C 24 F9";

        public const string BonfireWarpAOB = "48 89 5C 24 08 57 48 83 EC 20 48 8B D9 8B FA 48 8B 49 08 48 85 C9 0F 84 ? ? ? ? E8 ? ? ? ? 48 8B 4B 08";

        public const string DurabilityAOB = "41 8D 41 FF 45 8B C3 45 33 C9";
        public const string DurabilitySpecialAOB = "49 03 49 38 44 89 41 14 C3 32 C0";

        public const string LastTargetEntityAOB = "48 8B 03 48 8B CB E9 ? ? ? ? 48 8D 64 24 08 48 8D 64 24 08";
        public static readonly byte[] LastTargetEntityAOBBytes = { 0x48, 0x8B, 0x03, 0x48, 0x8B, 0xCB };
        public const int LastTargetEntityAsmOffset = 0x12;

        public const string LastHitEntityAOB = "48 8B 03 48 8B CB E9 ? ? ? ? B0 01 C3";
        public static readonly byte[] LastHitEntityAOBBytes = { 0x48, 0x8B, 0x03, 0x48, 0x8B, 0xCB};
        public const int LastHitEntityAsmOffset = 0x12;


        public const int EnemyInsOffset1 = 0x0;
        public enum EnemyIns
        {
            Handle = 0x8,

            ChrRes = 0x30,
            ChrModel = 0x60,
            EnemyCtrl = 0x68,
            ComManipulator = 0x70,
            ChrId = 0x88,
            ChrType = 0xD4,
            TeamType = 0xD8,
            MsbResCap = 0xB0,
            ChrTaeAnimEven = 0xC0,
            ChrFlags1 = 0x2A4,
            Opactiy = 0x328,
            Health = 0x3E8,
            MaxHealth = 0x3EC,
            Stamina = 0x3F8,
            MaxStamina = 0x3FC,
            ChrFlags2 = 0x524,

        }

        public const string BaseCARAOB = "48 8B 0D ? ? ? ? E8 ? ? ? ? 48 8B 4E 68 48 8B 05 ? ? ? ? 48 89 48 60 E8";
        public const int SosSignManOffset0 = 0xD48;
        public const int SosSignManOffset1 = 0x20;

        public enum SosSignMan
        {
            SosListEntry = 0x18,
        }

        public enum SosListEntry
        {
            Item1 = 0x0,
            Item2 = 0x8,
            Item3 = 0x10,
            SosSignManSign = 0x28,
        }

        public enum SosSignManSign
        {
            PosX = 0x8,
            PosY = 0xC,
            PosZ = 0x10,
            PosAngle = 0x14,
            AreaID = 0x18,
            SummonType = 0x22,
            Name = 0x24,

            LeftWep1 = 0x68,
            RightWep1 = 0x6C,
            LeftWep2 = 0x70,
            RightWep2 = 0x74,
            Arrow1 = 0x78,
            Bolt1 = 0x7C,
            Arrow2 = 0x80,
            Bolt2 = 0x84,
            ArmorHead = 0x88,
            ArmorChest = 0x8C,
            ArmorHands = 0x90,
            ArmorLegs = 0x94,
            Hair = 0x98,

            HairRed = 0x9C,
            HairGreen = 0x9D,
            HairBlue = 0x9E,
            HairAlpha = 0x9F,

            /*
            SummonType = 0x8,
            SteamID = 0xC,
            AreaID1 = 0x30,
            PosX = 0x34,
            PosY = 0x38,
            PosZ = 0x3C,
            PosAngle = 0x40,
            AreaID2 = 0x44,
            SoulLevel = 0x48,
            Covenant = 0x4C,
            MultiplayerCount = 0x50,
            */
        }
        
        public static DSROffsets GetOffsets(int moduleSize)
        {
            DSROffsets result = new DSROffsets();
            int version = versions.ContainsKey(moduleSize) ? versions[moduleSize] : 100;

            if (version > 1)
            {
            }

            if (version > 2)
            {
            }

            return result;
        }

        private static readonly Dictionary<int, int> versions = new Dictionary<int, int>()
        {
            [0x4869400] = 0, // 1.01
            [0x496BE00] = 1, // 1.01.1
            [0x37CB400] = 2, // 1.01.2
            [0x3817800] = 3, // 1.03
        };
        public const string PosLockAoB = "F3 0F 10 44 24 28 F3 0F 10 54 24 24 F3 0F 10 5C 24 20 0F 14 D8 0F 14 D1 0F 14 DA 0F 29 5B 10 0F 57 C0";
        public enum PosLock
        {
            PosLock = 0x1B
        }

        public const string FuncLevelUpAOB = "48 89 74 24 20 57 48 83 EC 20 48 8B 05 4F 20 69 01 4C 8B";
    }
}
