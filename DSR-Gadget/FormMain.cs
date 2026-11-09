using DSR_Gadget.List_Items;
using Octokit;
using Semver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Forms;

namespace DSR_Gadget
{
    public partial class FormMain : Form
    {
        private static Properties.Settings settings = Properties.Settings.Default;

        private DSRHook Hook;
        private bool loaded = false;
        private bool reading = false;
        private List<Control> criticalControls;
        private List<Control> lockedControls;

        public FormMain()
        {
            InitializeComponent();
            Hook = new DSRHook(5000, 5000);
            Hook.OnHooked += DsrProcess_OnHooked;
            Hook.OnUnhooked += DsrProcess_OnUnhooked;
            Hook.Start();
            criticalControls = new List<Control>
            {
                nudHealth, nudStamina, btnPosStore, btnPosRestore, cbxDeathCam, btnWarp, cmbBonfire,
                btnCreate, btnEventRead, btnEventWrite, nudChrType, nudTeamType, nudInvadeType, cbxFreezeChrType,
                cbxFreezeTeamType, cbxFreezeInvadeType, cmbChrSelect, cmbInvasionSelect, cmbAreaID, cmbMPAreaID,
                cbxFreezeMPAreaID, cbxFreezeAreaID, btnTeleportBloodstain, btnTeleportInitialPosition,
                btnResetMagicQuantity, btnLeaveSession, btnCurrentPlayerKick, cmbSosSummonType, nudSosPosX,
                nudSosPosY, nudSosPosZ, nudSosPosAngle, btnSosRestorePos, cbxCurrentPlayerCamera, cbxFreezePos,
                btnCurrentPlayerTeleport, btnCurrentPlayerFamilyShare, nudStoredX, nudStoredY, nudStoredZ, nudStoredAngle,
                pnlHairColor, pnlEyeColor//nudIndictments,
            };

            cmbGestures = new ComboBox[] { cmbGestureSlot1, cmbGestureSlot2, cmbGestureSlot3, cmbGestureSlot4,
                cmbGestureSlot5, cmbGestureSlot6, cmbGestureSlot7 };
            cbxGestures = new CheckBox[] { cbxGesturePointForward, cbxGesturePointUp, cbxGesturePointDown,
                cbxGestureBeckon, cbxGestureWave, cbxGestureBow, cbxGestureProperBow, cbxGestureHurrah,
                cbxGestureJoy, cbxGestureShrug, cbxGestureLookSkyward, cbxGestureWellWhatIsIt, cbxGestureProstration,
                cbxGesturePrayer, cbxGesturePraiseTheSun};
            
            criticalControls.AddRange(cbxGestures);
            criticalControls.AddRange(cmbGestures);
            
            lockedControls = new List<Control>()
            {
                cmbCovenant, nudWarriorOfSunlight, nudDarkwraith, nudPathOfTheDragon, nudGravelordServant, nudForestHunter, nudDarkmoonBlade, nudChaosServant,
                txtName, nudWeaponMemory,  cmbGender, cmbPhysique, btnGesturesUnlockAll, nudClearCount, nudIndictments, nudVitality, nudAttunement, nudEndurance,
                nudStrength, nudDexterity, nudResistance, nudIntelligence, nudFaith, nudHumanity, nudSouls, cmbClass, btnApplyHair
            };
        }

