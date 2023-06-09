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

namespace NaviDoctor
{
    public partial class MainForm : Form
    {
        private SaveDataObject saveData;
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

                form.Width = 860;
                form.Height = 500;
                form.FormBorderStyle = FormBorderStyle.FixedSingle; // Disable resizing
                form.MaximizeBox = false; // Disable maximize button

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
            public void GenerateConfirmButton(SaveDataObject saveData)
            {
                confirmButton = new Button();
                confirmButton.Text = "Confirm";
                confirmButton.Anchor = AnchorStyles.None;
                confirmButton.Top = flowLayoutPanel.Bottom + 10;
                confirmButton.Left = ((form.Width - confirmButton.Width) / 2) + 50;
                confirmButton.Click += (sender, e) => ConfirmButton_Click(sender, e, saveData);

                form.Controls.Add(confirmButton);
            }
            private void ConfirmButton_Click(object sender, EventArgs e, SaveDataObject saveData)
            {
                UpdateLibraryData(saveData);
                form.Close();
            }
            private void UpdateLibraryData(SaveDataObject saveData)
            {
                foreach (Control control in flowLayoutPanel.Controls)
                {
                    if (control is CheckBox checkBox)
                    {
                        string chipName = checkBox.Text;
                        int chipIndex = BattleChipData.GetChipIDByName(chipName);

                        if (chipIndex != -1)
                        {
                            int libraryIndex = chipIndex / 8;
                            int bitIndex = 7 - (chipIndex % 8);

                            if (checkBox.Checked)
                                saveData.LibraryData[libraryIndex] |= (byte)(1 << bitIndex);
                            else
                                saveData.LibraryData[libraryIndex] &= (byte)~(1 << bitIndex);
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
        private void GenerateLibraryWindow(SaveDataObject saveData)
        {
            LibraryWindow libraryWindow = new LibraryWindow();

            for (int i = 1; i <= 199; i++)
            {
                string chipName = BattleChipData.GetChipNameByID(i);

                if (chipName.Length < 3)
                    continue;

                int libraryIndex = (i) / 8;
                int bitIndex = 7 - ((i) % 8);
                bool isChecked = (saveData.LibraryData[libraryIndex] & (1 << bitIndex)) != 0;

                libraryWindow.AddChip(chipName, isChecked);
            }

            libraryWindow.GenerateCheckAllButton();
            libraryWindow.GenerateConfirmButton(saveData);
            libraryWindow.ShowDialog();
        }
        private string GetAlphabeticalCode(byte chipCode)
        {
            char chipCodeLetter = (char)(chipCode + 0x41);
            return chipCodeLetter.ToString();
        }
        private void PopulateDataGridView(BattleChipData battleChipData, SaveDataObject saveData)
        {
            List<BattleChipData> entries = battleChipData.GenerateChipEntries();
            List<byte> packChips = saveData.BattleChips.Concat(saveData.NaviChips).ToList();

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Quantity = packChips[i];
            }

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Code", typeof(string));
            dataTable.Columns.Add("Quantity", typeof(int));

            foreach (var entry in entries)
            {
                dataTable.Rows.Add(entry.ID, entry.Name, entry.Code, entry.Quantity);
            }

            dataTable.DefaultView.RowFilter = "Code <> 'None'";

            dataGridView1.DataSource = dataTable;

            dataGridView1.Columns["ID"].Visible = false;
            dataGridView1.Columns["Name"].ReadOnly = true;
            dataGridView1.Columns["Name"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridView1.Columns["Code"].ReadOnly = true;
            dataGridView1.Columns["Quantity"].ReadOnly = false;

            dataGridView1.AutoResizeColumns();
        }
        private void LoadFolderData(SaveDataObject saveData)
        {
            DataTable folderDataTable = new DataTable();
            folderDataTable.Columns.Add("ID", typeof(int));
            folderDataTable.Columns.Add("Name", typeof(string));
            folderDataTable.Columns.Add("Code", typeof(string));

            foreach (var folderData in saveData.FolderData)
            {
                byte chipID = folderData.Item1;
                byte chipCode = folderData.Item2;

                string chipName = BattleChipData.GetChipNameByID(chipID);
                string chipCodeLetter = GetAlphabeticalCode(chipCode);

                folderDataTable.Rows.Add(chipID, chipName, chipCodeLetter);
            }
            // Preserve the current selection and scroll position
            int selectedRowIndex = -1;
            int firstDisplayedScrollingRowIndex = -1;

            if (dataGridView2.SelectedRows.Count > 0)
                selectedRowIndex = dataGridView2.SelectedRows[0].Index;

            if (dataGridView2.FirstDisplayedScrollingRowIndex >= 0)
                firstDisplayedScrollingRowIndex = dataGridView2.FirstDisplayedScrollingRowIndex;

            dataGridView2.DataSource = null;
            dataGridView2.Rows.Clear();

            dataGridView2.DataSource = folderDataTable;
            dataGridView2.Columns["ID"].Visible = false;

            // Restore the previous selection and scroll position
            if (selectedRowIndex >= 0 && selectedRowIndex < dataGridView2.Rows.Count)
                dataGridView2.Rows[selectedRowIndex].Selected = true;

            if (firstDisplayedScrollingRowIndex >= 0 && firstDisplayedScrollingRowIndex < dataGridView2.Rows.Count)
                dataGridView2.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex;

            dataGridView2.Columns["Name"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        }
        private void btnRemoveChip_Click(object sender, EventArgs e)
        {
            // Check if there is a selected row in the Folder DataGridView
            if (dataGridView2.SelectedRows.Count > 0)
            {
                // Get the selected chip ID
                int chipID = (int)dataGridView2.SelectedRows[0].Cells["ID"].Value;
                string chipCode = (string)dataGridView2.SelectedRows[0].Cells["Code"].Value;
                int chipInt = chipCode[0] - 'A';

                // Find the first occurrence of the chip in the FolderData list
                var chipToRemove = saveData.FolderData.FirstOrDefault(data => data.Item1 == chipID && data.Item2 == chipInt);

                // Remove the chip if found
                if (chipToRemove != null)
                {
                    saveData.FolderData.Remove(chipToRemove);
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
        }
        // check the quantity of a chip ID and code in the Folder
        private int GetChipQuantityInFolder(int chipID, string chipCode)
        {
            byte codeValue = (byte)(chipCode[0] - 'A');
            return saveData.FolderData.Count(data => data.Item1 == chipID && data.Item2 == codeValue);
        }
        private void btnAddChip_Click(object sender, EventArgs e)
        {
            // Check if there is a selected row in the Pack DataGridView
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Get the selected chip ID and code from the Pack
                int chipID = (int)dataGridView1.SelectedRows[0].Cells["ID"].Value;
                string chipCode = dataGridView1.SelectedRows[0].Cells["Code"].Value.ToString();

                // Check if the Folder has reached the maximum number of copies for the selected chip ID
                int currentChipCopies = saveData.FolderData.Count(data => data.Item1 == chipID);
                int currentNaviChips = saveData.FolderData.Count(data => data.Item1 >= 148 && data.Item1 <= 239);

                bool isBattleChip = chipID >= 1 && chipID <= 147;
                bool isNaviChip = chipID >= 148 && chipID <= 239;

                int maxBattleChipCopies = 10;
                int maxNaviChips = 5;
                int maxTotalChipsInFolder = 30;

                if (isBattleChip && currentChipCopies >= maxBattleChipCopies)
                {
                    string chipName = BattleChipData.GetChipNameByID(chipID);
                    MessageBox.Show($"The limit for {chipName} cannot exceed {maxBattleChipCopies}.");
                }
                else if (isNaviChip && currentNaviChips >= maxNaviChips)
                {
                    MessageBox.Show($"The limit for Navi Chips cannot exceed {maxNaviChips}.");
                }
                else if (saveData.FolderData.Count >= maxTotalChipsInFolder)
                {
                    MessageBox.Show($"The number of chips in the folder cannot exceed {maxTotalChipsInFolder}.");
                }
                else
                {
                    // Check the quantity of the chip ID and code in the Folder
                    int folderChipQuantity = GetChipQuantityInFolder(chipID, chipCode);

                    // Increment the quantity in the Pack if the quantity in the Folder is less than or equal to the quantity in the Pack
                    int packChipQuantity = (int)dataGridView1.SelectedRows[0].Cells["Quantity"].Value;
                    if (folderChipQuantity >= packChipQuantity)
                    {
                        dataGridView1.SelectedRows[0].Cells["Quantity"].Value = folderChipQuantity + 1;
                    }

                    // Add the chip to the FolderData list with the selected chip code
                    saveData.FolderData.Add(new Tuple<byte, byte>((byte)chipID, (byte)(chipCode[0] - 'A')));

                    // Refresh the Folder DataGridView
                    LoadFolderData(saveData);
                }
            }
            else
            {
                MessageBox.Show("Please select a chip in the Pack view.");
            }
        }
        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Title = "Open MMBN Save File...";
            openFile.InitialDirectory = @"C:\Program Files (x86)\Steam\userdata";
            openFile.Filter = "MMBN1 Save File|exe1_save_0.bin|All Files|*.*";
            openFile.RestoreDirectory = true;
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    SaveParse saveParse = new SaveParse(openFile.FileName);
                    saveData = saveParse.ExtractSaveData();
                    BattleChipData battleChipData = new BattleChipData();
                    List<BattleChipData> bChips = battleChipData.GenerateChipEntries();
                    PopulateDataGridView(battleChipData, saveData);
                    LoadFolderData(saveData);
                    maxHPStat.Value = saveData.MaxHP;
                    attackStat.Value = saveData.AttackPower + 1;
                    rapidStat.Value = saveData.RapidPower + 1;
                    chargeStat.Value = saveData.ChargePower + 1;
                    zennyBox.Value = saveData.Zenny;
                    steamID.Value = saveData.SteamID;
                    haveFireArmor.Checked = saveData.Style1 == 1;
                    haveAquaArmor.Checked = saveData.Style2 == 1;
                    haveWoodArmor.Checked = saveData.Style3 == 1;
                    fireArmorRadio.Checked = saveData.EqStyle == 02;
                    aquaArmorRadio.Checked = saveData.EqStyle == 03;
                    woodArmorRadio.Checked = saveData.EqStyle == 04;
                    normalArmorRadio.Checked = !(fireArmorRadio.Checked || aquaArmorRadio.Checked || woodArmorRadio.Checked);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex}");
                }
            }
        }
        private void btnSaveFile_Click(object sender, EventArgs e)
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
            saveFile.FileName = "exe1_save_0.bin";
            saveFile.Filter = "MMBN1 Save File|exe1_save_0.bin|AllFiles|*.*";
            saveFile.RestoreDirectory = true;
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    SaveParse bn1SaveParse = new SaveParse(saveFile.FileName);

                    saveData.MaxHP = (short)maxHPStat.Value;
                    saveData.CurrHP = (short)maxHPStat.Value;
                    saveData.AttackPower = (byte)(attackStat.Value - 1);
                    saveData.RapidPower = (byte)(rapidStat.Value - 1);
                    saveData.ChargePower = (byte)(chargeStat.Value - 1);
                    saveData.Zenny = (int)zennyBox.Value;
                    saveData.SteamID = (int)steamID.Value;
                    if (haveAquaArmor.Checked)
                    { saveData.Style2 = 1; }
                    else 
                    { saveData.Style2 = 0; }
                    if (haveFireArmor.Checked)
                    { saveData.Style1 = 1; }
                    else 
                    { saveData.Style1 = 0; }
                    if (haveWoodArmor.Checked)
                    { saveData.Style3 = 1; }
                    else
                    { saveData.Style3 = 0; }
                    if (fireArmorRadio.Checked)
                    {
                        saveData.EqStyle = 2;
                    }
                    else if (aquaArmorRadio.Checked)
                    {
                        saveData.EqStyle = 3;
                    }
                    else if (woodArmorRadio.Checked)
                    {
                        saveData.EqStyle = 4;
                    }
                    else
                    {
                        saveData.EqStyle = 0;
                    }
                    PackageChips();
                    bn1SaveParse.UpdateSaveData(saveData);
                    bn1SaveParse.SaveChanges();

                    MessageBox.Show("Save data successfully updated!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while saving the file: {ex.Message}");
                }
            }
        }
        private void PackageChips()
        {
            List<byte> newBattleChips = new List<byte>(Enumerable.Repeat((byte)0, 147 * 5));
            List<byte> newNaviChips = new List<byte>(Enumerable.Repeat((byte)0, 239));

            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Ascending); // Sort by ID column in ascending order

            int codeSeries = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["ID"].Value != null && row.Cells["Quantity"].Value != null)
                {
                    int chipID = (int)row.Cells["ID"].Value;
                    int quantity = (int)row.Cells["Quantity"].Value;

                    quantity = Math.Max(0, Math.Min(quantity, 99));

                    if (chipID >= 1 && chipID <= 147)
                    {
                        int index = ((chipID - 1) * 5) + codeSeries;
                            newBattleChips[index] = (byte)quantity;
                        codeSeries = (codeSeries + 1) % 5;
                    }
                    else if (chipID >= 148 && chipID <= 239)
                    {
                        newNaviChips[chipID - 148] = (byte)quantity;
                    }
                }
            }

            saveData.BattleChips = newBattleChips;
            saveData.NaviChips = newNaviChips;
        }



        private void steamID_Enter(object sender, EventArgs e)
        {
            steamID.BackColor = Color.White;
        }

        private void steamID_Leave(object sender, EventArgs e)
        {
            steamID.BackColor = Color.Black;
        }
        private void btnShowLibrary_Click(object sender, EventArgs e)
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

    }
}