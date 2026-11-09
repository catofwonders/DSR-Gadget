using DSR_Gadget.SubForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSR_Gadget
{
    public partial class FormMain : Form
    {
        EnemyInsForm LastTargetEntityForm;
        EnemyInsForm LastHitEntityForm;
        List<DSRFashionCategory> Armor = new List<DSRFashionCategory>();
        List<DSRFashionCategory> Weapons = new List<DSRFashionCategory>();

        private void initMisc() 
        {
            DSRFashionCategory.GetItemCategories();
            foreach (DSRFashionCategory category in DSRFashionCategory.All)
            {
                if (category.ID == 0x10000000)
                {
                    Armor.Add(category);
                }
                else if (category.ID == 0x00000000)
                {
                    Weapons.Add(category);
                }
            }


            cmbSlot.Items.Add("Hair");
            cmbSlot.Items.Add("Bolt 1");
            cmbSlot.Items.Add("Arrow 1");

            cmbSlot.SelectedIndex = 0;
        }

        private void saveMisc() { }

        private void resetMisc() 
        {
            Hook.LastTargetEntity = false;
            Hook.LastHitEntity = false;
        }

        private void reloadMisc() 
        {
            if (cbxHairRandom.Checked)
                Task.Run(RandomizeHair);

            if (cbxEyeRandom.Checked)
                Task.Run(RandomizeEye);
        }

        private void onHookedMisc()
        {
            if (cbxLastTargetEntity.Checked)
                Hook.LastTargetEntity = true;
            if (cbxLastHitEntity.Checked)
                Hook.LastHitEntity = true;
        }

        internal void EnableMiscStats(bool enable)
        {
            if (enable)
            {
                SetIdLabel();
                SetPanelColor();
                pnlEyeColor.BorderStyle = BorderStyle.FixedSingle;
                pnlHairColor.BorderStyle = BorderStyle.FixedSingle;
            }
            else
            {
                pnlEyeColor.BorderStyle = BorderStyle.None;
                pnlHairColor.BorderStyle = BorderStyle.None;
            }
        }
        private void updateMisc() 
        {
            SetPanelColor();
        }

        private void btnEventRead_Click(object sender, EventArgs e)
        {
            if (!Int32.TryParse(txtEventFlag.Text.Trim(), out int id))
                return;
            try
            {
                cbxEventFlag.Checked = Hook.ReadEventFlag(id);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Event flag " + id + " isn't supported. Flags must be 8 digits from a known area.",
                    "Event Flag", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnEventWrite_Click(object sender, EventArgs e)
        {
            if (!Int32.TryParse(txtEventFlag.Text.Trim(), out int id))
                return;
            try
            {
                Hook.WriteEventFlag(id, cbxEventFlag.Checked);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Event flag " + id + " isn't supported. Flags must be 8 digits from a known area.",
                    "Event Flag", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnLastTargetEntity_Click(object sender, EventArgs e)
        {
            LastTargetEntityForm = new EnemyInsForm();
            LastTargetEntityForm.InitForm(Hook, Hook.GetLastTargetEntity(), "Last Targeted Entity");
            LastTargetEntityForm.StartPosition = FormStartPosition.CenterScreen;
            LastTargetEntityForm.Show();
        }

        private void cbxLastTargetEntity_CheckedChanged(object sender, EventArgs e)
        {
            Hook.LastTargetEntity = cbxLastTargetEntity.Checked;
            btnLastTargetEntity.Enabled = cbxLastTargetEntity.Checked;
            if (!cbxLastTargetEntity.Checked)
                LastTargetEntityForm.Close();
        }

        private void btnLastHitEntity_Click(object sender, EventArgs e)
        {
            LastHitEntityForm = new EnemyInsForm();
            LastHitEntityForm.InitForm(Hook, Hook.GetLastHitEntity(), "Last Hit Entity");
            LastHitEntityForm.StartPosition = FormStartPosition.CenterScreen;
            LastHitEntityForm.Show();
        }

        private void cbxLastHitEntity_CheckedChanged(object sender, EventArgs e)
        {
            Hook.LastHitEntity = cbxLastHitEntity.Checked;
            btnLastHitEntity.Enabled = cbxLastHitEntity.Checked;
            if (!cbxLastHitEntity.Checked)
                LastHitEntityForm.Close();
        }
        private void cmbCategoryFashion_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbxItemsFashion.Items.Clear();
            DSRFashionCategory category = cmbCategoryFashion.SelectedItem as DSRFashionCategory;
            foreach (DSRItem item in category.Items)
                lbxItemsFashion.Items.Add(item);
            lbxItemsFashion.SelectedIndex = 0;
            searchBoxFashion.Text = "";
            lblSearch.Visible = true;
        }

        //Clear items and add the ones that match text in search box
        private void FilterItemsFashion()
        {

            lbxItemsFashion.Items.Clear();

            if (cbxSearchAll.Checked && searchBoxFashion.Text != "")
            {
                //search every item category
                foreach (DSRFashionCategory category in cmbCategoryFashion.Items)
                {
                    foreach (DSRItem item in category.Items)
                    {
                        if (item.ToString().ToLower().Contains(searchBoxFashion.Text.ToLower()))
                            lbxItemsFashion.Items.Add(item);
                    }
                }
            }
            else
            {
                //only search selected item category
                DSRFashionCategory category = cmbCategoryFashion.SelectedItem as DSRFashionCategory;
                foreach (DSRItem item in category.Items)
                {
                    if (item.ToString().ToLower().Contains(searchBoxFashion.Text.ToLower()))
                        lbxItemsFashion.Items.Add(item);
                }
            }


            /*
            //original code
            DSRItemCategory category = cmbCategoryFashion.SelectedItem as DSRItemCategory;
            foreach (DSRItem item in category.Items)
            {
                if (item.ToString().ToLower().Contains(searchBoxFashion.Text.ToLower()))
                {
                    lbxItemsFashion.Items.Add(item);
                }
            }
            */

            if (lbxItemsFashion.Items.Count > 0)
                lbxItemsFashion.SelectedIndex = 0;

            HandleSearchLabelFashion();
        }

        private void searchBoxFashion_TextChanged(object sender, EventArgs e)
        {
            FilterItemsFashion();
        }

        //Handles the "Searching..." label on the text box
        private void HandleSearchLabelFashion()
        {
            if (searchBoxFashion.Text == "")
                lblSearch.Visible = true;
            else
                lblSearch.Visible = false;
        }

        private void btnApplyHair_Click(object sender, EventArgs e)
        {
            _ = ChangeColorFashion(Color.DarkGray);
            ApplyItem();
            SetIdLabel();
        }

        private void ApplyItem()
        {
            switch (cmbSlot.SelectedIndex)
            {
                case 0:
                    ApplyHair();
                    break;
                case 1:
                    ApplyBoltOne();
                    break;
                case 2:
                    ApplyArrowOne();
                    break;
                default:
                    break;
            }
        }

        //Apply hair to currently loaded character
        private void ApplyHair()
        {
            //Check if the button is enabled and the selected item isn't null
            if (btnApplyHair.Enabled == true && lbxItemsFashion.SelectedItem != null)
            {
                DSRItem item = lbxItemsFashion.SelectedItem as DSRItem;
                int id = item.ID;
                Player.Hair = id;
            }
        }

        private void ApplyBoltOne()
        {
            if (btnApplyHair.Enabled == true && lbxItemsFashion.SelectedItem != null)
            {
                DSRItem item = lbxItemsFashion.SelectedItem as DSRItem;
                int id = item.ID;
                Player.Bolt1 = id;
            }
        }

        private void ApplyArrowOne()
        {
            if (btnApplyHair.Enabled == true && lbxItemsFashion.SelectedItem != null)
            {
                DSRItem item = lbxItemsFashion.SelectedItem as DSRItem;
                int id = item.ID;
                Player.Arrow1 = id;
            }
        }

        //Give focus and select all
        private void searchBoxFashion_Click(object sender, EventArgs e)
        {
            searchBoxFashion.SelectAll();
            searchBoxFashion.Focus();
        }

        //handles up down and enter
        private void KeyDownListboxFashion(KeyEventArgs e)
        {
            //Scroll down through Items listbox and go back to bottom at end
            if (e.KeyCode == Keys.Up)
            {
                e.Handled = true;//Do not pass keypress along
                //Check is there's still items to go through
                if (lbxItemsFashion.SelectedIndex > 0)
                {
                    lbxItemsFashion.SelectedIndex -= 1;
                    return;
                }

                //Check if last item or "over" for safety
                if (lbxItemsFashion.SelectedIndex <= 0)
                {
                    lbxItemsFashion.SelectedIndex = lbxItemsFashion.Items.Count - 1; //-1 because Selected Index is 0 based and Count isn't
                    return;
                }
            }

            //Scroll down through Items listbox and go back to top at end
            if (e.KeyCode == Keys.Down)
            {
                e.Handled = true;//Do not pass keypress along
                //Check is there's still items to go through
                if (lbxItemsFashion.SelectedIndex < lbxItemsFashion.Items.Count - 1) //-1 because Selected Index is 0 based and Count isn't
                {
                    lbxItemsFashion.SelectedIndex += 1;
                    return;
                }

                //Check if last item or "over" for safety
                if (lbxItemsFashion.SelectedIndex >= lbxItemsFashion.Items.Count - 1) //-1 because Selected Index is 0 based and Count isn't
                {
                    lbxItemsFashion.SelectedIndex = 0;
                    return;
                }
            }
        }

        //Changes the color of the Apply button
        private async Task ChangeColorFashion(Color new_color)
        {
            btnApplyHair.BackColor = new_color;

            await Task.Delay(TimeSpan.FromSeconds(.25));

            btnApplyHair.BackColor = default(Color);
        }

        //handles escape
        private void KeyPressedFashion(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                searchBoxFashion.Clear();
                return;
            }

            //Create selected index as item
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true; //Do not pass keypress along
                _ = ChangeColorFashion(Color.DarkGray);
                ApplyItem();
                return;
            }

            if (lbxItemsFashion.Items.Count == 0)
            {
                if (e.KeyCode == Keys.Up)
                    e.Handled = true;
                if (e.KeyCode == Keys.Down)
                    e.Handled = true;
                return;
            }

            KeyDownListboxFashion(e);

        }

        private void SearchAllCheckboxFashion_CheckedChanged(object sender, EventArgs e)
        {
            //checkbox changed, refresh search filter (if searchBoxFashion is not empty)
            if (searchBoxFashion.Text != "")
                FilterItems();
        }

        private void cmbSlot_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbSlot.SelectedIndex)
            {
                case 0:
                    LoadCategory(Armor);
                    break;
                case 1:
                case 2:
                    LoadCategory(Weapons);
                    break;
                default:
                    break;
            }

            SetIdLabel();
        }

        private void SetIdLabel()
        {
            if (Hook.Loaded)
            {
                switch (cmbSlot.SelectedIndex)
                {
                    case 0:
                        lblID.Text = $"ID = {Player.Hair}";
                        break;
                    case 1:
                        lblID.Text = $"ID = {Player.Bolt1}";
                        break;
                    case 2:
                        lblID.Text = $"ID = {Player.Arrow1}";
                        break;
                    default:
                        break;
                }
            }
        }

        private void LoadCategory(List<DSRFashionCategory> list)
        {
            cmbCategoryFashion.Items.Clear();
            foreach (var category in list)
            {
                cmbCategoryFashion.Items.Add(category);
            }
            cmbCategoryFashion.SelectedIndex = 0;
        }



        private bool SelectorOpen;

        private void pnlHairColor_Click(object sender, EventArgs e)
        {
            if (!SelectorOpen)
            {
                var colorSelector = new ColorSelectorHair(Player);
                colorSelector.Disposed += OnHairColorSelectorDisposed;
                colorSelector.Show();
                SelectorOpen = true;
            }
        }

        private void OnHairColorSelectorDisposed(object sender, EventArgs e)
        {
            SelectorOpen = false;
        }

        private void pnlEyeColor_Click(object sender, EventArgs e)
        {
            if (!SelectorOpen)
            {
                var colorSelector = new ColorSelectorEye(Player);
                colorSelector.Disposed += OnEyeColorSelectorDisposed;
                colorSelector.Show();
                SelectorOpen = true;
            }
        }

        private void OnEyeColorSelectorDisposed(object sender, EventArgs e)
        {
            SelectorOpen = false;
        }


        private void SetPanelColor()
        {
            if (Hook.Loaded)
            {
                var red = Player.HairRed > 1 ? (byte)(((Player.HairRed / 10) * 255)) : (byte)(Player.HairRed * 255);
                var green = Player.HairGreen > 1 ? (byte)(((Player.HairGreen / 10) * 255)) : (byte)(Player.HairGreen * 255);
                var blue = Player.HairBlue > 1 ? (byte)(((Player.HairBlue / 10) * 255)) : (byte)(Player.HairBlue * 255);

                pnlHairColor.BackColor = Color.FromArgb(red, green, blue);

                red = Player.EyeRed > 1 ? (byte)(((Player.EyeRed / 10) * 255)) : (byte)(Player.EyeRed * 255);
                green = Player.EyeGreen > 1 ? (byte)(((Player.EyeGreen / 10) * 255)) : (byte)(Player.EyeGreen * 255);
                blue = Player.EyeBlue > 1 ? (byte)(((Player.EyeBlue / 10) * 255)) : (byte)(Player.EyeBlue * 255);

                pnlEyeColor.BackColor = Color.FromArgb(red, green, blue);
            }
        }
        Random Rand = new Random();

        private void cbxHairRandom_CheckedChanged(object sender, EventArgs e)
        {
            _ = Task.Run(RandomizeHair);
        }


        double HairSpeedMultiplier = 1;

        private async Task RandomizeHair()
        {
            if (!Hook.Loaded)
                return;
            var neon = Player.HairRed > 1 || Player.HairGreen > 1 || Player.HairBlue > 1;

            var red = neon ? (byte)(((Player.HairRed / 10) * 255)) : (byte)(Player.HairRed * 255);
            var green = neon ? (byte)(((Player.HairGreen / 10) * 255)) : (byte)(Player.HairGreen * 255);
            var blue = neon ? (byte)(((Player.HairBlue / 10) * 255)) : (byte)(Player.HairBlue * 255);

            var color = Color.FromArgb(red, green, blue);

            while (cbxHairRandom.Checked && Hook.Loaded)
            {
                var hsl = RgbaToHsl(color);
                hsl.H += 0.01f;
                color = HslToRgba(hsl);
                Player.HairRed = neon ? (float)((color.R / 255f) * 10) : (float)(color.R / 255f);
                Player.HairGreen = neon ? (float)((color.G / 255f) * 10) : (float)(color.G / 255f);
                Player.HairBlue = neon ? (float)((color.B / 255f) * 10) : (float)(color.B / 255f);
                Thread.Sleep((int)(100 / HairSpeedMultiplier));

                //var red = neon ? (byte)(((Player.HairRed / 10) * 255)) : (byte)(Player.HairRed * 255);
                //var green = neon ? (byte)(((Player.HairGreen / 10) * 255)) : (byte)(Player.HairGreen * 255);
                //var blue = neon ? (byte)(((Player.HairBlue / 10) * 255)) : (byte)(Player.HairBlue * 255);

                //var color = Color.FromArgb(red, green, blue);
                //var target = Color.FromArgb(Rand.Next(255), Rand.Next(255), Rand.Next(255));

                //var amount = 0.0;
                //while (amount < 1f)
                //{
                //    if (!cbxHairRandom.Checked && Hook.Loaded)
                //        break;
                //    var lColor = Lerp(color, target, (double)amount);
                //    Player.HairRed = neon ? (double)((lColor.R / 255f) * 10) : (double)(lColor.R / 255f);
                //    Player.HairGreen = neon ? (double)((lColor.G / 255f) * 10) : (double)(lColor.G / 255f);
                //    Player.HairBlue = neon ? (double)((lColor.B / 255f) * 10) : (double)(lColor.B / 255f);
                //    amount += 0.00003 * HairSpeedMultiplier;
                //}
            }
        }

        private void cbxEyeRandom_CheckedChanged(object sender, EventArgs e)
        {
            _ = Task.Run(RandomizeEye);
        }

        double EyeSpeedMultiplier = 1;

        private async Task RandomizeEye()
        {
            if (!Hook.Loaded)
                return;
            var neon = Player.EyeRed > 1 || Player.EyeGreen > 1 || Player.EyeBlue > 1;

            var red = neon ? (byte)(((Player.EyeRed / 10) * 255)) : (byte)(Player.EyeRed * 255);
            var green = neon ? (byte)(((Player.EyeGreen / 10) * 255)) : (byte)(Player.EyeGreen * 255);
            var blue = neon ? (byte)(((Player.EyeBlue / 10) * 255)) : (byte)(Player.EyeBlue * 255);

            var color = Color.FromArgb(red, green, blue);

            while (cbxEyeRandom.Checked && Hook.Loaded)
            {
                var hsl = RgbaToHsl(color);
                hsl.H += 0.01;
                color = HslToRgba(hsl);
                Player.EyeRed = neon ? (float)((color.R / 255f) * 10) : (float)(color.R / 255f);
                Player.EyeGreen = neon ? (float)((color.G / 255f) * 10) : (float)(color.G / 255f);
                Player.EyeBlue = neon ? (float)((color.B / 255f) * 10) : (float)(color.B / 255f);
                Thread.Sleep((int)(100 / EyeSpeedMultiplier));
                //var red = neon ? (byte)(((Player.EyeRed / 10) * 255)) : (byte)(Player.EyeRed * 255);
                //var green = neon ? (byte)(((Player.EyeGreen / 10) * 255)) : (byte)(Player.EyeGreen * 255);
                //var blue = neon ? (byte)(((Player.EyeBlue / 10) * 255)) : (byte)(Player.EyeBlue * 255);

                //var color = Color.FromArgb(red, green, blue);
                //var target = Color.FromArgb(Rand.Next(255), Rand.Next(255), Rand.Next(255));

                //var amount = 0.0;
                //while (amount < 1f)
                //{
                //    if (!cbxEyeRandom.Checked && Hook.Loaded)
                //        break;
                //    var lColor = Lerp(color, target, (double)amount);
                //    Player.EyeRed = neon ? (double)((lColor.R / 255f) * 10) : (double)(lColor.R / 255f);
                //    Player.EyeGreen = neon ? (double)((lColor.G / 255f) * 10) : (double)(lColor.G / 255f);
                //    Player.EyeBlue = neon ? (double)((lColor.B / 255f) * 10) : (double)(lColor.B / 255f);
                //    amount += 0.00003 * EyeSpeedMultiplier;
                //}
            }
        }
        public static Color Lerp(Color a, Color b, double t)
        {
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t)
             );

        }

        public struct HSLAColor
        {
            public double H;
            public double S;
            public double L;
            public double A;
            public HSLAColor(double h, double s, double l, double a)
            {
                H = h;
                S = s;
                L = l;
                A = a;
            }
        }
        public static HSLAColor RgbaToHsl(Color rgba)
        {
            double r = rgba.R / 255.0f;
            double g = rgba.G / 255.0f;
            double b = rgba.B / 255.0f;

            double max = (r > g && r > b) ? r : (g > b) ? g : b;
            double min = (r < g && r < b) ? r : (g < b) ? g : b;

            double h, s, l;
            h = s = l = (max + min) / 2.0f;

            if (max == min)
                h = s = 0.0f;

            else
            {
                double d = max - min;
                s = (l > 0.5f) ? d / (2.0f - max - min) : d / (max + min);

                if (r > g && r > b)
                    h = (g - b) / d + (g < b ? 6.0f : 0.0f);

                else if (g > b)
                    h = (b - r) / d + 2.0f;

                else
                    h = (r - g) / d + 4.0f;

                h /= 6.0f;
            }

            return new HSLAColor(h, s, l, rgba.A / 255.0f);
        }

        public static Color HslToRgba(HSLAColor hsl)
        {
            double r, g, b;

            if (hsl.S == 0.0f)
                r = g = b = hsl.L;

            else
            {
                var q = hsl.L < 0.5f ? hsl.L * (1.0f + hsl.S) : hsl.L + hsl.S - hsl.L * hsl.S;
                var p = 2.0f * hsl.L - q;
                r = HueToRgb(p, q, hsl.H + 1.0f / 3.0f);
                g = HueToRgb(p, q, hsl.H);
                b = HueToRgb(p, q, hsl.H - 1.0f / 3.0f);
            }

            return Color.FromArgb((int)(hsl.A * 255), (int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        // Helper for HslToRgba
        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0.0f) t += 1.0f;
            if (t > 1.0f) t -= 1.0f;
            if (t < 1.0f / 6.0f) return p + (q - p) * 6.0f * t;
            if (t < 1.0f / 2.0f) return q;
            if (t < 2.0f / 3.0f) return p + (q - p) * (2.0f / 3.0f - t) * 6.0f;
            return p;
        }

        private void nudHairSpeed_ValueChanged(object sender, EventArgs e)
        {
            HairSpeedMultiplier = (double)nudHairSpeed.Value;
        }

        private void nudEyeSpeed_ValueChanged(object sender, EventArgs e)
        {
            EyeSpeedMultiplier = (double)nudEyeSpeed.Value;
        }
    }
}
