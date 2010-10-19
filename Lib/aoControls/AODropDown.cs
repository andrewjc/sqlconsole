using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace AOControls
{
    public partial class AODropDown : ComboBox
    {
        private System.Collections.Hashtable _list;
        private System.Collections.Hashtable _list2;
        public struct ListValue
        {
            public string value;
            public string text;
        }
        public AODropDown()
            : base()
        {
            InitializeComponent();
            this._list = new System.Collections.Hashtable();
            this._list2 = new System.Collections.Hashtable();
        }

        public void addItem(string text, string value)
        {
            try
            {
                if(!this._list.ContainsKey(text)) {
                ListValue d = new ListValue();
                d.text = text;
                d.value = value;
                this._list.Add(text, d);
                this._list2.Add(value, d);
                this.Items.Add(text);
                }
            }
            catch { }
        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            this.OnMouseDown(e);
        }

        public override string Text
        {
            get
            {
                try
                {
                    string ret = "";
                    if(base.Text != null && base.Text != "")
                    if (this._list.ContainsKey(base.Text))
                    ret = ((ListValue)this._list[base.Text]).value;
                    return ret;
                }
                catch { return null; }
            }
            set
            {
                
                if(value != null && value != "")
                    if (this._list2.ContainsKey(value))
                        base.Text = ((ListValue)this._list2[value]).text;
            }
        }

       

        public ListValue getSelected()
        {
            ListValue d = (ListValue)this._list[base.Text];
            return getSelected();
        }
    }
}