        private void enableCriticalControls(bool enable)
        {
            foreach (Control ctrl in criticalControls)
                ctrl.Enabled = enable;
            
            if (Hook.Loaded)
                foreach (Control ctrl in lockedControls)
                    ctrl.Enabled = cbxUnlockFeatures.Checked;
            else
                foreach (Control ctrl in lockedControls)
                    ctrl.Enabled = false;

            EnableMiscStats(enable);

        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            Location = settings.WindowLocation;
            Text = "DSR Gadget Local Loader " + System.Windows.Forms.Application.ProductVersion;
            enableCriticalControls(false);
            initializeAll();

            llbUpdate.Visible = false;
            GitHubClient gitHubClient = new GitHubClient(new ProductHeaderValue("DSR-Gadget-Local-Loader"));
            try
            {
                Release release = await gitHubClient.Repository.Release.GetLatest("Nordgaren", "DSR-Gadget-Local-Loader");
                if (SemVersion.Parse(release.TagName) > System.Windows.Forms.Application.ProductVersion)
                {
                    lblUpdate.Text = "New version available!";
                    LinkLabel.Link link = new LinkLabel.Link();
                    link.LinkData = release.HtmlUrl;
                    llbUpdate.Links.Add(link);
                    llbUpdate.Visible = true;
                }
                else
                {
                    lblUpdate.Text = "App up to date";
                }
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is ApiException || ex is ArgumentException)
            {
                lblUpdate.Text = "Current app version unknown";
            }
        }

        private void llbUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
                settings.WindowLocation = Location;
            else
                settings.WindowLocation = RestoreBounds.Location;

            Player.SetPosLock(false);
            saveAll();
            resetAll();
        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if (Hook.Hooked)
            {
                if (Hook.Loaded)
                {
                    if (!loaded)
                    {
                        lblLoadedValue.Text = "Yes";
                        loaded = true;
                        reloadAll();
                        enableCriticalControls(true);
                    }
                    else
                    {
                        updateAll();
                    }
                }
                else if (loaded)
                {
                    lblLoadedValue.Text = "No";
                    enableCriticalControls(false);
                    loaded = false;
                }
            }
        }

        private void DsrProcess_OnHooked(object sender, PropertyHook.PHEventArgs e)
        {
            Invoke(new Action(() =>
            {
                lblVersionValue.Text = Hook.Version;
            }));
            onHookedMisc();
        }

        private void DsrProcess_OnUnhooked(object sender, PropertyHook.PHEventArgs e)
        {
            Invoke(new Action(() =>
            {
                lblVersionValue.Text = "None";
                lblLoadedValue.Text = "No";
                enableCriticalControls(false);
                loaded = false;
            }));
        }

        private void initializeAll()
        {
            reading = true;
            initPlayer();
            initStats();
            initItems();
            initCheats();
            initGraphics();
            initExtras();
            initParams();
            initMisc();
            initHotkeys();
            initInfo();
            reading = false;
        }

        private void reloadAll()
        {
            reading = true;
            reloadPlayer();
            reloadStats();
            reloadItems();
            reloadCheats();
            reloadGraphics();
            reloadExtras();
            reloadParams();
            reloadMisc();
            reloadHotkeys();
            reloadInfo();
            reading = false;
        }

        private void updateAll()
        {
            reading = true;
            updatePlayer();
            updateStats();
            updateItems();
            updateCheats();
            updateGraphics();
            updateExtras();
            updateParams();
            updateMisc();
            updateHotkeys();
            updateInfo();
            reading = false;
        }

        private void saveAll()
        {
            savePlayer();
            saveStats();
            saveItems();
            saveCheats();
            saveGraphics();
            saveExtras();
            saveParams();
            saveMisc();
            saveHotkeys();
            saveInfo();
        }

        private void resetAll()
        {
            resetPlayer();
            resetStats();
            resetItems();
            resetCheats();
            resetGraphics();
            resetExtras();
            resetParams();
            resetMisc();
            resetHotkeys();
            resetInfo();
        }

        private void updateDropdown<TDSRProperty>(ComboBox cmb, int id) where TDSRProperty : IDSRProperty, new()
        {
            IDSRProperty lastID = cmb.SelectedItem as IDSRProperty;
            if (!cmb.DroppedDown && lastID.ID != id)
            {
                bool found = false;
                foreach (IDSRProperty property in cmb.Items)
                {
                    if (property.ID == id)
                    {
                        cmb.SelectedItem = property;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    TDSRProperty property = new TDSRProperty();
                    property.Name = id + ": Unknown";
                    property.ID = id;
                    cmb.Items.Add(property);
                    cmb.SelectedItem = property;
                }
            }
        }

    }
}
