using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace AOControls
{
    class grads
    {

        static public void drawGrad(Color c1, Color c2, Rectangle rect, Graphics gr2)
        {
            drawGrad(c1, c2, rect, gr2, LinearGradientMode.Vertical);
        }

        static public void drawGrad(Color c1, Color c2, Rectangle rect, Graphics gr2, LinearGradientMode lgw)
        {
            using (LinearGradientBrush lgb = new
               LinearGradientBrush(rect,
                   c1, c2, lgw))

                gr2.FillRectangle(lgb, rect);
        }
    }
}
