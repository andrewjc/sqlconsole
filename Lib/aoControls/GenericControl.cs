using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace AOControls
{
    public class GenericControl:UserControl
    {
        protected System.Collections.Hashtable _paramList;

        public GenericControl()
        {
            this._paramList = new System.Collections.Hashtable();
        }

        public void setParam(string name, object value)
        {
            this._paramList[name] = value;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GenericControl
            // 
            this.Name = "GenericControl";
            this.Load += new System.EventHandler(this.GenericControl_Load);
            this.ResumeLayout(false);

        }

        private void GenericControl_Load(object sender, EventArgs e)
        {

        }

    }
}
