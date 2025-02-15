﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NaviDoctor.extensions;
using NaviDoctor.helpers;
using NaviDoctor.models;

namespace NaviDoctor
{
    public partial class MainForm : Form
    {
        private SaveDataObject saveData;
        private List<Style> _styles;
        private SaveParse _openFile;
        private int loadSpinCount = 0;
        public MainForm()
        {
            InitializeComponent();
        }
        private class LibraryWindow
        {
            private Form form;
            private FlowLayoutPanel flowLayoutPanel;
            private Button checkAllButton;
            private Button confirmButton;

            public LibraryWindow()
            {
                form = new Form();
                flowLayoutPanel = new FlowLayoutPanel();

                form.Text = "Library Viewer";

                form.Width = 860;
                form.Height = 500;
                form.FormBorderStyle = FormBorderStyle.FixedSingle; // Disable resizing
                form.MaximizeBox = false; // Disable maximize button
                form.Icon = Properties.Resources.icon;

                flowLayoutPanel.Dock = DockStyle.Top;
                flowLayoutPanel.Height = (int)(form.Height * 0.8);
                flowLayoutPanel.AutoScroll = true;

                form.Controls.Add(flowLayoutPanel);
            }

            public void GenerateCheckAllButton()
            {
                checkAllButton = new Button();
                checkAllButton.Text = "Check All";
                checkAllButton.Anchor = AnchorStyles.None;
                checkAllButton.Top = flowLayoutPanel.Bottom + 10;
                checkAllButton.Left = ((form.Width - checkAllButton.Width) / 2) - 50;
                checkAllButton.Click += CheckAllButton_Click;

                form.Controls.Add(checkAllButton);
            }
            private void CheckAllButton_Click(object sender, EventArgs e)
            {
                foreach (Control control in flowLayoutPanel.Controls)
                {
                    if (control is CheckBox checkBox)
                    {
                        checkBox.Checked = true;
                    }
                }
            }
            public void GenerateConfirmButton(SaveDataObject saveData, bool paLib = false)
            {
                confirmButton = new Button();
                confirmButton.Text = "Confirm";
                confirmButton.Anchor = AnchorStyles.None;
                confirmButton.Top = flowLayoutPanel.Bottom + 10;
                confirmButton.Left = ((form.Width - confirmButton.Width) / 2) + 50;
                confirmButton.Click += (sender, e) => ConfirmButton_Click(sender, e, saveData, paLib);

                form.Controls.Add(confirmButton);
            }
            private void ConfirmButton_Click(object sender, EventArgs e, SaveDataObject saveData, bool paLib = false)
            {
                UpdateLibraryData(saveData, paLib);
                form.Close();
            }
            private void UpdateLibraryData(SaveDataObject saveData, bool paLib = false)
            {
                List<BattleChipData> chipNameMap;
                List<byte> libraryData;
                int libraryIndex;
                int paIndexStart;
                switch (saveData.GameName)
                {
                    case GameTitle.Title.MegaManBattleNetwork:
                        chipNameMap = BattleChipData.BN1ChipNameMap;
                        libraryData = saveData.LibraryData;
                        paIndexStart = 0; // BN1 doesn't have a PA library. Just set this to 0.
                        break;
                    case GameTitle.Title.MegaManBattleNetwork2:
                        chipNameMap = BattleChipData.BN2ChipNameMap;
                        paIndexStart = 0x110;
                        if (paLib)
                        {
                            libraryData = saveData.PALibraryData;
                        }
                        else
                        {
                            libraryData = saveData.LibraryData;
                        }
                        break;
                    case GameTitle.Title.MegaManBattleNetwork3Blue:
                    case GameTitle.Title.MegaManBattleNetwork3White:
                        chipNameMap = BattleChipData.BN3ChipNameMap;
                        paIndexStart = 0x140;
                        if (paLib)
                        {
                            libraryData = saveData.PALibraryData;
                        }
                        else
                        {
                            libraryData = saveData.LibraryData;
                        }
                        break;
                    default:
                        return;
                }
                foreach (Control control in flowLayoutPanel.Controls)
                {
                    if (control is CheckBox checkBox)
                    {
                        string chipName = checkBox.Text;
                        int chipIndex = BattleChipData.GetChipIDByName(chipNameMap, chipName);

                        if (chipIndex != -1)
                        {
                            if (!paLib)
                            {
                                libraryIndex = chipIndex / 8;
                            }
                            else
                            {
                                libraryIndex = (chipIndex - paIndexStart) / 8;
                            }
                            int bitIndex = 7 - (chipIndex % 8);

                            if (checkBox.Checked)
                                libraryData[libraryIndex] |= (byte)(1 << bitIndex);
                            else
                                libraryData[libraryIndex] &= (byte)~(1 << bitIndex);
                        }
                    }
                }
            }
            public void AddChip(string chipName, bool isChecked)
            {
                CheckBox checkBox = new CheckBox();
                checkBox.Text = chipName;
                checkBox.Checked = isChecked;
                flowLayoutPanel.Controls.Add(checkBox);
            }
            public void ShowDialog()
            {
                form.ShowDialog();
            }
        }

        private void ShowLoading()
        {
            pbx_Loading.Visible = true;
            lbl_Loading.Visible = true;
            loadSpinCount++;
        }

        private void HideLoading()
        {
            loadSpinCount--;
            if (loadSpinCount == 0)
            {
                pbx_Loading.Visible = false;
                lbl_Loading.Visible = false;
            }
        }

        private void GenerateLibraryWindow(SaveDataObject saveData, bool paLib = false)
        {
            LibraryWindow libraryWindow = new LibraryWindow();

            int standardStartID = 1;
            int standardStopID;
            bool isChecked;
            int libraryIndex;
            int bitIndex;
            List<BattleChipData> chipNameMap;

            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork:
                    standardStopID = 199;
                    chipNameMap = BattleChipData.BN1ChipNameMap;
                    break;
                case GameTitle.Title.MegaManBattleNetwork2:
                    standardStopID = 0x10F;
                    chipNameMap = BattleChipData.BN2ChipNameMap;
                    if (paLib)
                    {
                        standardStartID = 0x110;
                        standardStopID = 0x12F;
                    }
                    break;
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                case GameTitle.Title.MegaManBattleNetwork3White:
                    standardStopID = 0x138;
                    chipNameMap = BattleChipData.BN3ChipNameMap;
                    if (paLib)
                    {
                        standardStartID = 0x140;
                        standardStopID = 0x15F;
                    }
                    break;
                default:
                    return;
            }

