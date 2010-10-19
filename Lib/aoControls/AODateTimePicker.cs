using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace AOControls
{
    public partial class AODateTimePicker : System.Windows.Forms.DateTimePicker
    {
        public AODateTimePicker()
        {
            InitializeComponent();
            base.CustomFormat = "dddd dd-MM-yyyy hh:mm:ss tt";
            base.Format = DateTimePickerFormat.Custom;
            //this.FormatEx = dtpCustomExtensions.dtpLongDateLongTimeAMPM;
        }

        public override string Text
        {
            get
            {
                return Convert.ToString(base.Value.Ticks);
            }
            set
            {
                if (value != null)
                {
                    DateTime dtm = new DateTime((long)Convert.ToDouble(value));

                    base.Value = dtm;
                }
            }
        }

    }
}


