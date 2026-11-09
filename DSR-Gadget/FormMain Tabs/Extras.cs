using System;
using System.Drawing;
using System.Windows.Forms;

namespace DSR_Gadget
{
    // Options ported from Phokz's DSR Cheat Engine table.
    // Gameplay options go on the Cheats tab, camera/display options on the Graphics tab.
    // The controls are built in code so the large form designer file doesn't need editing.
    public partial class FormMain : Form
    {
        private CheckBox cbxShopAccess, cbxNoUpgradeMats, cbxNoFallDamage, cbxHideHud, cbxFov, cbxDrawDistance, cbxLockOnBonus;
        private ComboBox cmbRollType;
        private NumericUpDown nudFov, nudDrawDistance, nudLockOnBonus;
        private float? originalDrawDistance, originalLockOnBonus;
        private bool extrasChanging = false;

        private void initExtras()
        {
            // --- Cheats tab: gameplay ---
            GroupBox gbxGameplay = new GroupBox() { Text = "Gameplay (from cheat table)", Location = new Point(6, 462), Size = new Size(384, 95) };
            tpgCheats.Controls.Add(gbxGameplay);

            cbxShopAccess = addExtraCheckBox(gbxGameplay, "Access everything in shops", 6, 19,
                () => Hook.SetShopAccess(cbxShopAccess.Checked));
            cbxNoUpgradeMats = addExtraCheckBox(gbxGameplay, "Upgrade without materials", 6, 42,
                () => Hook.SetNoUpgradeMaterials(cbxNoUpgradeMats.Checked));
            cbxNoFallDamage = addExtraCheckBox(gbxGameplay, "No fall damage", 6, 65,
                () => Hook.SetNoFallDamage(cbxNoFallDamage.Checked));

            gbxGameplay.Controls.Add(new Label() { Text = "Rolls:", Location = new Point(200, 22), AutoSize = true });
            cmbRollType = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(240, 19), Size = new Size(138, 21) };
            cmbRollType.Items.AddRange(new object[] { "Normal", "Always quick", "Always ninja (fast)" });
            cmbRollType.SelectedIndex = 0;
            cmbRollType.SelectedIndexChanged += (s, e) =>
            {
                if (extrasChanging)
                    return;
                if (Hook.Hooked && !Hook.SetRollType(cmbRollType.SelectedIndex))
                {
                    extrasChanging = true;
                    cmbRollType.SelectedIndex = 0;
                    extrasChanging = false;
                    showPatchError("Forced rolls");
                }
            };
            gbxGameplay.Controls.Add(cmbRollType);

            // --- Graphics tab: camera and display ---
            GroupBox gbxCamera = new GroupBox() { Text = "Camera / Display (from cheat table)", Location = new Point(6, 125), Size = new Size(384, 125) };
            tpgGraphics.Controls.Add(gbxCamera);

            cbxHideHud = new CheckBox() { Text = "Hide HUD", Location = new Point(6, 19), AutoSize = true };
            cbxHideHud.CheckedChanged += (s, e) =>
            {
                if (Hook.Hooked && Hook.HudReady)
                    Hook.HideHud = cbxHideHud.Checked;
            };
            gbxCamera.Controls.Add(cbxHideHud);

            cbxFov = new CheckBox() { Text = "Custom field of view (degrees)", Location = new Point(6, 45), AutoSize = true };
            nudFov = addExtraNumber(gbxCamera, 45, 20, 140, 0, 43);
            cbxFov.CheckedChanged += (s, e) =>
            {
                if (extrasChanging || !Hook.Hooked)
                    return;
                if (cbxFov.Checked && Hook.CameraReady)
                {
                    // Start from the game's current FOV so turning it on doesn't jump
                    decimal current = (decimal)(Hook.CameraFovRadians * 180 / Math.PI);
                    if (current >= nudFov.Minimum && current <= nudFov.Maximum)
                        nudFov.Value = Math.Round(current);
                }
                applyFov();
            };
            nudFov.ValueChanged += (s, e) => { if (cbxFov.Checked) applyFov(); };
            gbxCamera.Controls.Add(cbxFov);

            cbxDrawDistance = new CheckBox() { Text = "Set draw distance", Location = new Point(6, 71), AutoSize = true };
            nudDrawDistance = addExtraNumber(gbxCamera, 71, 0, 100000, 1, 1000);
            cbxDrawDistance.CheckedChanged += (s, e) =>
            {
                if (!Hook.Hooked || !Hook.CameraReady)
                    return;
                if (cbxDrawDistance.Checked)
                {
                    originalDrawDistance = Hook.DrawDistance;
                    setExtraNumber(nudDrawDistance, originalDrawDistance.Value);
                }
                else if (originalDrawDistance != null)
                {
                    Hook.DrawDistance = originalDrawDistance.Value;
                    originalDrawDistance = null;
                }
            };
            gbxCamera.Controls.Add(cbxDrawDistance);

