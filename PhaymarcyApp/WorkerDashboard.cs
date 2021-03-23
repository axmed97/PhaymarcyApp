using PhaymarcyApp.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhaymarcyApp
{
    public partial class WorkerDashboard : Form
    {
        PhaymarcyDbEntities _context = new PhaymarcyDbEntities();
        private readonly Worker _activeWorker;

        public WorkerDashboard(Worker activeWorker)
        {
            _activeWorker = activeWorker;
            InitializeComponent();
        }

        private void FillTagsComboBox()
        {
            cmbTags.Items.AddRange(_context.Tags.Select(x => x.Tag_Name).ToArray());
        }

        private void FillMedicineDataGridView()
        {
            dtgMedicine.DataSource = _context.Medicine_To_Tag
                .Where(x => x.Medicine.Medicine_Name.Contains(txtMedicineName.Text) && x.Tag.Tag_Name.Contains(cmbTags.Text))
                .Select(x => new
                {
                    x.Medicine.Id,
                    x.Medicine.Medicine_Name,
                    x.Medicine.Price,
                    x.Medicine.Quantity,
                    x.Medicine.Firm.Firm_Name,
                    x.Medicine.Description,
                    Receipt = x.Medicine.IsReceipt ? "Reseptli" : "Reseptsiz",
                    x.Medicine.Production_Date,
                    x.Medicine.Experience_Date,
                }).Distinct().ToList();
            dtgMedicine.Columns[0].Visible = false;

            for (int i = 0; i < dtgMedicine.Rows.Count; i++)
            {
                int quantity = (int)dtgMedicine.Rows[i].Cells[3].Value;
                DateTime experienceDate = (DateTime)dtgMedicine.Rows[i].Cells[8].Value;
                if (experienceDate < DateTime.Now)
                {
                    dtgMedicine.Rows[i].DefaultCellStyle.BackColor = Color.OrangeRed;
                    dtgMedicine.Rows[i].DefaultCellStyle.ForeColor = Color.White;
                }
                if (quantity <= 0)
                {
                    dtgMedicine.Rows[i].DefaultCellStyle.BackColor = Color.Crimson;
                    dtgMedicine.Rows[i].DefaultCellStyle.ForeColor = Color.White;
                }
                if (quantity <= 0 && experienceDate < DateTime.Now)
                {
                    dtgMedicine.Rows[i].DefaultCellStyle.BackColor = Color.Black;
                    dtgMedicine.Rows[i].DefaultCellStyle.ForeColor = Color.White;
                }
            }
        }

        private void WorkerDashboard_Load(object sender, EventArgs e)
        {
            FillMedicineDataGridView();
            FillTagsComboBox();
        }

        private void cmbTags_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillMedicineDataGridView();
        }

        private void txtMedicineName_TextChanged(object sender, EventArgs e)
        {
            FillMedicineDataGridView();
        }

        private void dtgMedicine_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int medId = (int)dtgMedicine.Rows[e.RowIndex].Cells[0].Value;
            Medicine selectedMedicine = _context.Medicines.First(x => x.Id == medId);

            if (selectedMedicine.Quantity > 0 && selectedMedicine.Experience_Date > DateTime.Now)
            {
                panel1.Visible = true;
                nmMedCount.Value = 1;
                txtMedName.Text = selectedMedicine.Medicine_Name;
                nmMedCount.Maximum = selectedMedicine.Quantity;
            }
            else
            {
                panel1.Visible = false;
            }
        }

        private void AddMedicineToList(string medicineName)
        {
            if (!ckMedicineList.Items.Contains(medicineName))
            {
                ckMedicineList.Items.Add(medicineName, true);
                btnSell.Enabled = true;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddMedicineToList(txtMedName.Text + "-" + nmMedCount.Value);
        }
        private void ClearAllField()
        {
            foreach (Control item in Controls)
            {
                if (item is TextBox || item is ComboBox || item is RichTextBox)
                {
                    item.Text = string.Empty;
                }
                else if (item is NumericUpDown)
                {
                    NumericUpDown numericUpDown = (NumericUpDown)item;
                    numericUpDown.Value = 1;
                }
                else if (item is DateTimePicker)
                {
                    DateTimePicker dateTimePicker = (DateTimePicker)item;
                    dateTimePicker.Value = DateTime.Now;
                }
                else if (item is CheckBox)
                {
                    CheckBox checkBox = (CheckBox)item;
                    checkBox.Checked = false;
                }
                else if (item is CheckedListBox)
                {
                    CheckedListBox checkedListBox = (CheckedListBox)item;
                    checkedListBox.Items.Clear();
                }
            }
        }

        private void ckMedicineList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = ckMedicineList.SelectedIndex;

            
            if (selectedIndex != -1)
            {
                ckMedicineList.Items.RemoveAt(selectedIndex);
                if (ckMedicineList.Items.Count == 0)
                {
                    btnSell.Enabled = false;
                }
            }
        }

        private void btnSell_Click(object sender, EventArgs e)
        {
            string result = "";
            for (int i = 0; i < ckMedicineList.Items.Count; i++)
            {

                string medItem = ckMedicineList.Items[i].ToString();
                string medName = medItem.Substring(0, medItem.LastIndexOf("-"));
                int medCount = Convert.ToInt32(medItem.Substring(medItem.LastIndexOf("-") + 1));

                Medicine selectedMedicine = _context.Medicines.First(x => x.Medicine_Name == medName);

                _context.Orders.Add(new Order()
                {
                    WorkerId = _activeWorker.Id,
                    Purchase_Date = DateTime.Now,
                    Count = medCount,
                    Amount = selectedMedicine.Price,
                    MedicineId = selectedMedicine.Id
                });
                selectedMedicine.Quantity -= medCount;
                _context.SaveChanges();
                result += $"Medicine Name - {medName}, Medicine Count - {medCount}, Medicine Price - {selectedMedicine.Price} AZN; \n";
                ClearAllField();
                FillMedicineDataGridView();
                panel1.Visible = false;
            }
            MessageBox.Show(result, "success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnBarcode_Click(object sender, EventArgs e)
        {
            BarcodeReaderForm barcodeReaderForm = new BarcodeReaderForm(_activeWorker);
            barcodeReaderForm.ShowDialog();
        }
    }
}
