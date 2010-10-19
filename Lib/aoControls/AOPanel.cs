using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace AOControls
{
    public partial class AOPanel : Panel
    {
        protected Border3DStyle borderStyle3D = Border3DStyle.RaisedInner;
        protected System.Drawing.Drawing2D.LinearGradientMode gradientDir = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
        protected System.Drawing.Color color1;
        protected System.Drawing.Color color2;

        
        public AOPanel()
        {
            InitializeComponent();
            this.color1 = Color.LightGray;
            this.color2 = Color.White;
            //this.DoubleBuffered = true;
        }

        protected override void OnResize(EventArgs e)
        {
            this.Invalidate();
            base.OnResize(e);
        }
        // Called when the control gets focus. Need to repaint
        // the control to ensure the focus rectangle is drawn correctly.
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.Invalidate();
        }
        //
        // Called when the control loses focus. Need to repaint
        // the control to ensure the focus rectangle is removed.
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.Invalidate();
        }

        // Override this method with no code to avoid flicker.
        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            grads.drawGrad(this.color1, this.color2, this.ClientRectangle, e.Graphics, this.gradientDir);
            ControlPaint.DrawBorder3D(e.Graphics, this.ClientRectangle, borderStyle3D);
            
        }

        [Category("Appearance")]
        [Description("Gets or sets the 3D Border style")]
        public Border3DStyle Border3D 
        {
            get {
                return this.borderStyle3D;
            }
            set {
                this.borderStyle3D = value;
                this.Invalidate();
            }
        }
        [Category("Appearance")]
        [Description("Gets or sets the gradient direction")]
        public System.Drawing.Drawing2D.LinearGradientMode GradientDirection 
        {
            get {
                return this.gradientDir;
            }
            set {
                this.gradientDir = value;
                this.Invalidate();
            }
        }
        [Category("Appearance")]
        [Description("Gets or sets the gradient colour 1")]
        public  System.Drawing.Color ColourPrimary
        {
            get
            {
                return this.color1;
            }
            set
            {
                this.color1 = value;
                this.Invalidate();
            }
        }
        [Category("Appearance")]
        [Description("Gets or sets the gradient colour 2")]
        public System.Drawing.Color ColourSecondary
        {
            get
            {
                return this.color2;
            }
            set
            {
                this.color2 = value;
                this.Invalidate();
            }
        }
    }
}