            cbxLockOnBonus = new CheckBox() { Text = "Extra lock-on range", Location = new Point(6, 97), AutoSize = true };
            nudLockOnBonus = addExtraNumber(gbxCamera, 97, 0, 1000, 1, 0);
            cbxLockOnBonus.CheckedChanged += (s, e) =>
            {
                if (!Hook.Hooked || !Hook.LockOnReady)
                    return;
                if (cbxLockOnBonus.Checked)
                {
                    originalLockOnBonus = Hook.LockOnRangeBonusValue;
                }
                else if (originalLockOnBonus != null)
                {
                    Hook.LockOnRangeBonusValue = originalLockOnBonus.Value;
                    originalLockOnBonus = null;
                }
            };
            gbxCamera.Controls.Add(cbxLockOnBonus);
        }

        private CheckBox addExtraCheckBox(Control parent, string text, int x, int y, Func<bool> apply)
        {
            CheckBox cbx = new CheckBox() { Text = text, Location = new Point(x, y), AutoSize = true };
            cbx.CheckedChanged += (s, e) =>
            {
                if (extrasChanging || !Hook.Hooked)
                    return;
                if (!apply())
                {
                    extrasChanging = true;
                    cbx.Checked = false;
                    extrasChanging = false;
                    showPatchError(text);
                }
            };
            parent.Controls.Add(cbx);
            return cbx;
        }

        private NumericUpDown addExtraNumber(Control parent, int y, decimal min, decimal max, int decimals, decimal value)
        {
            NumericUpDown nud = new NumericUpDown()
            {
                Location = new Point(250, y - 2),
                Size = new Size(128, 20),
                Minimum = min,
                Maximum = max,
                DecimalPlaces = decimals,
                Value = value,
            };
            parent.Controls.Add(nud);
            return nud;
        }

        private static void setExtraNumber(NumericUpDown nud, float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return;
            decimal d = (decimal)Math.Max((double)nud.Minimum, Math.Min((double)nud.Maximum, value));
            nud.Value = d;
        }

        private void applyFov()
        {
            float? radians = cbxFov.Checked ? (float)((double)nudFov.Value * Math.PI / 180) : (float?)null;
            if (!Hook.SetFovOverride(radians))
            {
                extrasChanging = true;
                cbxFov.Checked = false;
                extrasChanging = false;
                showPatchError("Custom field of view");
            }
        }

        private void showPatchError(string feature)
        {
            MessageBox.Show("Couldn't turn on \"" + feature + "\".\n\n"
                + "The game's code didn't look as expected. This usually means a different game version, "
                + "or another tool (like a Cheat Engine table) already changed the same spot.",
                "DSR Gadget", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void saveExtras() { }

        // Re-applies everything after the game is (re)hooked. Code patches stay in place through
        // loading screens, so applying again is harmless: already-applied patches are skipped.
        private void reloadExtras()
        {
            extrasChanging = true;
            if (cbxShopAccess.Checked && !Hook.SetShopAccess(true)) cbxShopAccess.Checked = false;
            if (cbxNoUpgradeMats.Checked && !Hook.SetNoUpgradeMaterials(true)) cbxNoUpgradeMats.Checked = false;
            if (cbxNoFallDamage.Checked && !Hook.SetNoFallDamage(true)) cbxNoFallDamage.Checked = false;
            if (cmbRollType.SelectedIndex != 0 && !Hook.SetRollType(cmbRollType.SelectedIndex)) cmbRollType.SelectedIndex = 0;
            extrasChanging = false;
            if (cbxFov.Checked)
                applyFov();
            if (cbxHideHud.Checked && Hook.HudReady)
                Hook.HideHud = true;
        }

        // Keeps "frozen" values in place, like Cheat Engine's freeze checkbox
        private void updateExtras()
        {
            if (cbxDrawDistance.Checked && Hook.CameraReady)
            {
                // Remember the game's own value the first time, so it can be put back later
                if (originalDrawDistance == null)
                {
                    originalDrawDistance = Hook.DrawDistance;
                    setExtraNumber(nudDrawDistance, originalDrawDistance.Value);
                }
                Hook.DrawDistance = (float)nudDrawDistance.Value;
            }
            if (cbxLockOnBonus.Checked && Hook.LockOnReady)
            {
                if (originalLockOnBonus == null)
                    originalLockOnBonus = Hook.LockOnRangeBonusValue;
                Hook.LockOnRangeBonusValue = (float)nudLockOnBonus.Value;
            }
        }

        // Puts the game back the way it was when DSR Gadget closes
        private void resetExtras()
        {
            if (!Hook.Hooked)
                return;
            Hook.SetShopAccess(false);
            Hook.SetNoUpgradeMaterials(false);
            Hook.SetNoFallDamage(false);
            Hook.SetRollType(0);
            Hook.SetFovOverride(null);
            if (cbxHideHud.Checked && Hook.HudReady)
                Hook.HideHud = false;
            if (originalDrawDistance != null && Hook.CameraReady)
                Hook.DrawDistance = originalDrawDistance.Value;
            if (originalLockOnBonus != null && Hook.LockOnReady)
                Hook.LockOnRangeBonusValue = originalLockOnBonus.Value;
        }
    }
}