            for (int i = standardStartID; i <= standardStopID; i++)
            {
                string chipName = BattleChipData.GetChipNameByID(chipNameMap, i).Name;

                if (chipName.Length < 3)
                    continue;
                if (!paLib)
                {
                    libraryIndex = (i) / 8;
                    bitIndex = 7 - ((i) % 8);
                    isChecked = (saveData.LibraryData[libraryIndex] & (1 << bitIndex)) != 0;
                } else
                {
                    libraryIndex = (i - standardStartID) / 8;
                    bitIndex = 7 - (i % 8);
                    isChecked = (saveData.PALibraryData[libraryIndex] & (1 << bitIndex)) != 0;
                }

                libraryWindow.AddChip(chipName, isChecked);
            }

            libraryWindow.GenerateCheckAllButton();
            libraryWindow.GenerateConfirmButton(saveData, paLib);
            libraryWindow.ShowDialog();
        }
        private string GetAlphabeticalCode(int chipCode)
        {
            if (chipCode == 26)
            {
                return "*";
            }

            char chipCodeLetter = (char)(chipCode + 0x41);
            return chipCodeLetter.ToString();
        }
        private void PopulateDataGridView(BattleChipData battleChipData, SaveDataObject saveData)
        {
            List<BattleChipData> entries = battleChipData.GenerateChipEntries(saveData);
            List<byte> packChips;

            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork:
                    packChips = saveData.BattleChips.Concat(saveData.NaviChips).ToList();
                    break;
                case GameTitle.Title.MegaManBattleNetwork2:
                    packChips = saveData.BattleChips.Concat(saveData.NaviChips).Concat(saveData.SecretChips).ToList();
                    break;
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                case GameTitle.Title.MegaManBattleNetwork3White:
                    packChips = saveData.BattleChips.Concat(saveData.NaviChips).ToList();
                    break;
                default:
                    return;
            }

            for (int i = 0; i < packChips.Count; i++)
            {
                entries[i].Quantity = packChips[i];
                if (entries[i].Code == "None") entries[i].Quantity = 0; // Automatically set illegal chips to 0.
            }

            DataView dataView = new DataView();
            dataView.Table = new DataTable("Pack");
            dataView.Table.Columns.Add("ID", typeof(int));
            dataView.Table.Columns.Add("Name", typeof(string));
            dataView.Table.Columns.Add("Code", typeof(string));
            dataView.Table.Columns.Add("Quantity", typeof(int));

            if(saveData.GameName != GameTitle.Title.MegaManBattleNetwork)
            {
                dataView.Table.Columns.Add("MB", typeof(int)); 
                foreach (var entry in entries)
                {
                    switch (saveData.GameName)
                    {
                        case GameTitle.Title.MegaManBattleNetwork2:
                            dataView.Table.Rows.Add(entry.ID, entry.Name, entry.Code, entry.Quantity, entry.Size);
                            break;
                        case GameTitle.Title.MegaManBattleNetwork3White:
                            if (entry.Type != 4) dataView.Table.Rows.Add(entry.ID, entry.Name, entry.Code, entry.Quantity, entry.Size);
                            break;
                        case GameTitle.Title.MegaManBattleNetwork3Blue:
                            if (entry.Type != 3) dataView.Table.Rows.Add(entry.ID, entry.Name, entry.Code, entry.Quantity, entry.Size);
                            break;
                    }
                }
            }
            else
            {
                foreach (var entry in entries)
                {
                    dataView.Table.Rows.Add(entry.ID, entry.Name, entry.Code, entry.Quantity);
                }
            }

            dataView.RowFilter = "Code <> 'None' AND Code <> '+'";

            dgvPack.DataSource = dataView;

            dgvPack.Columns["ID"].Visible = false;
            dgvPack.Columns["Name"].ReadOnly = true;
            dgvPack.Columns["Name"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvPack.Columns["Code"].ReadOnly = true;
            dgvPack.Columns["Quantity"].ReadOnly = false;
            if (saveData.GameName != GameTitle.Title.MegaManBattleNetwork)
            {
                dgvPack.Columns["MB"].ReadOnly = true;
            }

            dgvPack.RowHeadersWidth = 30;
        }
        private void LoadFolderData(SaveDataObject saveData)
        {
            switch(saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork:
                    {
                        PopulateFolderDataGridView(dgvFolder1, saveData.FolderData);
                        break;
                    }
                case GameTitle.Title.MegaManBattleNetwork2:
                    {
                        for (var i = 1; i <= saveData.Folders; i++)
                        {
                            switch (i)
                            {
                                case 1:
                                    {
                                        PopulateFolderDataGridView(dgvFolder1, saveData.FolderData);
                                        break;
                                    }
                                case 2:
                                    {
                                        PopulateFolderDataGridView(dgvFolder2, saveData.Folder2Data);
                                        break;
                                    }
                                case 3:
                                    {
                                        PopulateFolderDataGridView(dgvFolder3, saveData.Folder3Data);
                                        break;
                                    }
                                default:
                                    break;
                            }
                        }
                        PopulateRegChipCombobox();
                        break;
                    }
                case GameTitle.Title.MegaManBattleNetwork3White:
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                    {
                        for (var i = 1; i <= saveData.Folders; i++) // Just a copy/paste of BN2 for now.
                        {
                            switch (i)
                            {
                                case 1:
                                    {
                                        PopulateFolderDataGridView(dgvFolder1, saveData.FolderData);
                                        break;
                                    }
                                case 2:
                                    {
                                        PopulateFolderDataGridView(dgvFolder2, saveData.Folder3Data); 
                                        break;
                                    }
                                case 3:
                                    {
                                        PopulateFolderDataGridView(dgvFolder3, saveData.Folder2Data); // This is Xtra Folder
                                        break;
                                    }
                                default:
                                    break;
                            }
                        }
                        PopulateRegChipCombobox();
                        break;
                    }
            }
        }
        
        private void PopulateFolderDataGridView(DataGridView dgvFolder, List<Tuple<int, int>> folderSaveData)
        {
            DataTable folderDataTable = new DataTable();
            folderDataTable.Columns.Add("ID", typeof(int));
            folderDataTable.Columns.Add("Name", typeof(string));
            folderDataTable.Columns.Add("Code", typeof(string));
            if(saveData.GameName != GameTitle.Title.MegaManBattleNetwork)
            {
                folderDataTable.Columns.Add("MB", typeof(int));
            }

            List<BattleChipData> chipNameMap;
            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork:
                    chipNameMap = BattleChipData.BN1ChipNameMap;
                    break;
                case GameTitle.Title.MegaManBattleNetwork2:
                    chipNameMap = BattleChipData.BN2ChipNameMap;
                    break;
                case GameTitle.Title.MegaManBattleNetwork3White:
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                    chipNameMap = BattleChipData.BN3ChipNameMap;
                    break;
                default:
                    return;
            }

