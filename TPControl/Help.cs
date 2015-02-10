using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TPControl
{
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
        }

        private void helpOk_Click(object sender, EventArgs e)
        {
            TPC main = this.Owner as TPC;
            this.Close();
            main.Activate();
        }

        private void Help_Load(object sender, EventArgs e)
        {
            webBrowser1.Url = new Uri((Application.StartupPath + "\\Help.html"));

        }
    }
}
