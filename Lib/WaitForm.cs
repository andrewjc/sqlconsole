using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

using System.Text;
using System.Windows.Forms;


namespace SQLConsole.Data
{
    public partial class WaitForm : Form
    {
        delegate void FormDel();
        public WaitForm()
        {
            InitializeComponent();

            
        }

        public void ShowA()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new FormDel(ShowA));
            }
            else
                this.Show();
        }

        public void DelayWait()
        {
            this.hideFormA();
            // wait 3 seconds, then hide the form.
            //System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(hideForm));
            //t.Start();
        }

        private void hideForm()
        {
            //System.Threading.Thread.Sleep(1000);
            //this.hideFormA();
        }

        private void hideFormA()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new FormDel(hideFormA));
            }
            else
                this.Hide();
        }

        private void label1_Click(object sender, EventArgs e)
        {


        }

       
    }
}