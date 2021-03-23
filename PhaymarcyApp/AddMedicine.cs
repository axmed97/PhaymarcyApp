using iTextSharp.text;
using iTextSharp.text.pdf;
using PhaymarcyApp.Helper;
using PhaymarcyApp.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhaymarcyApp
{
    public partial class AddMedicine : Form
    {
        PhaymarcyDbEntities _context = new PhaymarcyDbEntities();
        public AddMedicine()
        {
            InitializeComponent();
        }

        private void FillFirmsComboBox()
        {
            cmbFirms.Items.AddRange(_context.Firms.Select(x => x.Firm_Name).ToArray());
        }

        private void FillTagsComboBox()
        {
            cmbTag.Items.AddRange(_context.Tags.Select(x => x.Tag_Name).ToArray());
        }

        private int FindFirm(string firmName)
        {
            Firm selectedFirm = _context.Firms.FirstOrDefault(x => x.Firm_Name == firmName);
            if (selectedFirm == null)
            {
                Firm firm = _context.Firms.Add(new Firm()
                {
                    Firm_Name = firmName
                });
                _context.SaveChanges();
                return firm.Id;
            }
            return selectedFirm.Id;
        }

        private void ClearAllField()
        {
            foreach (Control item in Controls)
            {
                if (item is TextBox || item is ComboBox || item is RichTextBox)
                {
                    item.Text = string.Empty;
                }
                else if(item is NumericUpDown)
                {
                    NumericUpDown numericUpDown = (NumericUpDown)item;
                    numericUpDown.Value = 1;
                }
                else if (item is DateTimePicker)
                {
                    DateTimePicker dateTimePicker = (DateTimePicker)item;
                    dateTimePicker.Value = DateTime.Now;
                }
                else if(item is CheckBox)
                {
                    CheckBox checkBox = (CheckBox)item;
                    checkBox.Checked = false;
                }
                else if(item is CheckedListBox)
                {
                    CheckedListBox checkedListBox = (CheckedListBox)item;
                    checkedListBox.Items.Clear();
                }
            }
        }

        private void FillMedicineDataGridView()
        {
            dtgMedicine.DataSource = _context.Medicines
                .Select(x => new 
                { 
                    x.Id,
                    x.Medicine_Name,
                    x.Price,
                    x.Quantity,
                    x.Firm.Firm_Name,
                    Receipt = x.IsReceipt ? "Reseptli" : "Reseptsiz",
                    x.Production_Date,
                    x.Experience_Date
                }).ToList();
            dtgMedicine.Columns[0].Visible = false;
            for (int i = 0; i < dtgMedicine.Rows.Count; i++)
            {
                if (dtgMedicine.Rows[i].Index % 2 == 0)
                {
                    dtgMedicine.Rows[i].DefaultCellStyle.BackColor = Color.LimeGreen;
                    dtgMedicine.Rows[i].DefaultCellStyle.ForeColor = Color.White;
                }
            }
        }

        private void txtBarcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < 47 || e.KeyChar > 58) && e.KeyChar != 8 && e.KeyChar != 46)
            {
                e.Handled = true;
            }
        }

        private bool CheckTag(string tagName)
        {
            Tag selectedTag = _context.Tags.FirstOrDefault(x => x.Tag_Name == tagName);
            if (selectedTag == null)
            {
                return false;
            }
            return true;
        }

        private void MedicineAddTag(int medicineId)
        {
            for (int i = 0; i < ckTagsList.Items.Count; i++)
            {
                string tagName = ckTagsList.Items[i].ToString();
                int tagId;
                if (CheckTag(tagName))
                {
                    tagId = _context.Tags.First(x => x.Tag_Name == tagName).Id;
                }
                else
                {
                    Tag tag = new Tag();
                    tag.Tag_Name = tagName;
                    _context.Tags.Add(tag);
                    _context.SaveChanges();
                    tagId = tag.Id;
                }
                _context.Medicine_To_Tag.Add(new Medicine_To_Tag()
                {
                    MedicineId = medicineId,
                    TagId = tagId
                });
                _context.SaveChanges();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string medicineName = txtName.Text;
            string barcode = txtBarcode.Text;
            decimal price = nmPrice.Value;
            int quantity = (int)nmQuantity.Value;
            DateTime productionDate = dtpProduction.Value;
            DateTime experienceDate = dtpExperience.Value;
            string tags = cmbTag.Text;
            string firms = cmbFirms.Text;
            string description = rtbDescription.Text;
            //string[] arr = { medicineName, barcode, firms };
            if (Helper.Utilities.IsEmpty(medicineName, barcode, firms))
            {
                if (productionDate < experienceDate)
                {
                    lblError.Visible = false;
                    int firmId = FindFirm(firms);
                    Medicine medicine = _context.Medicines.Add(new Medicine()
                    {
                        Medicine_Name = medicineName,
                        Barcode = barcode,
                        Price = price,
                        Quantity = quantity,
                        Production_Date = productionDate,
                        Experience_Date = experienceDate,
                        IsReceipt = ckIsReceipt.Checked ? true : false,
                        Description = description,
                        FirmId = firmId
                    });

                    _context.SaveChanges();
                    MedicineAddTag(medicine.Id);
                    ClearAllField();
                    FillMedicineDataGridView();
                }
                else
                {
                    lblError.Text = "Experience Date can not be more than Production Date";
                    lblError.Visible = true;
                }
            }
            else
            {
                lblError.Text = "Please fill all field!";
                lblError.Visible = true;
            }
        }

        private void AddMedicine_Load(object sender, EventArgs e)
        {
            FillFirmsComboBox();
            FillTagsComboBox();
            FillMedicineDataGridView();
        }

        private void CheckAndAddTags(string tagName)
        {
            if (tagName.Trim().Length != 0)
            {
                if (!ckTagsList.Items.Contains(tagName))
                {
                    ckTagsList.Items.Add(tagName, true);
                }
            }

        }

        private void cmbTag_SelectedIndexChanged(object sender, EventArgs e)
        {
            string tagName = cmbTag.Text;
            CheckAndAddTags(tagName);
        }

        private void cmbTag_KeyUp(object sender, KeyEventArgs e)
        {
            string tagName = cmbTag.Text;
            if (e.KeyCode == Keys.Enter)
            {
                CheckAndAddTags(tagName);
            }
        }

        private void ckTagsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = ckTagsList.SelectedIndex;

            if (selected != -1)
            {
                ckTagsList.Items.RemoveAt(selected);
            }
        }

        private void btnExcel_Click(object sender, EventArgs e)
        {
            // creating Excel Application  
            Microsoft.Office.Interop.Excel._Application app = new Microsoft.Office.Interop.Excel.Application();
            // creating new WorkBook within Excel application  
            Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
            // creating new Excelsheet in workbook  
            Microsoft.Office.Interop.Excel._Worksheet worksheet = null;
            // see the excel sheet behind the program  
            app.Visible = true;
            // get the reference of first sheet. By default its name is Sheet1.  
            // store its reference to worksheet  
            worksheet = workbook.Sheets["Sheet1"];
            worksheet = workbook.ActiveSheet;
            // changing the name of active sheet  
            string excelName = Guid.NewGuid().ToString();
            // storing header part in Excel  
            for (int i = 1; i < dtgMedicine.Columns.Count + 1; i++)
            {
                worksheet.Cells[1, i] = dtgMedicine.Columns[i - 1].HeaderText;
            }
            // storing Each row and column value to excel sheet  
            for (int i = 0; i < dtgMedicine.Rows.Count - 1; i++)
            {
                for (int j = 0; j < dtgMedicine.Columns.Count; j++)
                {
                    worksheet.Cells[i + 2, j + 1] = dtgMedicine.Rows[i].Cells[j].Value.ToString();
                }
            }
            // save the application  
            workbook.SaveAs($@"C:\Users\axmed\Downloads\Medicine_{excelName}.xlsx");
            // Exit from the application  
            app.Quit();
        }

        private void btnPdf_Click(object sender, EventArgs e)
        {
            if (dtgMedicine.Rows.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = "Output.pdf";
                bool fileError = false;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(sfd.FileName))
                    {
                        try
                        {
                            File.Delete(sfd.FileName);
                        }
                        catch (IOException ex)
                        {
                            fileError = true;
                            MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                        }
                    }
                    if (!fileError)
                    {
                        try
                        {
                            PdfPTable pdfTable = new PdfPTable(dtgMedicine.Columns.Count);
                            pdfTable.DefaultCell.Padding = 3;
                            pdfTable.WidthPercentage = 100;
                            pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;

                            foreach (DataGridViewColumn column in dtgMedicine.Columns)
                            {
                                PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText));
                                pdfTable.AddCell(cell);
                            }

                            foreach (DataGridViewRow row in dtgMedicine.Rows)
                            {
                                foreach (DataGridViewCell cell in row.Cells)
                                {
                                    pdfTable.AddCell(cell.Value.ToString());
                                }
                            }

                            using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
                            {
                                Document pdfDoc = new Document(PageSize.A4, 10f, 20f, 20f, 10f);
                                PdfWriter.GetInstance(pdfDoc, stream);
                                pdfDoc.Open();
                                pdfDoc.Add(pdfTable);
                                pdfDoc.Close();
                                stream.Close();
                            }

                            MessageBox.Show("Data Exported Successfully !!!", "Info");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error :" + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("No Record To Export !!!", "Info");
            }
        }
    }
}