            foreach (var folderData in folderSaveData)
            {
                int chipID = folderData.Item1;
                int chipCode = folderData.Item2;

                string chipName = BattleChipData.GetChipNameByID(chipNameMap, chipID).Name;
                string chipCodeLetter = GetAlphabeticalCode(chipCode);
                if (saveData.GameName != GameTitle.Title.MegaManBattleNetwork)
                {
                    int size = BattleChipData.GetChipNameByID(chipNameMap, chipID).Size;

                    folderDataTable.Rows.Add(chipID, chipName, chipCodeLetter, size);
                }
                else
                {
                    folderDataTable.Rows.Add(chipID, chipName, chipCodeLetter);
                }
            }
            // Preserve the current selection and scroll position
            int selectedRowIndex = -1;
            int firstDisplayedScrollingRowIndex = -1;

            if (dgvFolder.SelectedRows.Count > 0)
                selectedRowIndex = dgvFolder.SelectedRows[0].Index;

            if (dgvFolder.FirstDisplayedScrollingRowIndex >= 0)
                firstDisplayedScrollingRowIndex = dgvFolder.FirstDisplayedScrollingRowIndex;

            dgvFolder.DataSource = null;
            dgvFolder.Rows.Clear();

            dgvFolder.DataSource = folderDataTable;
            dgvFolder.Columns["ID"].Visible = false;

            // Restore the previous selection and scroll position
            if (selectedRowIndex >= 0 && selectedRowIndex < dgvFolder.Rows.Count)
                dgvFolder.Rows[selectedRowIndex].Selected = true;

            if (firstDisplayedScrollingRowIndex >= 0 && firstDisplayedScrollingRowIndex < dgvFolder.Rows.Count)
                dgvFolder.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex;

            dgvFolder.Columns["Name"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvFolder.RowHeadersWidth = 30;

        }

        private void PopulateRegChipCombobox()
        {
            panelRegChip.Enabled = true;
            var chipData = new List<BattleChipData>();
            List<Tuple<int, int>> folderSaveData = null;
            List<BattleChipData> chipNameMap;

            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork2:
                    chipNameMap = BattleChipData.BN2ChipNameMap;
                    break;
                case GameTitle.Title.MegaManBattleNetwork3White:
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                    chipNameMap = BattleChipData.BN3ChipNameMap;
                    break;
                default:
                    return;
            }

