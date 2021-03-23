using PhaymarcyApp.Helper;
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
    public partial class Login : Form
    {
        PhaymarcyDbEntities _context = new PhaymarcyDbEntities();

        public Login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (Utilities.IsEmpty(username, password))
            {
                Worker selectedWorker = _context.Workers.FirstOrDefault(x => x.Fullname == username);
                if (selectedWorker != null)
                {
                    if (selectedWorker.Password == password.HashCode())
                    {
                        if (ckbRemember.Checked)
                        {
                            Properties.Settings.Default.password = txtPassword.Text;
                            Properties.Settings.Default.username = txtUsername.Text;
                            Properties.Settings.Default.isChecked = true;
                            Properties.Settings.Default.Save();
                        }
                        else
                        {
                            Properties.Settings.Default.password = string.Empty;
                            Properties.Settings.Default.username = string.Empty;
                            Properties.Settings.Default.isChecked = false;
                            Properties.Settings.Default.Save();
                        }
                        switch (selectedWorker.RoleId)
                        {
                            case 1:
                                Dashboard dashboard = new Dashboard();
                                dashboard.ShowDialog();
                                break;
                            case 2:
                                WorkerDashboard workerDashboard = new WorkerDashboard(selectedWorker);
                                workerDashboard.ShowDialog();
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Username or Password is not valid!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Worker does not exist!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.isChecked)
            {
                txtUsername.Text = Properties.Settings.Default.username;
                txtPassword.Text = Properties.Settings.Default.password;
                ckbRemember.Checked = true;
            }
        }
    }
}
