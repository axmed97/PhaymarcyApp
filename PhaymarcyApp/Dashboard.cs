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
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            pcWelcome.Location = new Point((pcWelcome.Parent.ClientSize.Width - pcWelcome.ClientSize.Width) / 2,
                (pcWelcome.Parent.ClientSize.Height - pcWelcome.ClientSize.Height) / 2
                );
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddMedicine addMedicine = new AddMedicine();
            addMedicine.ShowDialog();
        }
    }
}
