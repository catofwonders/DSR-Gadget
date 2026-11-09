using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DSR_Gadget
{
    // Live param editor: pick a param, pick a row, edit its fields while the game runs.
    // Changes are made in the game's memory only and last until the game is restarted.
    // Field layouts come from Paramdex (Resources/Params/Defs), row names from Resources/Params/Names.
    public partial class FormMain : Form
    {
        private TabPage tpgParams;
        private ComboBox cmbParam;
        private TextBox txtParamSearch;
        private ListBox lbxParamRows;
        private DataGridView dgvParamFields;
        private CheckBox cbxParamShowPadding;
        private Button btnParamReload, btnParamResetRow, btnParamResetAll;
        private Label lblParamStatus;

        private List<LiveParam> liveParams;
        private LiveParam currentParam;
        private LiveParamRow currentRow;
        private bool paramGridFilling = false;
        private Font paramBoldFont;

        // Original bytes of every row changed this session, so it can be put back
        private readonly Dictionary<string, Dictionary<int, byte[]>> paramOriginals = new Dictionary<string, Dictionary<int, byte[]>>();
        private readonly Dictionary<string, Dictionary<int, string>> paramNameCache = new Dictionary<string, Dictionary<int, string>>();

        private void initParams()
        {
            tpgParams = new TabPage("Params") { UseVisualStyleBackColor = true, Padding = new Padding(3) };

            // --- Top: param picker and search ---
            Panel top = new Panel() { Dock = DockStyle.Top, Height = 58 };
            top.Controls.Add(new Label() { Text = "Param:", Location = new Point(3, 6), AutoSize = true });
            cmbParam = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(55, 3), Width = 240, MaxDropDownItems = 25 };
            cmbParam.SelectedIndexChanged += (s, e) => selectParam(cmbParam.SelectedItem as LiveParam);
            top.Controls.Add(cmbParam);
            btnParamReload = new Button() { Text = "Reload", Location = new Point(300, 2), Width = 84, UseVisualStyleBackColor = true };
            btnParamReload.Click += (s, e) => loadParams(true);
            top.Controls.Add(btnParamReload);
            top.Controls.Add(new Label() { Text = "Search:", Location = new Point(3, 35), AutoSize = true });
            txtParamSearch = new TextBox() { Location = new Point(55, 32), Width = 329 };
            txtParamSearch.TextChanged += (s, e) => fillParamRows();
            top.Controls.Add(txtParamSearch);

            // --- Bottom: buttons and status ---
            Panel bottom = new Panel() { Dock = DockStyle.Bottom, Height = 52 };
            btnParamResetRow = new Button() { Text = "Reset row", Location = new Point(3, 3), Width = 90, UseVisualStyleBackColor = true };
            btnParamResetRow.Click += (s, e) => resetCurrentParamRow();
            bottom.Controls.Add(btnParamResetRow);
            btnParamResetAll = new Button() { Text = "Reset all changes", Location = new Point(97, 3), Width = 120, UseVisualStyleBackColor = true };
            btnParamResetAll.Click += (s, e) => resetAllParamChanges();
            bottom.Controls.Add(btnParamResetAll);
            cbxParamShowPadding = new CheckBox() { Text = "Show padding", Location = new Point(225, 7), AutoSize = true };
            cbxParamShowPadding.CheckedChanged += (s, e) => fillParamFields();
            bottom.Controls.Add(cbxParamShowPadding);
            lblParamStatus = new Label() { Location = new Point(3, 31), Size = new Size(384, 18), AutoEllipsis = true };
            bottom.Controls.Add(lblParamStatus);

            // --- Middle: rows on top, fields below ---
            SplitContainer split = new SplitContainer() { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
            lbxParamRows = new ListBox() { Dock = DockStyle.Fill, IntegralHeight = false, HorizontalScrollbar = true };
            lbxParamRows.SelectedIndexChanged += (s, e) => selectParamRow(lbxParamRows.SelectedItem as LiveParamRow);
            split.Panel1.Controls.Add(lbxParamRows);

            dgvParamFields = new DataGridView()
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2,
                BackgroundColor = SystemColors.Window,
                ShowCellToolTips = true,
            };
            dgvParamFields.Columns.Add(new DataGridViewTextBoxColumn() { Name = "Field", HeaderText = "Field", ReadOnly = true, FillWeight = 55 });
            dgvParamFields.Columns.Add(new DataGridViewTextBoxColumn() { Name = "Type", HeaderText = "Type", ReadOnly = true, FillWeight = 15 });
            dgvParamFields.Columns.Add(new DataGridViewTextBoxColumn() { Name = "Value", HeaderText = "Value", FillWeight = 30 });
            dgvParamFields.CellEndEdit += dgvParamFields_CellEndEdit;
            dgvParamFields.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == 2 && !dgvParamFields.Rows[e.RowIndex].Cells[2].ReadOnly)
                    dgvParamFields.BeginEdit(true);
            };
            split.Panel2.Controls.Add(dgvParamFields);

            tpgParams.Controls.Add(split);
            tpgParams.Controls.Add(top);
            tpgParams.Controls.Add(bottom);
            split.SplitterDistance = 170;

            addTabWithWrapping(tpgParams);
            tclMain.SelectedIndexChanged += (s, e) =>
            {
                if (tclMain.SelectedTab == tpgParams && liveParams == null)
                    loadParams(false);
            };

            setParamStatus("Open this tab with the game running to load its params.");
            updateParamButtons();
        }

        // Adds a tab; if the tab headers no longer fit on one line, wraps them onto rows and grows the window
        private void addTabWithWrapping(TabPage page)
        {
            IntPtr tabHandle = tclMain.Handle;
            int oneRowTop = tclMain.DisplayRectangle.Top;
            tclMain.TabPages.Add(page);
            if (!tclMain.Multiline)
            {
                tclMain.Multiline = true;
                int extraHeight = tclMain.DisplayRectangle.Top - oneRowTop;
                if (extraHeight > 0)
                    ClientSize = new Size(ClientSize.Width, ClientSize.Height + extraHeight);
                else
                    tclMain.Multiline = false;
            }
        }

        private void saveParams() { }
        private void reloadParams() { }
        private void updateParams() { }
        private void resetParams() { }

        private void setParamStatus(string text)
        {
            lblParamStatus.Text = text;
        }

        private void updateParamButtons()
        {
            bool haveRow = currentParam != null && currentRow != null;
            btnParamResetRow.Enabled = haveRow && rowIsChanged(currentParam, currentRow);
            btnParamResetAll.Enabled = paramOriginals.Values.Any(d => d.Count > 0);
        }

        private Dictionary<int, string> loadRowNames(string file)
        {
            if (paramNameCache.TryGetValue(file, out var cached))
                return cached;
            Dictionary<int, string> names = new Dictionary<int, string>();
            string path = Path.Combine(GetTxtResourceClass.ExeDir, "Resources", "Params", "Names", file + ".txt");
            if (File.Exists(path))
            {
                foreach (string line in File.ReadAllLines(path))
                {
                    string trimmed = line.Trim();
                    int space = trimmed.IndexOf(' ');
                    if (space > 0 && int.TryParse(trimmed.Substring(0, space), NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
                        names[id] = trimmed.Substring(space + 1).Trim();
                }
            }
            paramNameCache[file] = names;
            return names;
        }

        private void loadParams(bool userAsked)
        {
            if (!Hook.Hooked)
            {
                setParamStatus("Start the game first, then press Reload.");
                return;
            }

            string previous = currentParam?.Name;
            int? previousRow = currentRow?.ID;

            try
            {
                liveParams = Hook.LoadParams(loadRowNames);
            }
            catch (Exception ex)
            {
                liveParams = null;
                setParamStatus("Couldn't read params: " + ex.Message);
                return;
            }

            cmbParam.BeginUpdate();
            cmbParam.Items.Clear();
            foreach (LiveParam param in liveParams.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
                cmbParam.Items.Add(param);
            cmbParam.EndUpdate();

            int usable = liveParams.Count(p => p.Problem == null);
            if (usable == 0)
            {
                setParamStatus("No params found. Load into the game (past the title screen) and press Reload.");
                return;
            }

            LiveParam select = liveParams.FirstOrDefault(p => p.Name == previous && p.Problem == null)
                ?? liveParams.FirstOrDefault(p => p.Name == "EquipParamWeapon" && p.Problem == null)
                ?? liveParams.First(p => p.Problem == null);
            cmbParam.SelectedItem = select;
            if (previousRow != null)
                selectRowById(previousRow.Value);
            setParamStatus($"Loaded {usable} params. Changes last until the game is restarted.");
        }

        private void selectParam(LiveParam param)
        {
            currentParam = param;
            currentRow = null;
            fillParamRows();
            fillParamFields();
            if (param?.Problem != null)
                setParamStatus(param.Name + ": " + param.Problem);
        }

        private void fillParamRows()
        {
            lbxParamRows.BeginUpdate();
            lbxParamRows.Items.Clear();
            if (currentParam != null)
            {
                string search = txtParamSearch.Text.Trim();
                foreach (LiveParamRow row in currentParam.Rows)
                {
                    if (search.Length == 0
                        || row.ID.ToString(CultureInfo.InvariantCulture).StartsWith(search, StringComparison.Ordinal)
                        || (row.Name != null && row.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0))
                        lbxParamRows.Items.Add(row);
                }
            }
            lbxParamRows.EndUpdate();
            if (currentRow != null && lbxParamRows.Items.Contains(currentRow))
                lbxParamRows.SelectedItem = currentRow;
        }

        private void selectRowById(int id)
        {
            foreach (LiveParamRow row in lbxParamRows.Items)
            {
                if (row.ID == id)
                {
                    lbxParamRows.SelectedItem = row;
                    return;
                }
            }
        }

        private void selectParamRow(LiveParamRow row)
        {
            currentRow = row;
            fillParamFields();
        }

        private bool checkParamUsable()
        {
            if (currentParam == null || currentRow == null || currentParam.Problem != null)
                return false;
            if (!Hook.Hooked || !Hook.ParamStillValid(currentParam))
            {
                setParamStatus("The game reloaded its params (or closed). Press Reload.");
                liveParams = null;
                return false;
            }
            return true;
        }

        private void fillParamFields()
        {
            paramGridFilling = true;
            dgvParamFields.Rows.Clear();
            try
            {
                if (!checkParamUsable())
                    return;

                byte[] bytes = Hook.ReadParamRow(currentParam, currentRow);
                byte[] original = getOriginal(currentParam, currentRow);
                foreach (ParamField field in currentParam.Def.Fields)
                {
                    if (field.IsPadding && !cbxParamShowPadding.Checked)
                        continue;
                    if (field.Type == "dummy8" && field.IsBitfield && !cbxParamShowPadding.Checked)
                        continue;

                    int index = dgvParamFields.Rows.Add(field.Name, field.TypeLabel, field.Read(bytes));
                    DataGridViewRow gridRow = dgvParamFields.Rows[index];
                    gridRow.Tag = field;
                    bool editable = !field.IsPadding && !field.IsText;
                    gridRow.Cells[2].ReadOnly = !editable;
                    if (!editable)
                        gridRow.Cells[2].Style.ForeColor = SystemColors.GrayText;
                    if (!string.IsNullOrWhiteSpace(field.Description))
                        gridRow.Cells[0].ToolTipText = field.Description;
                    markParamCell(gridRow, field, bytes, original);
                }
                setParamStatus($"{currentParam.Name} row {currentRow.ID}" + (string.IsNullOrEmpty(currentRow.Name) ? "" : " (" + currentRow.Name + ")")
                    + ". Double-click a value to edit it.");
            }
            finally
            {
                paramGridFilling = false;
                updateParamButtons();
            }
        }

        // Highlights values that differ from the game's original
        private void markParamCell(DataGridViewRow gridRow, ParamField field, byte[] bytes, byte[] original)
        {
            bool changed = original != null && field.Read(original) != field.Read(bytes);
            gridRow.Cells[2].Style.BackColor = changed ? Color.LightGoldenrodYellow : SystemColors.Window;
            if (paramBoldFont == null)
                paramBoldFont = new Font(dgvParamFields.Font, FontStyle.Bold);
            gridRow.Cells[2].Style.Font = changed ? paramBoldFont : null;
            if (changed)
                gridRow.Cells[2].ToolTipText = "Original: " + field.Read(original);
            else
                gridRow.Cells[2].ToolTipText = "";
        }

        private void dgvParamFields_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (paramGridFilling || e.RowIndex < 0 || e.ColumnIndex != 2)
                return;
            DataGridViewRow gridRow = dgvParamFields.Rows[e.RowIndex];
            ParamField field = gridRow.Tag as ParamField;
            if (field == null || !checkParamUsable())
                return;

            // Work on fresh bytes from the game, so nothing else in the row is overwritten with stale data
            byte[] bytes = Hook.ReadParamRow(currentParam, currentRow);
            string typed = Convert.ToString(gridRow.Cells[2].Value, CultureInfo.InvariantCulture);
            if (typed == field.Read(bytes))
                return;

            rememberOriginal(currentParam, currentRow, bytes);
            string error = field.Write(bytes, typed);
            if (error != null)
            {
                paramGridFilling = true;
                gridRow.Cells[2].Value = field.Read(Hook.ReadParamRow(currentParam, currentRow));
                paramGridFilling = false;
                setParamStatus(field.Name + ": " + error);
                MessageBox.Show(error, field.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Hook.WriteParamRow(currentParam, currentRow, bytes))
            {
                setParamStatus("Couldn't write to the game's memory.");
                return;
            }

            byte[] after = Hook.ReadParamRow(currentParam, currentRow);
            paramGridFilling = true;
            gridRow.Cells[2].Value = field.Read(after);
            markParamCell(gridRow, field, after, getOriginal(currentParam, currentRow));
            paramGridFilling = false;
            setParamStatus($"Set {field.Name} to {field.Read(after)}.");
            updateParamButtons();
        }

        private void rememberOriginal(LiveParam param, LiveParamRow row, byte[] bytes)
        {
            if (!paramOriginals.TryGetValue(param.Name, out var rows))
                paramOriginals[param.Name] = rows = new Dictionary<int, byte[]>();
            if (!rows.ContainsKey(row.ID))
                rows[row.ID] = (byte[])bytes.Clone();
        }

        private byte[] getOriginal(LiveParam param, LiveParamRow row)
        {
            if (paramOriginals.TryGetValue(param.Name, out var rows) && rows.TryGetValue(row.ID, out byte[] bytes))
                return bytes;
            return null;
        }

        private bool rowIsChanged(LiveParam param, LiveParamRow row)
        {
            byte[] original = getOriginal(param, row);
            return original != null && Hook.Hooked && !original.SequenceEqual(Hook.ReadParamRow(param, row));
        }

        private void resetCurrentParamRow()
        {
            if (!checkParamUsable())
                return;
            byte[] original = getOriginal(currentParam, currentRow);
            if (original != null)
                Hook.WriteParamRow(currentParam, currentRow, original);
            fillParamFields();
            setParamStatus($"Row {currentRow.ID} is back to the game's original values.");
        }

        private void resetAllParamChanges()
        {
            if (liveParams == null || !Hook.Hooked)
                return;
            if (MessageBox.Show("Put every param you changed this session back to the game's original values?",
                "Reset all changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int count = 0;
            foreach (LiveParam param in liveParams)
            {
                if (param.Problem != null || !paramOriginals.TryGetValue(param.Name, out var rows) || !Hook.ParamStillValid(param))
                    continue;
                foreach (LiveParamRow row in param.Rows)
                {
                    if (rows.TryGetValue(row.ID, out byte[] original))
                    {
                        Hook.WriteParamRow(param, row, original);
                        count++;
                    }
                }
            }
            fillParamFields();
            setParamStatus($"Reset {count} row(s) to the game's original values.");
        }
    }
}
