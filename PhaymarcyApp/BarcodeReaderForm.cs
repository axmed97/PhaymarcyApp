using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using PhaymarcyApp.Model;
using ZXing;

namespace PhaymarcyApp
{
    public partial class BarcodeReaderForm : Form
    {
        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice videoCaptureDevice;
        PhaymarcyDbEntities _context = new PhaymarcyDbEntities();
        Medicine selectedMedicine;
        private readonly Worker _activeWorker;
        public BarcodeReaderForm(Worker activeWorker)
        {
            _activeWorker = activeWorker;
            InitializeComponent();
        }

        private void BarcodeReaderForm_Load(object sender, EventArgs e)
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filter in filterInfoCollection)
            {
                cmbCamera.Items.Add(filter.Name);
            }
            cmbCamera.SelectedIndex = 0;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[cmbCamera.SelectedIndex].MonikerString);
            videoCaptureDevice.NewFrame += videoCaptureDevice_NewFrame;
            videoCaptureDevice.Start();
        }

        private void videoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            BarcodeReader barcodeReader = new BarcodeReader();
            var result = barcodeReader.Decode(bitmap);
            if (result != null)
            {
                txtBarcode.Invoke(new MethodInvoker(delegate ()
                {
                    txtBarcode.Text = result.ToString();
                    selectedMedicine = _context.Medicines.FirstOrDefault(x => x.Barcode == txtBarcode.Text);
                    if (selectedMedicine != null)
                    {
                        txtMedName.Text = selectedMedicine.Medicine_Name;
                    }
                }));
            }
            cameraBox.Image = bitmap;
        }

        private void BarcodeReaderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoCaptureDevice != null)
            {
                if (videoCaptureDevice.IsRunning)
                {
                    videoCaptureDevice.Stop();
                }
            }
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
                panel1.Visible = false;
            }
            MessageBox.Show(result, "success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
