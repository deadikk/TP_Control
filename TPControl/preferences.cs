using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TPControl
{
    public partial class preferences : Form
    {
        public preferences()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TPC main = this.Owner as TPC;
            textBox1.Text = "1024";
            textBox2.Text = "768";
        }

        private void ok_Click(object sender, EventArgs e)
        {
            TPC main = this.Owner as TPC;
            
                main.tab.Size = new Size(int.Parse(textBox1.Text) + 8, int.Parse(textBox2.Text) + 28);
                main.tab.Refresh();
                main.stripsSave();
                main.setUpFlags();
                this.Close();
        }

        private void preferences_Load(object sender, EventArgs e)
        {
            TPC main = this.Owner as TPC;
            textBox1.Text = (main.tab.Width-8).ToString();
            textBox2.Text = (main.tab.Height-28).ToString();

            this.AcceptButton = ok;
            this.CancelButton = cancel;

            checkBox1.Checked = main.stripLoading;
            checkBox2.Checked = main.aimDraw;

            autosaveCheck.Checked = main.autosave;
            autosaveMinutes.Value =main.autosaveTimer;
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            TPC main = this.Owner as TPC;
            main.stripLoading = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            TPC main = this.Owner as TPC;
            main.aimDraw = checkBox2.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            TPC main = this.Owner as TPC;
           
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            TPC main = this.Owner as TPC;
           
        }

        private void autosaveCheck_CheckedChanged(object sender, EventArgs e)
        {
            TPC main = this.Owner as TPC;
            main.autosave = autosaveCheck.Checked;

        }

        private void autosaveMinutes_ValueChanged(object sender, EventArgs e)
        {
            TPC main = this.Owner as TPC;
            main.autosaveTimer = (int)autosaveMinutes.Value;
            main.autosaveTimerSteps = main.autosaveTimer * 60 * (1000 / main.time.Interval);

        }
    }
}
