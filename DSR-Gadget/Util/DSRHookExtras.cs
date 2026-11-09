using PropertyHook;
using System;

namespace DSR_Gadget
{
    // Features ported from Phokz's DSR Cheat Engine table ("Scripts" and "Camera" sections).
    public partial class DSRHook
    {
        // --- Code locations (same AOBs as the cheat table) ---
        private const string ShopAOB = "8B 51 0C 83 FA FF 74 ? 49";
        private const string UpgradeMatsAOB = "41 8B 00 89 04 24";
        private const string FallDamageAOB = "0F 28 80 10 01 00 00 0F 5C";
        private const string RollTypeAOB = "48 8B F9 0F 29 74 24 20 0F 28 F2 3B";
        private const string FovWriteAOB = "F3 0F 11 4E 50";

        // --- Pointers for camera/display values ---
        private const string HudBaseAOB = "48 8B 05 ? ? ? ? 45 33 ED 48 8B F1 48 85 C0";
        private const int HudOffset1 = 0x58;
        private const int HudFlag = 0x11;
        private const string CameraBaseAOB = "48 8B 05 ? ? ? ? 48 89 48 60 E8";
        private const int CameraFov = 0x50;
        private const int CameraDrawDistance = 0x5C;
        private const string LockOnBonusAOB = "48 8B 0D ? ? ? ? 89 99 58 15 00 00";
        private const int LockOnRangeBonus = 0x1520;

        private CodePatch ShopPatch, UpgradeMatsPatch, FallDamagePatch, QuickRollPatch, NinjaRollPatch, FovPatch;
        private PHPointer HudPtr, CameraPtr, LockOnBonusPtr;

        // Called from the constructor
        private void initExtras()
        {
            // mov edx,[rcx+0C] / cmp edx,-1  ->  or edx,-1 / cmp edx,-1
            // Every shop line is treated as always available
            ShopPatch = CodePatch.InPlace(this, RegisterAbsoluteAOB(ShopAOB),
                new byte[] { 0x8B, 0x51, 0x0C, 0x83, 0xFA, 0xFF },
                new byte[] { 0x83, 0xCA, 0xFF, 0x83, 0xFA, 0xFF });

            // mov eax,[r8] / mov [rsp],eax  ->  mov eax,FFFFFFFF / mov [rsp],eax
            // The game reads "upgrade material needed" as "none"
            UpgradeMatsPatch = CodePatch.Cave(this, RegisterAbsoluteAOB(UpgradeMatsAOB),
                new byte[] { 0x41, 0x8B, 0x00, 0x89, 0x04, 0x24 },
                new byte[] { 0xB8, 0xFF, 0xFF, 0xFF, 0xFF, 0x89, 0x04, 0x24 },
                runOriginal: false);

            // Before reading fall data, reset the fall start height so the game never sees a long fall:
            // mov dword ptr [rax+114],(float)-2000
            FallDamagePatch = CodePatch.Cave(this, RegisterAbsoluteAOB(FallDamageAOB),
                new byte[] { 0x0F, 0x28, 0x80, 0x10, 0x01, 0x00, 0x00 },
                new byte[] { 0xC7, 0x80, 0x14, 0x01, 0x00, 0x00, 0x00, 0x00, 0xFA, 0xC4 });

            // Force the roll type passed in edx: 1 = quick, 0 = ninja (fast) roll
            PHPointer rollLocation = RegisterAbsoluteAOB(RollTypeAOB);
            byte[] rollOriginal = { 0x48, 0x8B, 0xF9, 0x0F, 0x29, 0x74, 0x24, 0x20 };
            QuickRollPatch = CodePatch.Cave(this, rollLocation, rollOriginal, new byte[] { 0xBA, 0x01, 0x00, 0x00, 0x00 });
            NinjaRollPatch = CodePatch.Cave(this, rollLocation, rollOriginal, new byte[] { 0xBA, 0x00, 0x00, 0x00, 0x00 });

            // movss xmm1,[rip+0A] (our FOV value, kept in the cave's data area) before the game's own FOV write
            FovPatch = CodePatch.Cave(this, RegisterAbsoluteAOB(FovWriteAOB),
                new byte[] { 0xF3, 0x0F, 0x11, 0x4E, 0x50 },
                new byte[] { 0xF3, 0x0F, 0x10, 0x0D, 0x0A, 0x00, 0x00, 0x00 });

            HudPtr = RegisterRelativeAOB(HudBaseAOB, 3, 7, 0, HudOffset1);
            CameraPtr = RegisterRelativeAOB(CameraBaseAOB, 3, 7, 0, 0x60, 0x60);
            LockOnBonusPtr = RegisterRelativeAOB(LockOnBonusAOB, 3, 7, 0);

            OnUnhooked += (s, e) =>
            {
                foreach (CodePatch patch in new[] { ShopPatch, UpgradeMatsPatch, FallDamagePatch, QuickRollPatch, NinjaRollPatch, FovPatch })
                    patch.Forget();
            };
        }

        private static bool setPatch(CodePatch patch, bool enable)
        {
            if (enable)
                return patch.Apply();
            patch.Remove();
            return true;
        }

        // Each setter returns false if the patch couldn't be applied
        public bool SetShopAccess(bool enable) => setPatch(ShopPatch, enable);
        public bool SetNoUpgradeMaterials(bool enable) => setPatch(UpgradeMatsPatch, enable);
        public bool SetNoFallDamage(bool enable) => setPatch(FallDamagePatch, enable);

        /// <summary>0 = normal, 1 = quick rolls, 2 = ninja rolls</summary>
        public bool SetRollType(int type)
        {
            // Both patch the same instruction, so only one can be on at a time
            QuickRollPatch.Remove();
            NinjaRollPatch.Remove();
            if (type == 1)
                return QuickRollPatch.Apply();
            if (type == 2)
                return NinjaRollPatch.Apply();
            return true;
        }

        /// <summary>Overrides the camera FOV (radians). Pass null to give control back to the game.</summary>
        public bool SetFovOverride(float? radians)
        {
            if (radians == null)
            {
                FovPatch.Remove();
                return true;
            }
            if (!FovPatch.Apply())
                return false;
            FovPatch.WriteCave(FovPatch.CaveDataOffset, BitConverter.GetBytes(radians.Value));
            return true;
        }

        public float CameraFovRadians => CameraPtr.ReadSingle(CameraFov);

        public bool HideHud
        {
            get => HudPtr.ReadByte(HudFlag) == 0;
            set => HudPtr.WriteByte(HudFlag, value ? (byte)0 : (byte)1);
        }

        public float DrawDistance
        {
            get => CameraPtr.ReadSingle(CameraDrawDistance);
            set => CameraPtr.WriteSingle(CameraDrawDistance, value);
        }

        public float LockOnRangeBonusValue
        {
            get => LockOnBonusPtr.ReadSingle(LockOnRangeBonus);
            set => LockOnBonusPtr.WriteSingle(LockOnRangeBonus, value);
        }

        public bool CameraReady => CameraPtr.Resolve() != IntPtr.Zero;
        public bool LockOnReady => LockOnBonusPtr.Resolve() != IntPtr.Zero;
        public bool HudReady => HudPtr.Resolve() != IntPtr.Zero;
    }
}
