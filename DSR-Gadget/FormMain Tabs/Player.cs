using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSR_Gadget
{
    public partial class FormMain : Form
    {
        private List<int> unknownBonfires = new List<int>();
        private List<SavedPos> Positions = new List<SavedPos>();
        private State.PlayerState playerState;
        private DSRPlayer Player;

        private void initPlayer()
        {
            cbxRestoreState.Checked = settings.RestoreState;
            cbxQuickSelectBonfire.Checked = settings.QuickSelectbonfireChecked;

            Positions = SavedPos.GetSavedPositions();
            UpdatePositions();

            foreach (DSRBonfire bonfire in DSRBonfire.All)
                cmbBonfire.Items.Add(bonfire);
            cmbBonfire.SelectedIndex = 0;

            foreach (DSRTeam team in DSRTeam.All)
                cmbChrSelect.Items.Add(team);
            cmbChrSelect.SelectedIndex = 0;
            cmbChrSelect.SelectedIndexChanged += cmbChrSelect_SelectedIndexChanged;

            foreach (DSRInvasion invasion in DSRInvasion.All)
                cmbInvasionSelect.Items.Add(invasion);
            cmbInvasionSelect.SelectedIndex = 0;
            cmbInvasionSelect.SelectedIndexChanged += cmbInvasionSelect_SelectedIndexChanged;

            foreach (DSRArea area in DSRArea.All)
            {
                cmbMPAreaID.Items.Add(area);
                cmbAreaID.Items.Add(area);
            }
            cmbMPAreaID.SelectedIndex = 0;
            cmbAreaID.SelectedIndex = 0;
            cmbMPAreaID.SelectedIndexChanged += cmbMPAreaID_SelectedIndexChanged;
            cmbAreaID.SelectedIndexChanged += cmbAreaID_SelectedIndexChanged;
            Player = Hook.GetPlayer();

            nudSpeed.Value = settings.AnimSpeed;

            lastSetBonfire = new DSRBonfire(-1, "Last Set: None"); //last set bonfire (default values)
            cmbBonfire.Items.Add(lastSetBonfire); //add to end of filter
        }

        private void savePlayer()
        {
            settings.RestoreState = cbxRestoreState.Checked;
            settings.AnimSpeed = nudSpeed.Value;
            settings.QuickSelectbonfireChecked = cbxQuickSelectBonfire.Checked;
        }

        private void resetPlayer()
        {
            if (loaded)
            {
                if (!cbxGravity.Checked)
                    Hook.NoGravity = false;
                if (!cbxCollision.Checked)
                    Hook.NoCollision = false;
                if (cbxSpeed.Checked)
                    Player.AnimSpeed = 1;
            }
        }

        private void reloadPlayer()
        {
            if (!cbxGravity.Checked)
                Hook.NoGravity = true;
            if (!cbxCollision.Checked)
                Hook.NoCollision = true;

            cmbBonfire.SelectedIndex = cmbBonfire.Items.Count - 1;
        }

        private void updatePlayer()
        {
            setClamped(nudHealth, Player.Hp);
            setClamped(nudHealthMax, Player.MaxHp);
            setClamped(nudStamina, Player.Stamina);
            setClamped(nudStaminaMax, Player.MaxStamina);

            if (cbxFreezeChrType.Checked)
                Player.ChrType = (int)nudChrType.Value;
            else
                nudChrType.Value = Player.ChrType;

            if (cbxFreezeTeamType.Checked)
                Player.TeamType = (int)nudTeamType.Value;
            else
                nudTeamType.Value = Player.TeamType;

            if (cbxFreezeInvadeType.Checked)
                Player.InvadeType = (byte)nudInvadeType.Value;
            else
                nudInvadeType.Value = Player.InvadeType;

            try
            {
                DSRPlayer.Position pos = Player.GetPosition();
                nudPosX.Value = (decimal)pos.X;
                nudPosY.Value = (decimal)pos.Y;
                nudPosZ.Value = (decimal)pos.Z;
                nudPosAngle.Value = angleToDegree(pos.Angle);

                pos = Hook.GetStablePosition();
                nudStableX.Value = (decimal)pos.X;
                nudStableY.Value = (decimal)pos.Y;
                nudStableZ.Value = (decimal)pos.Z;
                nudStableAngle.Value = angleToDegree(pos.Angle);
            }
            catch (Exception ex) when (ex is OverflowException || ex is ArgumentOutOfRangeException)
            {
                nudPosX.Value = 0;
                nudPosY.Value = 0;
                nudPosZ.Value = 0;
                nudPosAngle.Value = 0;

                nudStableX.Value = 0;
                nudStableY.Value = 0;
                nudStableZ.Value = 0;
                nudStableAngle.Value = 0;
            }

            cbxDeathCam.Checked = Hook.DeathCam;
            cbxAutoSave.Checked = Hook.AutoSave;

            if (cbxSpeed.Checked)
                Player.AnimSpeed = (float)nudSpeed.Value;
   
            updateDropdown<DSRInvasion>(cmbInvasionSelect, Player.InvadeType);

            updateTeam(cmbChrSelect, Player.ChrType, Player.TeamType);

 

            updateAreaID(cmbMPAreaID, cbxFreezeMPAreaID, Player.MPAreaID, value => Player.MPAreaID = value);
            updateAreaID(cmbAreaID, cbxFreezeAreaID, Player.AreaID, value => Player.AreaID = value);

            //manage unknown warps and current warps that are not in filter
            int bonfireID = Hook.LastBonfire;

            if (lastSetBonfire.ID != bonfireID) // lastSetBonfire does not match game LastBonfire
            {
                //target warp is not in filter
                DSRBonfire result = DSRBonfire.All.FirstOrDefault(b => b.ID == bonfireID); //check if warp is in bonfire resource
                if (result == null)
                {
                    //bonfire not in filter. Add to filter as unknown
                    result = new DSRBonfire(bonfireID, $"Unknown {bonfireID}");
                    DSRBonfire.All.Add(result);
                    FilterBonfires();
                }

                //manage lastSetBonfire
                cmbBonfire.Items.Remove(lastSetBonfire); //remove from filter (if there)

                lastSetBonfire.ID = result.ID;
                lastSetBonfire.Name = "Last Set: " + result.Name;

                cmbBonfire.Items.Add(lastSetBonfire); //add to end of filter
                cmbBonfire.SelectedItem = lastSetBonfire;
                //AddLastSetBonfire();
            }
        }

        private void cbxFreezePos_CheckedChanged(object sender, EventArgs e)
        {
            Player.SetPosLock(cbxFreezePos.Checked);
            nudPosX.Enabled = cbxFreezePos.Checked;
            nudPosY.Enabled = cbxFreezePos.Checked;
            nudPosZ.Enabled = cbxFreezePos.Checked;
        }

        private void nudPos_ValueChanged(object sender, EventArgs e)
        {
            if (cbxFreezePos.Checked)
            {
                float x = (float)nudPosX.Value;
                float y = (float)nudPosY.Value;
                float z = (float)nudPosZ.Value;
                Player.PosWarpLock(x, y, z);
            }
        }

        private void nudHealth_ValueChanged(object sender, EventArgs e)
        {
            if (loaded && !reading)
                Player.Hp = (int)nudHealth.Value;
        }

        private void nudStamina_ValueChanged(object sender, EventArgs e)
        {
            if (loaded && !reading)
                Player.Stamina = (int)nudStamina.Value;
        }

        private void btnResetMagicQuantity_Click(object sender, EventArgs e)
        {
            resetMagicQuantity();
        }

        private void btnLeaveSession_Click(object sender, EventArgs e)
        {
            if (loaded && !reading)
                Hook.LeaveSession();
        }

        private void resetMagicQuantity()
        {
            if (loaded && !reading)
            {
                Hook.ResetMagicQuantity();
            }
        }

        private void nudChrType_ValueChanged(object sender, EventArgs e)
        {
            if (loaded && !reading)
                Player.ChrType = (int)nudChrType.Value;
        }

        private void nudTeamType_ValueChanged(object sender, EventArgs e)
        {
            if (loaded && !reading)
                Player.TeamType = (int)nudTeamType.Value;
        }

        private void nudInvadeType_ValueChanged(object sender, EventArgs e)
        {
            if (loaded && !reading)
                Player.InvadeType = (byte)nudInvadeType.Value;
        }

        private void cmbChrSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loaded && !reading)
            {
                DSRTeam item = cmbChrSelect.SelectedItem as DSRTeam;
                Player.ChrType = item.ChrType;
                Player.TeamType = item.TeamType;
            }
        }

        private void cmbInvasionSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loaded && !reading)
            {
                DSRInvasion item = cmbInvasionSelect.SelectedItem as DSRInvasion;
                Player.InvadeType = (byte)item.ID;
            }
        }

        private void cmbMPAreaID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loaded && !reading)
            {
                DSRArea item = cmbMPAreaID.SelectedItem as DSRArea;
                Player.MPAreaID = item.AreaID;
            }
        }

        private void cmbAreaID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loaded && !reading)
            {
                DSRArea item = cmbAreaID.SelectedItem as DSRArea;
                Player.AreaID = item.AreaID;
            }
        }

        private void btnPosStore_Click(object sender, EventArgs e)
        {
            if (loaded)
                storePosition();
        }

        private void storePosition()
        {
            if (btnPosStore.Enabled)
            {
                var pos = new SavedPos();
                pos.Name = storedPositions.Text;
                pos.X = nudStoredX.Value = nudPosX.Value;
                pos.Y = nudStoredY.Value = nudPosY.Value;
                pos.Z = nudStoredZ.Value = nudPosZ.Value;
                pos.Angle = nudStoredAngle.Value = nudPosAngle.Value;
                playerState.HP = (int)nudHealth.Value;
                playerState.Stamina = (int)nudStamina.Value;
                playerState.FollowCam = Hook.DumpFollowCam();
                playerState.DeathCam = Hook.DeathCam;
                playerState.EquipMagicData = Hook.EquipMagicData;
                playerState.Set = true;
                pos.PlayerState = playerState;
                ProcessSavedPos(pos);
                UpdatePositions();
                SavedPos.Save(Positions);
            }
        }

        private void btnPosRestore_Click(object sender, EventArgs e)
        {
            if (loaded)
                restorePosition();
        }

        private void restorePosition()
        {
            DSRPlayer.Position pos;
            pos.X = (float)nudStoredX.Value;
            pos.Y = (float)nudStoredY.Value;
            pos.Z = (float)nudStoredZ.Value;
            pos.Angle = degreeToAngle(nudStoredAngle.Value);
            Player.PosWarp(pos);

            if (playerState.Set)
            {
                // Two frames for safety, wait until after warp
                System.Threading.Thread.Sleep(1000 / 15);
                Hook.UndumpFollowCam(playerState.FollowCam);

                if (cbxRestoreState.Checked)
                {
                    nudHealth.Value = playerState.HP;
                    nudStamina.Value = playerState.Stamina;
                    cbxDeathCam.Checked = playerState.DeathCam;
                    Hook.EquipMagicData = playerState.EquipMagicData;
                }
            }
        }

        private void btnTeleportBloodstain_Click(object sender, EventArgs e)
        {
            teleportBloodstain();
        }

        private void teleportBloodstain()
        {
            if (loaded && !reading)
            {
                Player.PosWarp(Hook.GetLastBloodstainPosition());
            }
        }

        private void btnTeleportInitialPosition_Click(object sender, EventArgs e)
        {
            teleportInitialPosition();
        }

        private void teleportInitialPosition()
        {
            if (loaded && !reading)
            {
                Player.PosWarp(Hook.GetInitialPosition());
            }
        }

        private void cbxGravity_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded)
                Hook.NoGravity = !cbxGravity.Checked;
        }

        private void cbxCollision_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded)
                Hook.NoCollision = !cbxCollision.Checked;
        }

        private void cbxDeathCam_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded && !reading)
                Hook.DeathCam = cbxDeathCam.Checked;
        }

        private void cbxAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded && !reading)
                Hook.AutoSave = cbxAutoSave.Checked;
        }


        private void btnWarp_Click(object sender, EventArgs e)
        {
            if (loaded)
            {
                DSRBonfire bonfire = cmbBonfire.SelectedItem as DSRBonfire;
                //hook warp entityID
                Hook.LastBonfire = bonfire.ID;
                _ = ChangeColorWarp(Color.DarkGray);
                Hook.BonfireWarp();
            }
        }

        private void cbxSpeed_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded && !cbxSpeed.Checked)
                Player.AnimSpeed = 1;
        }

        // Sets a number box without crashing when the game reports a value outside the box's range
        private static void setClamped(NumericUpDown nud, decimal value)
        {
            if (value < nud.Minimum)
                value = nud.Minimum;
            else if (value > nud.Maximum)
                value = nud.Maximum;
            nud.Value = value;
        }

        private decimal angleToDegree(float angle)
        {
            return (decimal)((angle + Math.PI) / (Math.PI * 2) * 360);
        }

        private float degreeToAngle(decimal degree)
        {
            return (float)((double)degree / 360 * (Math.PI * 2) - Math.PI);
        }

        private void updateTeam(ComboBox cmbChrSelect, int chrType, int teamType)
        {
            // TODO: fix changing team when nud is frozen
            DSRTeam lastTeam = cmbChrSelect.SelectedItem as DSRTeam;
            if (!cmbChrSelect.DroppedDown && (lastTeam.ChrType != chrType || lastTeam.TeamType != teamType))
            {
                bool found = false;
                foreach (DSRTeam item in cmbChrSelect.Items)
                {
                    if (item.ChrType == chrType && item.TeamType == teamType)
                    {
                        cmbChrSelect.SelectedItem = item;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    DSRTeam item = new DSRTeam("Unknown", chrType, teamType);
                    cmbChrSelect.Items.Add(item);
                    cmbChrSelect.SelectedItem = item;
                }
            }
        }

        private void updateAreaID(ComboBox cmbAreaID, CheckBox cbxFreezeAreaID, int areaID, Action<int> setHook)
        {
            DSRArea lastAreaID = cmbAreaID.SelectedItem as DSRArea;
            if (cbxFreezeAreaID.Checked)
            {
                if (lastAreaID != null)
                    setHook(lastAreaID.AreaID);
            }
            else
            {
                if (!cmbAreaID.DroppedDown && lastAreaID.AreaID != areaID)
                {
                    bool found = false;
                    foreach (DSRArea item in cmbAreaID.Items)
                    {
                        if (item.AreaID == areaID)
                        {
                            cmbAreaID.SelectedItem = item;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        DSRArea item = new DSRArea(areaID.ToString(), areaID);
                        cmbAreaID.Items.Add(item);
                        cmbAreaID.SelectedItem = item;
                    }
                }
            }
        }

        private void txtBonfireSearch_TextChanged(object sender, EventArgs e)
        {
            FilterBonfires();
        }

        private void txtBonfireSearch_Click(object sender, EventArgs e)
        {
            txtBonfireSearch.SelectAll();
            txtBonfireSearch.Focus();
        }

        private void KeyDownListbox(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                e.Handled = true;

                if (cmbBonfire.SelectedIndex < cmbBonfire.Items.Count - 1)
                {
                    cmbBonfire.SelectedIndex += 1;
                    return;
                }
            }

            if (e.KeyCode == Keys.Up)
            {
                e.Handled = true;

                if (cmbBonfire.SelectedIndex != 0)
                {
                    cmbBonfire.SelectedIndex -= 1;
                    return;
                }
            }

            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                btnWarp_Click(null, null);
                return;
            }
        }

        private async Task ChangeColorWarp(Color new_color)
        {
            btnWarp.BackColor = new_color;

            await Task.Delay(TimeSpan.FromSeconds(.25));

            btnWarp.BackColor = default(Color);
        }

        private void KeyPressedBonfire(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                txtBonfireSearch.Clear();

            KeyDownListbox(e);
        }

        private DSRBonfire lastSetBonfire;

        private void FilterBonfires()
        {
            //warp filter management

            cmbBonfire.Items.Clear();
            cmbBonfire.SelectedItem = null;

            //go through bonfire resource and add to filter
            foreach (DSRBonfire bonfire in DSRBonfire.All)
            {
                if (bonfire.ToString().ToLower().Contains(txtBonfireSearch.Text.ToLower()))
                {
                    cmbBonfire.Items.Add(bonfire);
                }
            }

            cmbBonfire.Items.Add(lastSetBonfire); //add lastSetBonfire to end of filter

            cmbBonfire.SelectedIndex = 0;

            if (txtBonfireSearch.Text == "")
                lblBonfireSearch.Visible = true;
            else
                lblBonfireSearch.Visible = false;
        }

        //Only when item selected from combobox (Or arrow keys while it's in focus.)
        private void cmbBonfire_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (Hook.Loaded && cbxQuickSelectBonfire.Checked)
                Hook.LastBonfire = ((DSRBonfire)cmbBonfire.SelectedItem).ID;
        }

        private void storedPositions_SelectedIndexChanged(object sender, EventArgs e)
        {
            var savedPos = storedPositions.SelectedItem as SavedPos;
            nudStoredX.Value = savedPos.X;
            nudStoredY.Value = savedPos.Y;
            nudStoredZ.Value = savedPos.Z;
            nudStoredAngle.Value = savedPos.Angle;
            playerState = savedPos.PlayerState;
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            RemoveSavedPos();
        }

        private void storedPositions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                StorePosition();
            }

            if (e.KeyCode == Keys.Delete && e.Shift == true)
            {
                deleteButton_Click(sender, e);
            }
        }
        public void RemoveSavedPos()
        {
            if (Positions.Any(n => n.Name == storedPositions.Text))
            {
                if (MessageBox.Show("Are you sure you want to delete this positon?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    var old = Positions.Single(n => n.Name == storedPositions.Text);
                    Positions.Remove(old);
                    storedPositions.SelectedIndex = 0;
                    UpdatePositions();
                    SavedPos.Save(Positions);
                }
            }
        }
        public void StorePosition()
        {
            if (btnPosStore.Enabled)
            {

                var pos = new SavedPos();
                pos.Name = storedPositions.Text;
                pos.X = nudStoredX.Value = nudPosX.Value;
                pos.Y = nudStoredY.Value = nudPosY.Value;
                pos.Z = nudStoredZ.Value = nudPosZ.Value;
                pos.Angle = nudStoredAngle.Value = nudPosAngle.Value;
                playerState.HP = (int)nudHealth.Value;
                playerState.Stamina = (int)nudStamina.Value;
                playerState.FollowCam = Hook.DumpFollowCam();
                playerState.DeathCam = Hook.DeathCam;
                playerState.EquipMagicData = Hook.EquipMagicData;
                playerState.Set = true;
                pos.PlayerState = playerState;
                ProcessSavedPos(pos);
                UpdatePositions();
                SavedPos.Save(Positions);
            }
        }
        public void ProcessSavedPos(SavedPos pos)
        {
            if (!string.IsNullOrWhiteSpace(storedPositions.Text))
            {
                if (Positions.Any(n => n.Name == storedPositions.Text))
                {
                    var old = Positions.Single(n => n.Name == storedPositions.Text);
                    Positions.Remove(old);
                    Positions.Add(pos);
                    return;
                }

                Positions.Add(pos);
            }
        }
        private void UpdatePositions()
        {
            if (storedPositions.SelectedItem != new SavedPos())
            {
                storedPositions.Items.Clear();
                storedPositions.Items.Add(new SavedPos());
                foreach (var item in Positions)
                {
                    storedPositions.Items.Add(item);
                }
            }
        }
    }
}
