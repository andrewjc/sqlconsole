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
    public partial class AOButton : Button
    {
        protected Color gradientColorOne = Color.White;
        protected Color gradientColorTwo = Color.LightGray;
        protected LinearGradientMode lgm =
                LinearGradientMode.Vertical;
        protected Border3DStyle b3dstyle = Border3DStyle.Raised;
        protected Point lastCursorpos;
        protected Bitmap bmDoubleBuffer;
        protected bool mouseOver = false;
        protected bool focusOK = false;
        public AOButton()
        {
            InitializeComponent();
        }

        private Bitmap DoubleBufferImage
        {
            get
            {
                if (bmDoubleBuffer == null)
                    bmDoubleBuffer = new Bitmap(
                        this.ClientSize.Width,
                        this.ClientSize.Height);
                return bmDoubleBuffer;
            }
            set
            {
                if (bmDoubleBuffer != null)
                    bmDoubleBuffer.Dispose();
                bmDoubleBuffer = value;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            DoubleBufferImage = new Bitmap(
                this.ClientSize.Width,
                this.ClientSize.Height);
            base.OnResize(e);
        }
        // Called when the control gets focus. Need to repaint
        // the control to ensure the focus rectangle is drawn correctly.
        protected override void OnGotFocus(EventArgs e)
        {
            this.focusOK = true;
            base.OnGotFocus(e);
            this.Invalidate();
        }
        //
        // Called when the control loses focus. Need to repaint
        // the control to ensure the focus rectangle is removed.
        protected override void OnLostFocus(EventArgs e)
        {
            this.focusOK = false;
            base.OnLostFocus(e);
            this.Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point coord = new Point(e.X, e.Y);
            if (this.Capture)
            {
                
                if (this.ClientRectangle.Contains(coord) !=
                    this.ClientRectangle.Contains(lastCursorCoordinates))
                {
                    DrawButton(this.ClientRectangle.Contains(coord));
                }
               
            }
            else
            {
                mouseOver = true;
                    DrawButton(false);
               
            }
            lastCursorCoordinates = coord;
            base.OnMouseMove(e);
        }

        // The coordinates of the cursor the last time
        // there was a MouseUp or MouseDown message.
        Point lastCursorCoordinates;

        protected override void OnMouseLeave(EventArgs e)
        {
            mouseOver = false;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Start capturing the mouse input
                this.Capture = true;
                // Get the focus because button is clicked.
                this.Focus();

                // draw the button
                DrawButton(true);
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            this.Capture = false;

            DrawButton(false);

            base.OnMouseUp(e);
        }

        bool bGotKeyDown = false;
        protected override void OnKeyDown(KeyEventArgs e)
        {
            bGotKeyDown = true;
            switch (e.KeyCode)
            {
                case Keys.Space:
                case Keys.Enter:
                    DrawButton(true);
                    break;
                case Keys.Up:
                case Keys.Left:
                    this.Parent.SelectNextControl(this, false, false, true, true);
                    break;
                case Keys.Down:
                case Keys.Right:
                    this.Parent.SelectNextControl(this, true, false, true, true);
                    break;
                default:
                    bGotKeyDown = false;
                    base.OnKeyDown(e);
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                case Keys.Enter:
                    if (bGotKeyDown)
                    {
                        DrawButton(false);
                        OnClick(EventArgs.Empty);
                        bGotKeyDown = false;
                    }
                    break;
                default:
                    base.OnKeyUp(e);
                    break;
            }
        }

        // Override this method with no code to avoid flicker.
    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        DrawButton(e.Graphics, this.Capture &&
            (this.ClientRectangle.Contains(lastCursorCoordinates)));
    }

    //
    // Gets a Graphics object for the provided window handle
    //  and then calls DrawButton(Graphics, bool).
    //
    // If pressed is true, the button is drawn
    // in the depressed state.
    void DrawButton(bool pressed)
    {
        Graphics gr = this.CreateGraphics();
        DrawButton(gr, pressed);
        gr.Dispose();
    }

        void DrawButton(Graphics gr, bool pressed)
        {
            Graphics gr2 = Graphics.FromImage(DoubleBufferImage);
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            Color fontColor = Color.Black;
            Font fontFont = this.Font;

           

            grads.drawGrad(Color.White, Color.LightGray, rect, gr2);
                       // Draw the border.
            // Need to shrink the width and height by 1 otherwise
            // there will be no border on the right or bottom.

            if (pressed)
            {
                ControlPaint.DrawBorder3D(gr2, rect, Border3DStyle.Sunken);
            }
            else
            {
                if (mouseOver)
                {
                    grads.drawGrad(Color.White, Color.LightSteelBlue, rect, gr2);
                    //ControlPaint.DrawBorder(gr2, rect, Color.Black, ButtonBorderStyle.Solid);
                    ControlPaint.DrawBorder3D(gr2, rect, Border3DStyle.RaisedInner);
                    fontColor = Color.Black;
                    //fontFont = new Font(this.Font, FontStyle.Bold);
                }
                else

                    if (base.IsDefault)
                    {
                        grads.drawGrad(Color.White, Color.LightSteelBlue, rect, gr2);
                        ControlPaint.DrawBorder3D(gr2, rect, Border3DStyle.RaisedInner);
                        gr2.DrawLines(new Pen(Brushes.Black, 1), new Point[] { new Point(0, rect.Height-1), new Point(rect.Width-1, rect.Height-1), new Point(rect.Width-1, 0) });

                        //gr2.DrawRectangle(new Pen(Brushes.DarkSlateGray), Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right-1, rect.Bottom-1));
                        //ControlPaint.DrawBorder(gr2, rect, Color.Black, ButtonBorderStyle.Outset);
                    }
                    else
                    {
                        ControlPaint.DrawBorder3D(gr2, rect, Border3DStyle.RaisedInner);
                    }
            }
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Show;

            gr2.DrawString(this.Text, fontFont, new SolidBrush(fontColor), this.ClientRectangle, sf);

            // Draw from the background image onto the screen.
            gr.DrawImage(DoubleBufferImage, 0, 0);
            gr2.Dispose();


        }

       
        
    }
}
