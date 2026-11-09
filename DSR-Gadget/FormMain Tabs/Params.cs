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
    // Field layouts and row names come from Paramdex (Resources/Params/Defs, Names);
    // English field names, descriptions, value lists and references come from Smithbox (Resources/Params/Smithbox).
    public partial class FormMain : Form
    {
        private TabPage tpgParams;
        private ComboBox cmbParam;
        private TextBox txtParamSearch, txtParamFieldFind, txtParamHelp;
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

        private class ParamGridTag
        {
            public ParamField Field;
            public ParamFieldHelp Help;
            public string DisplayName;
            public Dictionary<string, long> Choices;   // dropdown text -> raw value, for value lists and yes/no fields
        }

        // Friendly names shown in the param dropdown
        private static readonly Dictionary<string, string> paramFriendlyNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["EquipParamWeapon"] = "Weapons",
            ["EquipParamProtector"] = "Armor",
            ["EquipParamAccessory"] = "Rings",
            ["EquipParamGoods"] = "Items (consumables, keys, souls)",
            ["ReinforceParamWeapon"] = "Weapon upgrade levels",
            ["ReinforceParamProtector"] = "Armor upgrade levels",
            ["NpcParam"] = "Enemies and NPCs",
            ["AtkParam_Npc"] = "Enemy attacks",
            ["AtkParam_Pc"] = "Player attacks",
            ["NpcThinkParam"] = "Enemy AI",
            ["ObjectParam"] = "Objects",
            ["Bullet"] = "Projectiles",
            ["BehaviorParam"] = "Enemy behaviors",
            ["BehaviorParam_PC"] = "Player behaviors",
            ["Magic"] = "Spells",
            ["SpEffectParam"] = "Special effects (buffs, debuffs, statuses)",
            ["SpEffectVfxParam"] = "Special effect visuals",
            ["TalkParam"] = "NPC dialogue",
            ["MenuColorTableParam"] = "Menu colors",
            ["ItemLotParam"] = "Item drops and pickups",
            ["MoveParam"] = "Movement speeds",
            ["CharaInitParam"] = "Starting classes and NPC loadouts",
            ["EquipMtrlSetParam"] = "Upgrade material costs",
            ["FaceGenParam"] = "Faces",
            ["RagdollParam"] = "Ragdolls",
            ["ShopLineupParam"] = "Shops",
            ["QwcChange"] = "World tendency changes",
            ["QwcJudge"] = "World tendency checks",
            ["GameAreaParam"] = "Areas (boss soul rewards)",
            ["SkeletonParam"] = "Skeletons",
            ["CalcCorrectGraph"] = "Stat scaling curves",
            ["LockCamParam"] = "Lock-on camera",
            ["ObjActParam"] = "Object actions (doors, levers, chests)",
            ["HitMtrlParam"] = "Surface materials",
            ["KnockBackParam"] = "Knockback",
            ["LevelSyncParam"] = "Level sync (multiplayer)",
            ["CoolTimeParam"] = "Cooldowns",
            ["WhiteCoolTimeParam"] = "Summon cooldowns",
        };

        // Smithbox refers to a couple of params by slightly different names
        private static readonly Dictionary<string, string> paramRefAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["QwcChangeParam"] = "QwcChange",
            ["QwcJudgeParam"] = "QwcJudge",
        };

        private void initParams()
        {
            tpgParams = new TabPage("Params") { UseVisualStyleBackColor = true, Padding = new Padding(3) };

            // --- Top: param picker and row search ---
            Panel top = new Panel() { Dock = DockStyle.Top, Height = 58 };
            top.Controls.Add(new Label() { Text = "Param:", Location = new Point(3, 6), AutoSize = true });
            cmbParam = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(55, 3), Width = 240, MaxDropDownItems = 25, DropDownWidth = 360 };
            cmbParam.SelectedIndexChanged += (s, e) => selectParam(cmbParam.SelectedItem as LiveParam);
            top.Controls.Add(cmbParam);
            btnParamReload = new Button() { Text = "Reload", Location = new Point(300, 2), Width = 84, UseVisualStyleBackColor = true };
            btnParamReload.Click += (s, e) => loadParams();
            top.Controls.Add(btnParamReload);
            top.Controls.Add(new Label() { Text = "Find row:", Location = new Point(3, 35), AutoSize = true });
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
            cbxParamShowPadding = new CheckBox() { Text = "Show unused/padding", Location = new Point(225, 7), AutoSize = true };
            cbxParamShowPadding.CheckedChanged += (s, e) => fillParamFields();
            bottom.Controls.Add(cbxParamShowPadding);
            lblParamStatus = new Label() { Location = new Point(3, 31), Size = new Size(384, 18), AutoEllipsis = true };
            bottom.Controls.Add(lblParamStatus);

            // --- Middle: rows on top; field finder, fields and help text below ---
            SplitContainer split = new SplitContainer() { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
            lbxParamRows = new ListBox() { Dock = DockStyle.Fill, IntegralHeight = false, HorizontalScrollbar = true };
            lbxParamRows.SelectedIndexChanged += (s, e) => selectParamRow(lbxParamRows.SelectedItem as LiveParamRow);
            split.Panel1.Controls.Add(lbxParamRows);

            Panel findPanel = new Panel() { Dock = DockStyle.Top, Height = 26 };
            findPanel.Controls.Add(new Label() { Text = "Find field:", Location = new Point(0, 6), AutoSize = true });
            txtParamFieldFind = new TextBox() { Location = new Point(60, 3), Width = 320, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            txtParamFieldFind.TextChanged += (s, e) => fillParamFields();
            findPanel.Controls.Add(txtParamFieldFind);

            txtParamHelp = new TextBox()
            {
                Dock = DockStyle.Bottom,
                Height = 92,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = SystemColors.Info,
                ForeColor = SystemColors.InfoText,
            };

            dgvParamFields = new DataGridView()
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EditMode = DataGridViewEditMode.EditOnEnter,
                BackgroundColor = SystemColors.Window,
                ShowCellToolTips = true,
            };
            dgvParamFields.Columns.Add(new DataGridViewTextBoxColumn() { Name = "Field", HeaderText = "Field", ReadOnly = true, FillWeight = 45 });
            dgvParamFields.Columns.Add(new DataGridViewTextBoxColumn() { Name = "Value", HeaderText = "Value", FillWeight = 30 });
            dgvParamFields.Columns.Add(new DataGridViewTextBoxColumn() { Name = "Means", HeaderText = "Points to", ReadOnly = true, FillWeight = 25 });
            dgvParamFields.CellValueChanged += dgvParamFields_CellValueChanged;
            dgvParamFields.CurrentCellDirtyStateChanged += (s, e) =>
            {
                // Make dropdown choices take effect as soon as they're picked
                if (dgvParamFields.CurrentCell is DataGridViewComboBoxCell && dgvParamFields.IsCurrentCellDirty)
                    dgvParamFields.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            dgvParamFields.DataError += (s, e) => { e.ThrowException = false; };
            dgvParamFields.SelectionChanged += (s, e) => showParamHelp();

            split.Panel2.Controls.Add(dgvParamFields);
            split.Panel2.Controls.Add(txtParamHelp);
            split.Panel2.Controls.Add(findPanel);

            tpgParams.Controls.Add(split);
            tpgParams.Controls.Add(top);
            tpgParams.Controls.Add(bottom);
            split.SplitterDistance = 140;

            addTabWithWrapping(tpgParams);
            tclMain.SelectedIndexChanged += (s, e) =>
            {
                if (tclMain.SelectedTab == tpgParams && liveParams == null)
                    loadParams();
            };

            txtParamHelp.Text = "Pick a param and a row, then click a value to change it. "
                + "Select a field to see what it does here.";
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

        private static string paramDisplayName(LiveParam param)
        {
            return paramFriendlyNames.TryGetValue(param.Name, out string friendly) ? $"{friendly}  ({param.Name})" : param.Name;
        }

        private void updateParamButtons()
        {
            bool haveRow = currentParam != null && currentRow != null && currentParam.Problem == null;
            btnParamResetRow.Enabled = haveRow && getOriginal(currentParam, currentRow) != null;
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

        private void loadParams()
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

            foreach (LiveParam param in liveParams)
                param.DisplayName = paramDisplayName(param);

            cmbParam.BeginUpdate();
            cmbParam.Items.Clear();
            foreach (LiveParam param in liveParams.OrderBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase))
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
                {
                    showParamHelp();
                    return;
                }

                ParamHelpInfo info = ParamHelp.For(currentParam.Def);
                byte[] bytes = Hook.ReadParamRow(currentParam, currentRow);
                byte[] original = getOriginal(currentParam, currentRow);
                string find = txtParamFieldFind.Text.Trim();

                foreach (ParamField field in currentParam.Def.Fields)
                {
                    ParamFieldHelp help = info.For(field);
                    bool padding = field.IsPadding || field.Type == "dummy8" || (help?.IsPadding ?? false);
                    if (padding && !cbxParamShowPadding.Checked)
                        continue;
                    if ((help?.IsObsolete ?? false) && !cbxParamShowPadding.Checked)
                        continue;

                    string display = !string.IsNullOrWhiteSpace(help?.Name) ? help.Name : field.Name;
                    if (find.Length > 0
                        && display.IndexOf(find, StringComparison.OrdinalIgnoreCase) < 0
                        && field.Name.IndexOf(find, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    ParamGridTag tag = new ParamGridTag() { Field = field, Help = help, DisplayName = display };
                    DataGridViewRow gridRow = new DataGridViewRow();
                    gridRow.CreateCells(dgvParamFields);
                    gridRow.Tag = tag;
                    gridRow.Cells[0].Value = display;
                    gridRow.Cells[0].ToolTipText = field.Name;

                    bool editable = !padding && !field.IsText;
                    tag.Choices = buildChoices(field, help);
                    if (editable && tag.Choices != null)
                    {
                        DataGridViewComboBoxCell combo = new DataGridViewComboBoxCell() { FlatStyle = FlatStyle.Flat, DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton };
                        foreach (string choice in tag.Choices.Keys)
                            combo.Items.Add(choice);
                        gridRow.Cells[1] = combo;
                    }
                    string current = valueText(tag, bytes);
                    if (gridRow.Cells[1] is DataGridViewComboBoxCell comboCell && !comboCell.Items.Contains(current))
                        comboCell.Items.Add(current);   // a number that isn't one of the listed choices
                    gridRow.Cells[1].Value = current;
                    gridRow.Cells[1].ReadOnly = !editable;
                    if (!editable)
                        gridRow.Cells[1].Style.ForeColor = SystemColors.GrayText;
                    gridRow.Cells[2].Value = describeReference(help, field, bytes);

                    dgvParamFields.Rows.Add(gridRow);
                    markParamCell(gridRow, tag, bytes, original);
                }

                setParamStatus($"{currentRow.ID}" + (string.IsNullOrEmpty(currentRow.Name) ? "" : " " + currentRow.Name)
                    + " - click a value to change it.");
            }
            finally
            {
                paramGridFilling = false;
                updateParamButtons();
                showParamHelp();
            }
        }

        // Dropdown choices for value lists and yes/no fields, or null for plain numbers
        private static Dictionary<string, long> buildChoices(ParamField field, ParamFieldHelp help)
        {
            List<KeyValuePair<long, string>> options = ParamHelp.Enum(help?.Enum);
            if (options != null && options.Count > 0)
            {
                Dictionary<string, long> choices = new Dictionary<string, long>();
                foreach (var option in options)
                {
                    string text = $"{option.Value} ({option.Key})";
                    if (!choices.ContainsKey(text))
                        choices[text] = option.Key;
                }
                return choices;
            }
            if ((help?.IsBool ?? false) || (field.IsBitfield && field.BitSize == 1))
                return new Dictionary<string, long>() { ["Yes"] = 1, ["No"] = 0 };
            return null;
        }

        // The text shown in the Value cell; adds the current number to the dropdown if it isn't a listed choice
        private string valueText(ParamGridTag tag, byte[] bytes)
        {
            string raw = tag.Field.Read(bytes);
            if (tag.Choices == null || !long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value))
                return raw;
            foreach (var choice in tag.Choices)
                if (choice.Value == value)
                    return choice.Key;
            return raw;
        }

        // For fields that hold the ID of a row in another param, shows that row's name
        private string describeReference(ParamFieldHelp help, ParamField field, byte[] bytes)
        {
            if (string.IsNullOrEmpty(help?.Refs) || liveParams == null)
                return null;
            if (!long.TryParse(field.Read(bytes), NumberStyles.Integer, CultureInfo.InvariantCulture, out long id))
                return null;
            if (id < 0)
                return "(none)";

            foreach (string part in splitRefs(help.Refs))
            {
                string name = part;
                string condition = null;
                int paren = part.IndexOf('(');
                if (paren > 0 && part.EndsWith(")"))
                {
                    name = part.Substring(0, paren).Trim();
                    condition = part.Substring(paren + 1, part.Length - paren - 2);
                }

                if (condition != null && !conditionHolds(condition, bytes))
                    continue;

                if (paramRefAliases.TryGetValue(name, out string alias))
                    name = alias;
                LiveParam target = liveParams.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase) && p.Problem == null);
                if (target == null)
                    continue;
                LiveParamRow row = target.FindRow((int)id);
                if (row != null)
                {
                    string friendly = paramFriendlyNames.TryGetValue(target.Name, out string f) ? f : target.Name;
                    return (string.IsNullOrEmpty(row.Name) ? "row " + row.ID : row.Name) + " [" + friendly + "]";
                }
            }
            return id == 0 ? null : "(no such row)";
        }

        private bool conditionHolds(string condition, byte[] bytes)
        {
            int equals = condition.IndexOf('=');
            if (equals <= 0)
                return true;
            string fieldName = condition.Substring(0, equals).Trim();
            string expected = condition.Substring(equals + 1).Trim();
            ParamField other = currentParam.Def.Fields.FirstOrDefault(f => string.Equals(f.Name, fieldName, StringComparison.OrdinalIgnoreCase));
            return other == null || other.Read(bytes) == expected;
        }

        private static IEnumerable<string> splitRefs(string refs)
        {
            int depth = 0, start = 0;
            for (int i = 0; i < refs.Length; i++)
            {
                if (refs[i] == '(') depth++;
                else if (refs[i] == ')') depth--;
                else if (refs[i] == ',' && depth == 0)
                {
                    yield return refs.Substring(start, i - start).Trim();
                    start = i + 1;
                }
            }
            if (start < refs.Length)
                yield return refs.Substring(start).Trim();
        }

        // Highlights values that differ from the game's original
        private void markParamCell(DataGridViewRow gridRow, ParamGridTag tag, byte[] bytes, byte[] original)
        {
            bool changed = original != null && tag.Field.Read(original) != tag.Field.Read(bytes);
            if (paramBoldFont == null)
                paramBoldFont = new Font(dgvParamFields.Font, FontStyle.Bold);
            gridRow.Cells[1].Style.BackColor = changed ? Color.LightGoldenrodYellow : SystemColors.Window;
            gridRow.Cells[1].Style.Font = changed ? paramBoldFont : null;
            gridRow.Cells[1].ToolTipText = changed ? "Original: " + valueText(tag, original) : "";
        }

        private void showParamHelp()
        {
            if (txtParamHelp == null)
                return;

            ParamGridTag tag = dgvParamFields.CurrentRow?.Tag as ParamGridTag;
            if (tag == null || currentParam?.Def == null)
            {
                string description = currentParam?.Def == null ? null : ParamHelp.For(currentParam.Def).Description;
                txtParamHelp.Text = string.IsNullOrWhiteSpace(description)
                    ? "Pick a param and a row, then click a value to change it. Select a field to see what it does here."
                    : currentParam.DisplayName + "\r\n" + description.Replace("\n", "\r\n");
                return;
            }

            ParamField field = tag.Field;
            List<string> lines = new List<string>();
            lines.Add(tag.DisplayName);
            string details = field.Name + ", " + describeType(field);
            lines.Add("(" + details + ")");
            if (!string.IsNullOrWhiteSpace(tag.Help?.Description))
                lines.Add(tag.Help.Description);
            else
                lines.Add("No description available for this field.");
            if (tag.Help?.IsObsolete ?? false)
                lines.Add("This field isn't used by the game.");
            if (!string.IsNullOrEmpty(tag.Help?.Refs))
                lines.Add("This is the ID of a row in another param; the 'Points to' column shows which one.");
            string original = dgvParamFields.CurrentRow.Cells[1].ToolTipText;
            if (!string.IsNullOrEmpty(original))
                lines.Add(original);
            txtParamHelp.Text = string.Join("\r\n", lines).Replace("\r\n", "\n").Replace("\n", "\r\n");
        }

        private static string describeType(ParamField field)
        {
            if (field.IsBitfield)
                return field.BitSize == 1 ? "on/off flag" : $"whole number 0 to {(1L << field.BitSize) - 1}";
            switch (field.Type)
            {
                case "s8": return "whole number -128 to 127";
                case "u8": return "whole number 0 to 255";
                case "s16": return "whole number -32768 to 32767";
                case "u16": return "whole number 0 to 65535";
                case "s32": return "whole number";
                case "u32": return "whole number, 0 or more";
                case "f32": return "decimal number";
                case "fixstr":
                case "fixstrW": return "text (view only)";
                default: return "unused bytes";
            }
        }

        private void dgvParamFields_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (paramGridFilling || e.RowIndex < 0 || e.ColumnIndex != 1)
                return;
            DataGridViewRow gridRow = dgvParamFields.Rows[e.RowIndex];
            ParamGridTag tag = gridRow.Tag as ParamGridTag;
            if (tag == null || !checkParamUsable())
                return;
            ParamField field = tag.Field;

            // Turn what's in the cell back into a plain number
            string shown = Convert.ToString(gridRow.Cells[1].Value, CultureInfo.InvariantCulture) ?? "";
            string typed = shown;
            if (tag.Choices != null && tag.Choices.TryGetValue(shown, out long chosen))
                typed = chosen.ToString(CultureInfo.InvariantCulture);

            // Work on fresh bytes from the game, so nothing else in the row is overwritten with stale data
            byte[] bytes = Hook.ReadParamRow(currentParam, currentRow);
            if (typed.Trim() == field.Read(bytes))
                return;

            byte[] before = (byte[])bytes.Clone();
            string error = field.Write(bytes, typed);
            if (error != null)
            {
                paramGridFilling = true;
                gridRow.Cells[1].Value = valueText(tag, before);
                paramGridFilling = false;
                setParamStatus(tag.DisplayName + ": " + error);
                MessageBox.Show(error, tag.DisplayName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            rememberOriginal(currentParam, currentRow, before);
            if (!Hook.WriteParamRow(currentParam, currentRow, bytes))
            {
                setParamStatus("Couldn't write to the game's memory.");
                return;
            }

            byte[] after = Hook.ReadParamRow(currentParam, currentRow);
            paramGridFilling = true;
            gridRow.Cells[1].Value = valueText(tag, after);
            markParamCell(gridRow, tag, after, getOriginal(currentParam, currentRow));
            // A change can affect what other fields point to (some references depend on another field)
            foreach (DataGridViewRow other in dgvParamFields.Rows)
                if (other.Tag is ParamGridTag otherTag)
                    other.Cells[2].Value = describeReference(otherTag.Help, otherTag.Field, after);
            paramGridFilling = false;
            setParamStatus($"Set {tag.DisplayName} to {valueText(tag, after)}.");
            updateParamButtons();
            showParamHelp();
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
