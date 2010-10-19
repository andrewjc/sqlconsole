using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace AOControls
{
    public partial class AOTabPanel : TabControl
    {
        public AOTabPanel()
        {
            InitializeComponent();
            this.Appearance = TabAppearance.Normal;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;

            Font font = new Font("Trebuchet MS", 10.0f);
            
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            
            

            Rectangle rect = this.GetTabRect(e.Index);
            LinearGradientBrush lgb = new LinearGradientBrush(this.GetTabRect(e.Index), Color.LightSteelBlue, Color.White,270F );
            g.FillRectangle(lgb, this.GetTabRect(e.Index));


           
            g.DrawString(this.TabPages[e.Index].Text, font, Brushes.Black, (RectangleF)this.GetTabRect(e.Index), sf);
            if (e.State == DrawItemState.Selected)
            {
                e.DrawFocusRectangle();
            }
        }

        
    }
}