            //This is because of the way Extra Folder is saved into Folder2Data
            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork2:
                    switch (tabsFolders.SelectedIndex)
                    {
                        case 0:
                            folderSaveData = saveData.FolderData;
                            break;
                        case 1:
                            folderSaveData = saveData.Folder2Data;
                            break;
                        case 2:
                            folderSaveData = saveData.Folder3Data;
                            break;
                        default:
                            break;
                    }
                    break;
                case GameTitle.Title.MegaManBattleNetwork3White:
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                    switch (tabsFolders.SelectedIndex)
                    {
                        case 0:
                            folderSaveData = saveData.FolderData;
                            break;
                        case 1:
                            folderSaveData = saveData.Folder3Data;
                            break;
                        case 2:
                            cbxRegChip.DataSource = null;
                            panelRegChip.Enabled = false;
                            return; //no reg chip for extra folder
                        default:
                            break;
                    }
                    break;
                default:
                    return;
            }

            foreach (var folderData in folderSaveData)
            {
                int chipID = folderData.Item1;
                int chipCode = folderData.Item2;

                string chipCodeLetter = GetAlphabeticalCode(chipCode);

                var bcd = BattleChipData.GetChipNameByID(chipNameMap, chipID);
                bcd.AlphabeticalCode = chipCodeLetter;
                chipData.Add(bcd);
            }


            cbxRegChip.DataSource = chipData;
            cbxRegChip.DisplayMember = "RegChipDisplayMember";
            cbxRegChip.ValueMember = "ID";
            var currentRegChipIndex = 0;

            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork2:
                    switch (tabsFolders.SelectedIndex)
                    {
                        case 0:
                            currentRegChipIndex = saveData.RegChip1;
                            break;
                        case 1:
                            currentRegChipIndex = saveData.RegChip2;
                            break;
                        case 2:
                            currentRegChipIndex = saveData.RegChip3;
                            break;
                        default:
                            break;
                    }
                    break;
                case GameTitle.Title.MegaManBattleNetwork3White:
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                    switch (tabsFolders.SelectedIndex)
                    {
                        case 0:
                            currentRegChipIndex = saveData.RegChip1;
                            break;
                        case 1:
                            currentRegChipIndex = saveData.RegChip2;
                            break;
                        case 2:
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    return;
            }

            if (currentRegChipIndex != -1 && currentRegChipIndex != 255)
            {
                cbxRegChip.SelectedIndex = currentRegChipIndex;
            }
        }

        private void btnRemoveChip_Click(object sender, EventArgs e)
        {
            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork:
                    {
                        RemoveChip(dgvFolder1, saveData.FolderData);
                        break;
                    }
                case GameTitle.Title.MegaManBattleNetwork2:
                    {
                        switch (tabsFolders.SelectedIndex)
                        {
                            case 0:
                                {
                                    RemoveChip(dgvFolder1, saveData.FolderData);
                                    break;
                                }
                            case 1:
                                {
                                    RemoveChip(dgvFolder2, saveData.Folder2Data);
                                    break;
                                }
                            case 2:
                                {
                                    RemoveChip(dgvFolder3, saveData.Folder3Data);
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                case GameTitle.Title.MegaManBattleNetwork3White:
                    {
                        switch (tabsFolders.SelectedIndex)
                        {
                            case 0:
                                {
                                    RemoveChip(dgvFolder1, saveData.FolderData);
                                    break;
                                }
                            case 1:
                                {
                                    RemoveChip(dgvFolder2, saveData.Folder3Data); 
                                    break;
                                }
                            case 2:
                                {
                                    MessageBox.Show("Unable to edit the Xtra Folder");
                                    // RemoveChip(dgvFolder3, saveData.Folder2Data); // Can't edit Xtra Folder
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
            }
           
        }

        private void RemoveChip(DataGridView dgvFolder, List<Tuple<int, int>> folderData)
        {
            // Check if there is a selected row in the Folder DataGridView
            if (dgvFolder.SelectedRows.Count > 0)
            {
                // Get the selected chip ID
                int chipID = (int)dgvFolder.SelectedRows[0].Cells["ID"].Value;
                string chipCode = (string)dgvFolder.SelectedRows[0].Cells["Code"].Value;
                int chipInt = 26; // Assuming * code unless told otherwise.
                if (chipCode != "*") chipInt = chipCode[0] - 'A';

                // Find the first occurrence of the chip in the FolderData list
                var chipToRemove = folderData.FirstOrDefault(data => data.Item1 == chipID && data.Item2 == chipInt);

                // Remove the chip if found
                if (chipToRemove != null)
                {
                    folderData.Remove(chipToRemove);
                }
                else
                {
                    MessageBox.Show("The selected chip was not found in the Folder.");
                }

                // Refresh the Folder DataGridView
                LoadFolderData(saveData);
            }
            else
            {
                MessageBox.Show("Please select a chip in the Folder view.");
            }

            UpdateFolderCount();
        }

        // check the quantity of a chip ID and code in the Folder
        private int GetChipQuantityInFolder(int chipID, string chipCode)
        {
            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork:
                    if (chipCode != "*") return saveData.FolderData.Count(data => data.Item1 == chipID && data.Item2 == (byte)(chipCode[0] - 'A'));
                    return saveData.FolderData.Count(data => data.Item1 == chipID && data.Item2 == 26);
                case GameTitle.Title.MegaManBattleNetwork2:
                    if (chipCode != "*") return saveData.FolderData.Count(data => data.Item1 == chipID && data.Item2 == (byte)(chipCode[0] - 'A')) + saveData.Folder2Data.Count(data => data.Item1 == chipID && data.Item2 == (byte)(chipCode[0] - 'A')) + saveData.Folder3Data.Count(data => data.Item1 == chipID && data.Item2 == (byte)(chipCode[0] - 'A'));
                    return saveData.FolderData.Count(data => data.Item1 == chipID && data.Item2 == 26) + saveData.Folder2Data.Count(data => data.Item1 == chipID && data.Item2 == 26) + saveData.Folder3Data.Count(data => data.Item1 == chipID && data.Item2 == 26);
                case GameTitle.Title.MegaManBattleNetwork3White:
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                    if (chipCode != "*") return saveData.FolderData.Count(data => data.Item1 == chipID && data.Item2 == (byte)(chipCode[0] - 'A')) + saveData.Folder3Data.Count(data => data.Item1 == chipID && data.Item2 == (byte)(chipCode[0] - 'A'));
                    return saveData.FolderData.Count(data => data.Item1 == chipID && data.Item2 == 26) + saveData.Folder3Data.Count(data => data.Item1 == chipID && data.Item2 == 26);
                default:
                    return 0;
            }
        }

        private void btnAddChip_Click(object sender, EventArgs e)
        {
            List<Tuple<int, int>> folderData;
            int battleIDHigh = 0;
            int naviIDLow = 0;
            int naviIDHigh = 0;
            int secretIDLow = 0;
            int secretIDHigh = 0;
            int maxBattleChipCopies = 0;
            int maxNaviChips = 0;
            int maxTotalChipsInFolder = 30;
            bool isBattleChip = false;
            bool isNaviChip = false;
            bool isMegaChip = false;
            bool isGigaChip = false;
            int megaLimit = 0;
            int gigaLimit = 0;
            List<BattleChipData> chipNameMap;
            switch (tabsFolders.SelectedIndex) // Check which folder is currently selected
            {
                case 1:
                    folderData = saveData.Folder2Data;
                    if (saveData.GameName > GameTitle.Title.MegaManBattleNetwork3White) folderData = saveData.Folder3Data;
                    break;
                case 2:
                    folderData = saveData.Folder3Data;
                    if (saveData.GameName > GameTitle.Title.MegaManBattleNetwork3White) folderData = saveData.Folder2Data;
                    break;
                default:
                    folderData = saveData.FolderData;
                    break;
            }

            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork:
                    battleIDHigh = 147;
                    naviIDLow = 148;
                    naviIDHigh = 239;
                    maxBattleChipCopies = 10;
                    maxNaviChips = 5;
                    chipNameMap = BattleChipData.BN1ChipNameMap;
                    break;
                case GameTitle.Title.MegaManBattleNetwork2: // Not actual BN2 values.
                    battleIDHigh = 193;
                    naviIDLow = 194;
                    naviIDHigh = 250;
                    secretIDLow = 251;
                    secretIDHigh = 271;
                    maxBattleChipCopies = 5;
                    maxNaviChips = 5;
                    chipNameMap = BattleChipData.BN2ChipNameMap;
                    break;
                case GameTitle.Title.MegaManBattleNetwork3White:
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                    battleIDHigh = 0xC8;
                    naviIDLow = 0xC9;
                    naviIDHigh = 0x138;
                    maxBattleChipCopies = 4;
                    megaLimit = saveData.MegaLimit;
                    gigaLimit = saveData.GigaLimit;
                    maxNaviChips = 5;
                    chipNameMap = BattleChipData.BN3ChipNameMap;
                    break;
                default:
                    return;
            }

            // Check if there is a selected row in the Pack DataGridView
            if (dgvPack.SelectedRows.Count > 0)
            {
                // Get the selected chip ID and code from the Pack
                int chipID = (int)dgvPack.SelectedRows[0].Cells["ID"].Value;
                string chipCode = dgvPack.SelectedRows[0].Cells["Code"].Value.ToString();

                // Check if the Folder has reached the maximum number of copies for the selected chip ID
                int currentChipCopies = folderData.Count(data => data.Item1 == chipID);
                int currentNaviChips = 0;
                int currentGigaChips = 0;

                switch (saveData.GameName)
                {
                    case GameTitle.Title.MegaManBattleNetwork:
                        currentNaviChips = folderData.Count(data => data.Item1 >= naviIDLow && data.Item1 <= naviIDHigh);
                        isBattleChip = chipID >= 1 && chipID <= battleIDHigh;
                        isNaviChip = chipID >= naviIDLow && chipID <= naviIDHigh;
                        break;
                    case GameTitle.Title.MegaManBattleNetwork2:
                        currentNaviChips = folderData.Count(data => (data.Item1 >= naviIDLow && data.Item1 <= naviIDHigh) || (data.Item1 >= 261 && data.Item1 <= 265));
                        isBattleChip = (chipID >= 1 && chipID <= battleIDHigh) || (chipID >= secretIDLow && chipID <= 260) || (chipID >= 266 && chipID <= secretIDHigh); // Secret NetBattle reward chips
                        isNaviChip = (chipID >= naviIDLow && chipID <= naviIDHigh) || (chipID >= 261 && chipID <= 265); // Event Navis
                        break;
                    case GameTitle.Title.MegaManBattleNetwork3Blue:
                    case GameTitle.Title.MegaManBattleNetwork3White:
                        currentNaviChips = folderData.Count(data => data.Item1 >= naviIDLow && data.Item1 <= naviIDHigh && chipNameMap.FirstOrDefault(chip => chip.ID == data.Item1).Type == 1); // current count of mega chips
                        currentGigaChips = folderData.Count(data => data.Item1 >= naviIDLow && data.Item1 <= naviIDHigh && chipNameMap.FirstOrDefault(chip => chip.ID == data.Item1).Type > 1); // current count of giga chips
                        isBattleChip = chipNameMap.FirstOrDefault(data => data.ID == chipID).Type == 0;
                        isNaviChip = chipNameMap.FirstOrDefault(data => data.ID == chipID).Type != 0;
                        isMegaChip = chipNameMap.FirstOrDefault(data => data.ID == chipID).Type == 1;
                        isGigaChip = chipNameMap.FirstOrDefault(data => data.ID == chipID).Type >= 2;
                        break;
                    default:
                        return;
                }

                string chipName = BattleChipData.GetChipNameByID(chipNameMap, chipID).Name;

                if (isBattleChip && currentChipCopies >= maxBattleChipCopies)
                {
                    MessageBox.Show($"The limit for {chipName} cannot exceed {maxBattleChipCopies}.");
                }
                else if (isNaviChip && currentNaviChips >= maxNaviChips && saveData.GameName <= GameTitle.Title.MegaManBattleNetwork2)
                {
                    MessageBox.Show($"The limit for Navi Chips cannot exceed {maxNaviChips}.");
                }
                else if (isNaviChip && saveData.GameName >= GameTitle.Title.MegaManBattleNetwork3White && isMegaChip && currentChipCopies < 1 && currentNaviChips >= megaLimit)
                {
                    MessageBox.Show($"The limit for Mega Chips cannot exceed {megaLimit}.");
                }
                else if (isNaviChip && saveData.GameName >= GameTitle.Title.MegaManBattleNetwork3White && isMegaChip && currentChipCopies >= 1)
                {
                    MessageBox.Show($"You may only have one copy of {chipName} in this folder.");
                }
                else if (isNaviChip && saveData.GameName >= GameTitle.Title.MegaManBattleNetwork3White && isGigaChip && currentChipCopies < 1 && currentGigaChips >= gigaLimit)
                {
                    MessageBox.Show($"The limit for Giga Chips cannot exceed {gigaLimit}.");
                }
                else if (isNaviChip && saveData.GameName >= GameTitle.Title.MegaManBattleNetwork3White && isGigaChip && currentChipCopies >= 1)
                {
                    MessageBox.Show($"You may only have one copy of {chipName} in this folder.");
                }
                else if (folderData.Count >= maxTotalChipsInFolder)
                {
                    MessageBox.Show($"The number of chips in the folder cannot exceed {maxTotalChipsInFolder}.");
                }
                else
                {
                    // Check the quantity of the chip ID and code in the Folder
                    int folderChipQuantity = GetChipQuantityInFolder(chipID, chipCode);

                    // Increment the quantity in the Pack if the quantity in the Folder is less than or equal to the quantity in the Pack
                    int packChipQuantity = (int)dgvPack.SelectedRows[0].Cells["Quantity"].Value;
                    if (folderChipQuantity >= packChipQuantity)
                    {
                        dgvPack.SelectedRows[0].Cells["Quantity"].Value = folderChipQuantity + 1;
                    }

                    // Add the chip to the FolderData list with the selected chip code
                    if (chipCode != "*") folderData.Add(new Tuple<int, int>(chipID, chipCode[0] - 'A'));
                    else folderData.Add(new Tuple<int, int>(chipID, 26));

                    // Refresh the Folder DataGridView
                    LoadFolderData(saveData);
                }
            }
            else
            {
                MessageBox.Show("Please select a chip in the Pack view.");
            }
            UpdateFolderCount();
        }

        private void PackageChips()
        {
            dgvPack.Sort(dgvPack.Columns[0], ListSortDirection.Ascending); // Sort by ID column in ascending order
            Dictionary<string, List<string>> chipCodeMap;

            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork:
                    chipCodeMap = BattleChipData.BN1ChipCodeMap;
                    saveData.BattleChips = GeneratePackage(1, 147, 5, chipCodeMap);
                    saveData.NaviChips = GeneratePackage(148, 239, 1, chipCodeMap);
                    break;

                case GameTitle.Title.MegaManBattleNetwork2:
                    chipCodeMap = BattleChipData.BN2ChipCodeMap;
                    saveData.BattleChips = GeneratePackage(1, 193, 6, chipCodeMap);
                    saveData.NaviChips = GeneratePackage(194, 250, 2, chipCodeMap);
                    saveData.SecretChips = GeneratePackage(251, 271, 6, chipCodeMap);
                    break;
                case GameTitle.Title.MegaManBattleNetwork3White:
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                    chipCodeMap = BattleChipData.BN3ChipCodeMap;
                    saveData.BattleChips = GeneratePackage(1, 0xC8, 6, chipCodeMap);
                    saveData.NaviChips = GeneratePackage(0xC9, 0x138, 2, chipCodeMap);
                    break;
            }
        }

        private List<byte> GeneratePackage(int startID, int stopID, int codeCount, Dictionary<string, List<string>> chipCodeMap)
        {
            List<byte> chipPackage = new List<byte>(Enumerable.Repeat((byte)0, (stopID - startID + 1) * codeCount));

            foreach (DataGridViewRow row in dgvPack.Rows)
            {
                if (row.Cells["ID"].Value != null && row.Cells["Quantity"].Value != null)
                {
                    int chipID = (int)row.Cells["ID"].Value;
                    int quantity = (int)row.Cells["Quantity"].Value;
                    
                    quantity = Math.Max(0, Math.Min(quantity, 99));

                    if (chipID >= startID && chipID <= stopID)
                    {
                        int codeSeries = chipCodeMap[row.Cells["Name"].Value.ToString()].IndexOf(row.Cells["Code"].Value.ToString());
                        int index = ((chipID - startID) * codeCount) + codeSeries;
                        chipPackage[index] = (byte)quantity;
                    }
                }
            }
            return chipPackage;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Title = "Open MMBN Save File...";
            openFile.InitialDirectory = @"C:\Program Files (x86)\Steam\userdata";
            openFile.Filter = "Legacy Collection Save File|*save_0.bin|MMBN1 Save File|exe1_save_0.bin|MMBN2 Save File|exe2j_save_0.bin|MMBN3 Save File|exe3?_save_0.bin|All Files|*.*";
            openFile.RestoreDirectory = true;
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _openFile = new SaveParse(openFile.FileName);
                    LoadFile(_openFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex}");
                }
            }
        }

        private void LoadFile(SaveParse saveParse)
        {
            saveData = saveParse.ExtractSaveData();

            DisplayModulesBasedOnGame();
            BattleChipData battleChipData = new BattleChipData();
            PopulateDataGridView(battleChipData, saveData);

            LoadFolderData(saveData);
            saveData.BonusHP = 0;
            maxHPStat.Value = (saveData.HPUp * 20) + 100; // Calculate base HP based on how many HP ups were obtained.
            if (saveData.GameName == GameTitle.Title.MegaManBattleNetwork || saveData.GameName == GameTitle.Title.MegaManBattleNetwork2)
            {
                attackStat.Value = saveData.AttackPower + 1;
                rapidStat.Value = saveData.RapidPower + 1;
                chargeStat.Value = saveData.ChargePower + 1;
            }
            zennyBox.Value = saveData.Zenny;
            steamID.Value = saveData.SteamID;

            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork:
                    saveData.LoadStyles(ref _styles);
                    nudBugFrag.Value = 0; // It bothered me seeing my BugFrags and RegMem from BN2 when I loaded BN1 over it.
                    nudRegMem.Value = 0;  // So they're 0 now.
                    break;

                case GameTitle.Title.MegaManBattleNetwork2:
                    saveData.LoadStyles(ref _styles);
                    nudBugFrag.Value = saveData.BugFrags;
                    nudRegMem.Value = saveData.RegMem;
                    nudSubChipMax.Value = saveData.SubChipMax;
                    nudMiniEnrg.Value = saveData.SubMiniEnrg;
                    nudFullEnrg.Value = saveData.SubFullEnrg;
                    nudLocEnemy.Value = saveData.SubLocEnemy;
                    nudSneakRun.Value = saveData.SubSneakRun;
                    nudUnlocker.Value = saveData.SubUnlocker;
                    nudUntrap.Value = saveData.SubUntrap;
                    break;
                case GameTitle.Title.MegaManBattleNetwork3White:
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                    saveData.BonusHP = (short)(saveData.MaxHP - maxHPStat.Value);
                    saveData.AttackPower += 1;
                    saveData.LoadStyles(ref _styles);
                        string equipstyle = _styles.FirstOrDefault(x => x.Equip == true).Name.ToString();
                        if (equipstyle.Contains("Guts"))
                        {
                            saveData.AttackPower = (byte)(saveData.AttackPower / 2);
                        }
                        else if (equipstyle.Contains("Team"))
                        {
                            saveData.MegaLimit -= 1;
                        }
                        else if (equipstyle.Contains("Cust"))
                        {
                            saveData.CustEffects[18] -= 1;
                        }
                    nudBugFrag.Value = saveData.BugFrags;
                    nudRegMem.Value = saveData.RegMem;
                    nudSubChipMax.Value = saveData.SubChipMax;
                    nudMiniEnrg.Value = saveData.SubMiniEnrg;
                    nudFullEnrg.Value = saveData.SubFullEnrg;
                    nudLocEnemy.Value = saveData.SubLocEnemy;
                    nudSneakRun.Value = saveData.SubSneakRun;
                    nudUnlocker.Value = saveData.SubUnlocker;
                    nudUntrap.Value = saveData.SubUntrap;
                    break;
            }

            //Show the current game loaded
            lblGameVersion.Text = $"Loaded: {saveData.GameName.GetString()}";
            UpdateFolderCount();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if a save file has been loaded
            if (saveData == null)
            {
                MessageBox.Show("Please load a save file first.");
                return; // Exit the event handler
            }

            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Title = "Save MMBN Save File...";
            saveFile.InitialDirectory = $@"C:\Program Files (x86)\Steam\userdata\{saveData.SteamID}\1798010\remote\";

            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork:
                    saveFile.FileName = "exe1_save_0.bin";
                    saveFile.Filter = "MMBN1 Save File|exe1_save_0.bin|All Files|*.*";
                    break;
                case GameTitle.Title.MegaManBattleNetwork2:
                    saveFile.FileName = "exe2j_save_0.bin";
                    saveFile.Filter = "MMBN2 Save File|exe2j_save_0.bin|All Files|*.*";
                    break;
                case GameTitle.Title.MegaManBattleNetwork3White:
                    saveFile.FileName = "exe3w_save_0.bin";
                    saveFile.Filter = "MMBN3W Save File|exe3w_save_0.bin|All Files|*.*";
                    break;
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                    saveFile.FileName = "exe3b_save_0.bin";
                    saveFile.Filter = "MMBN3B Save File|exe3b_save_0.bin|All Files|*.*";
                    break;

            }
            saveFile.RestoreDirectory = true;
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                try
                { 
                    if (_openFile.saveFilePath == saveFile.FileName && File.Exists(saveFile.FileName))
                    {
                        _openFile.saveFilePath = saveFile.FileName + ".backup";
                        _openFile.SaveChanges();
                    }
                    _openFile.saveFilePath = saveFile.FileName;
                    SaveFile(_openFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while saving the file:\n{ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        private void SaveFile(SaveParse saveParse)
        {
            saveData.HPUp = (byte)((maxHPStat.Value - 100) / 20);
            if (saveData.GameName == GameTitle.Title.MegaManBattleNetwork || saveData.GameName == GameTitle.Title.MegaManBattleNetwork2)
            {
                saveData.MaxHP = (short)maxHPStat.Value;
                saveData.CurrHP = (short)maxHPStat.Value;
                saveData.AttackPower = (byte)(attackStat.Value - 1);
                saveData.RapidPower = (byte)(rapidStat.Value - 1);
                saveData.ChargePower = (byte)(chargeStat.Value - 1);
            }
            if (saveData.GameName == GameTitle.Title.MegaManBattleNetwork2)
            {
                nudBugFrag.Value = Math.Min(nudBugFrag.Value, 32);
            }
            if (saveData.GameName == GameTitle.Title.MegaManBattleNetwork3White || saveData.GameName == GameTitle.Title.MegaManBattleNetwork3Blue)
            {
                saveData.MaxHP = (short)(maxHPStat.Value + saveData.BonusHP);
                saveData.CurrHP = saveData.MaxHP;
            }
            saveData.Zenny = (int)zennyBox.Value;
            saveData.SteamID = (int)steamID.Value;
            PackageChips();
            
            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork:
                    saveData.UpdateStyles(ref _styles);
                    break;
                case GameTitle.Title.MegaManBattleNetwork2:
                case GameTitle.Title.MegaManBattleNetwork3White:
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                    saveData.UpdateStyles(ref _styles);
                    if (saveData.GameName != GameTitle.Title.MegaManBattleNetwork2)
                    {
                        string equipstyle = _styles.FirstOrDefault(x => x.Equip == true).Name.ToString();
                        if (equipstyle.Contains("Guts"))
                        {
                            saveData.AttackPower = (byte)(saveData.AttackPower * 2);
                        }
                        else if (equipstyle.Contains("Team"))
                        {
                            saveData.MegaLimit += 1;
                        }
                        else if (equipstyle.Contains("Cust"))
                        {
                            saveData.CustEffects[18] += 1;
                        }
                    }
                    saveData.AttackPower -= 1;
                    saveData.BugFrags = (int)nudBugFrag.Value; 
                    saveData.RegMem = (byte)nudRegMem.Value;
                    saveData.SubChipMax = (byte)nudSubChipMax.Value;
                    saveData.SubMiniEnrg = (byte)nudMiniEnrg.Value;
                    saveData.SubFullEnrg = (byte)nudFullEnrg.Value;
                    saveData.SubLocEnemy = (byte)nudLocEnemy.Value;
                    saveData.SubSneakRun = (byte)nudSneakRun.Value;
                    saveData.SubUnlocker = (byte)nudUnlocker.Value;
                    saveData.SubUntrap = (byte)nudUntrap.Value;
                    break;
            }
            if (dgvFolder1.Rows.Count < 30 || (saveData.Folders >= 2 && dgvFolder2.Rows.Count < 30) || (saveData.Folders == 3 && dgvFolder3.Rows.Count < 30))
            {
                MessageBox.Show("An error occured while saving the file:\nFolder has less than 30 chips.");
                return;
            }
            saveParse.UpdateSaveData(saveData);
            saveParse.SaveChanges();
            MessageBox.Show("Save data successfully updated!");
        }

        private async void btnSetPackQuantity_Click(object sender, EventArgs e)
        {
            ShowLoading();
            dgvPack.Enabled = false;
            await Task.Run(() => UpdatePackQty());
            dgvPack.Enabled = true;
            HideLoading();
        }

        private void UpdatePackQty()
        {
            int packQuantity = (int)nudPackQuantity.Value;

            foreach (DataGridViewRow chip in dgvPack.Rows)
            {
                if ((int)chip.Cells[3].Value < packQuantity)
                {
                    chip.Cells[3].Value = packQuantity;
                }
            }
        }

        private void cbx_EditSteamID_CheckedChanged(object sender, EventArgs e)
        {
            steamID.Enabled = cbx_EditSteamID.Checked;
        }

        private void DisplayModulesBasedOnGame()
        {
            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork:
                    if (tabsFolders.TabPages.Contains(tabPage_Folder2)) tabsFolders.TabPages.Remove(tabPage_Folder2);
                    if (tabsFolders.TabPages.Contains(tabPage_Folder3)) tabsFolders.TabPages.Remove(tabPage_Folder3);
                    btnSelectStyles.Enabled = true;
                    panelBugFragRegMem.Visible = false;
                    panelSubChips.Visible = false;
                    programAdvanceMemoToolStripMenuItem.Enabled = false;
                    customizeToolStripMenuItem.Enabled = false;
                    panel_MegamanStats.Visible = true;
                    panelRegChip.Visible = false;
                    break;
                case GameTitle.Title.MegaManBattleNetwork2:
                    programAdvanceMemoToolStripMenuItem.Enabled = true;
                    customizeToolStripMenuItem.Enabled = false;
                    panelRegChip.Visible = true;
                    switch (saveData.Folders)
                    {
                        case 1:
                            if (tabsFolders.TabPages.Contains(tabPage_Folder2)) tabsFolders.TabPages.Remove(tabPage_Folder2);
                            if (tabsFolders.TabPages.Contains(tabPage_Folder3)) tabsFolders.TabPages.Remove(tabPage_Folder3);
                            break;
                        case 2:
                            if (!tabsFolders.TabPages.Contains(tabPage_Folder2)) tabsFolders.TabPages.Insert(1, tabPage_Folder2);
                            if (tabsFolders.TabPages.Contains(tabPage_Folder3)) tabsFolders.TabPages.Remove(tabPage_Folder3);
                            break;
                        case 3:
                            if (!tabsFolders.TabPages.Contains(tabPage_Folder2)) tabsFolders.TabPages.Insert(1, tabPage_Folder2);
                            if (!tabsFolders.TabPages.Contains(tabPage_Folder3)) tabsFolders.TabPages.Insert(2, tabPage_Folder3);
                            break;
                        default:
                            break;
                    }
                    btnSelectStyles.Enabled = true;
                    panelBugFragRegMem.Visible = true;
                    panelSubChips.Visible = true;
                    panel_MegamanStats.Visible = true;
                    break;
                case GameTitle.Title.MegaManBattleNetwork3White:
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                    programAdvanceMemoToolStripMenuItem.Enabled = true;
                    customizeToolStripMenuItem.Enabled = true;
                    panelRegChip.Visible = true;
                    switch (saveData.Folders)
                    {
                        case 1:
                            if (tabsFolders.TabPages.Contains(tabPage_Folder2)) tabsFolders.TabPages.Remove(tabPage_Folder2);
                            if (tabsFolders.TabPages.Contains(tabPage_Folder3)) tabsFolders.TabPages.Remove(tabPage_Folder3);
                            break;
                        case 2:
                            if (!tabsFolders.TabPages.Contains(tabPage_Folder2)) tabsFolders.TabPages.Insert(1, tabPage_Folder2);
                            if (tabsFolders.TabPages.Contains(tabPage_Folder3)) tabsFolders.TabPages.Remove(tabPage_Folder3);
                            break;
                        case 3:
                            if (!tabsFolders.TabPages.Contains(tabPage_Folder2)) tabsFolders.TabPages.Insert(1, tabPage_Folder2);
                            if (!tabsFolders.TabPages.Contains(tabPage_Folder3)) tabsFolders.TabPages.Insert(2, tabPage_Folder3);
                            break;
                        default:
                            break;
                    }
                    btnSelectStyles.Enabled = true;
                    panelBugFragRegMem.Visible = true;
                    panelSubChips.Visible = true;
                    panel_MegamanStats.Visible = false;
                    break;
                case GameTitle.Title.MegaManBattleNetwork4RedSun:
                case GameTitle.Title.MegaManBattleNetwork4BlueMoon:
                    break;
                case GameTitle.Title.MegaManBattleNetwork5TeamProtoman:
                case GameTitle.Title.MegaManBattleNetwork5TeamColonel:
                    break;
                case GameTitle.Title.MegaManBattleNetwork6CybeastGregar:
                case GameTitle.Title.MegaManBattleNetwork6CybeastFalzar:
                    break;
            }
        }

        private void btnSelectStyles_Click(object sender, EventArgs e)
        {
            var styleLoader = new StyleLoader(_styles, saveData.GameName);
            if (styleLoader.ShowDialog() == DialogResult.OK)
            {
                _styles = styleLoader._styles;
            }
        }

        private void tabsFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFolderCount();
            PopulateRegChipCombobox();
        }

        private void UpdateFolderCount()
        {
            switch (tabsFolders.SelectedIndex)
            {
                case 0:
                    lblFolderCount.Text = $"Folder1 Count: {dgvFolder1.RowCount}";
                    break;
                case 1:
                    lblFolderCount.Text = $"Folder2 Count: {dgvFolder2.RowCount}";
                    break;
                case 2:
                    lblFolderCount.Text = $"Folder3 Count: {dgvFolder3.RowCount}";
                    break;
                default:
                    lblFolderCount.Text = "";
                    break;
            }
        }

        private void nudMiniEnrg_ValueChanged(object sender, EventArgs e)
        {
            if(SubChipOverMax(nudMiniEnrg.Value))
            {
                nudMiniEnrg.Value--;
            }
        }

        private void nudFullEnrg_ValueChanged(object sender, EventArgs e)
        {
            if (SubChipOverMax(nudFullEnrg.Value))
            {
                nudFullEnrg.Value--;
            }
        }

        private void nudUntrap_ValueChanged(object sender, EventArgs e)
        {
            if (SubChipOverMax(nudUntrap.Value))
            {
                nudUntrap.Value--;
            }
        }

        private void nudSneakRun_ValueChanged(object sender, EventArgs e)
        {
            if (SubChipOverMax(nudSneakRun.Value))
            {
                nudSneakRun.Value--;
            }

        }

        private void nudLocEnemy_ValueChanged(object sender, EventArgs e)
        {
            if (SubChipOverMax(nudLocEnemy.Value))
            {
                nudLocEnemy.Value--;
            }

        }

        private void nudUnlocker_ValueChanged(object sender, EventArgs e)
        {
            if (SubChipOverMax(nudUnlocker.Value))
            {
                nudUnlocker.Value--;
            }

        }

        private bool SubChipOverMax(decimal value)
        {
            return value > nudSubChipMax.Value;
        }

        private void libraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if a save file has been loaded
            if (saveData == null)
            {
                MessageBox.Show("Please load a save file first.");
                return; // Exit the event handler
            }

            // Call the GenerateLibraryWindow method to display the library data
            GenerateLibraryWindow(saveData);
        }

        private void programAdvanceMemoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if a save file has been loaded
            if (saveData == null)
            {
                MessageBox.Show("Please load a save file first.");
                return; // Exit the event handler
            }

            // Call the GenerateLibraryWindow method to display the library data
            GenerateLibraryWindow(saveData, true);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var aboutPage = new AboutUs();
            aboutPage.ShowDialog();
        }

        private void customizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveData == null)
            {
                MessageBox.Show("Please load a save file first.");
                return; // Exit the event handler
            }
            Style style = _styles.FirstOrDefault(x => x.Equip == true);
            saveData.HPUp = (byte)((maxHPStat.Value - 100) / 20); // Recalculate HP like we're saving
            saveData.MaxHP = (short)(maxHPStat.Value + saveData.BonusHP);
            saveData.RegMem = (int)nudRegMem.Value; // update RegMem
            var ncpEdit = new NaviCustEdit(saveData, style);
            if (ncpEdit.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void btnSetRegChip_Click(object sender, EventArgs e)
        {
            var regChip = (int)cbxRegChip.SelectedValue;
            var regChipIndex = cbxRegChip.SelectedIndex;
            List<BattleChipData> chipNameMap;

            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork:
                    chipNameMap = BattleChipData.BN1ChipNameMap;
                    break;
                case GameTitle.Title.MegaManBattleNetwork2:
                    chipNameMap = BattleChipData.BN2ChipNameMap;
                    break;
                case GameTitle.Title.MegaManBattleNetwork3White:
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                    chipNameMap = BattleChipData.BN3ChipNameMap;
                    break;
                default:
                    return;
            }

            var bcd = BattleChipData.GetChipNameByID(chipNameMap, regChip);

            if(bcd.Size > nudRegMem.Value)
            {
                MessageBox.Show("Not enough RegMem for this chip.");
                return;
            }


            //This is because of the way Extra Folder is saved into Folder2Data
            switch (saveData.GameName)
            {
                case GameTitle.Title.MegaManBattleNetwork:
                case GameTitle.Title.MegaManBattleNetwork2:
                    switch (tabsFolders.SelectedIndex)
                    {
                        case 0:
                            saveData.RegChip1 = regChipIndex;
                            break;
                        case 1:
                            saveData.RegChip2 = regChipIndex;
                            break;
                        case 2:
                            saveData.RegChip3 = regChipIndex;
                            break;
                        default:
                            break;
                    }
                    break;
                case GameTitle.Title.MegaManBattleNetwork3White:
                case GameTitle.Title.MegaManBattleNetwork3Blue:
                    switch (tabsFolders.SelectedIndex)
                    {
                        case 0:
                            saveData.RegChip1 = regChipIndex;
                            break;
                        case 1:
                            saveData.RegChip2 = regChipIndex;
                            break;
                        case 2:
                            return;
                        default:
                            break;
                    }
                    break;
                default:
                    return;
            }

            MessageBox.Show($"RegChip set to {bcd.Name} [{bcd.AlphabeticalCode}]!");
        }
    }
}