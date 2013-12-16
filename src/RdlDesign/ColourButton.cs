using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace ColourPicker
{
    class ColourButton : Button
    {
        ColourPicker parentPicker;
        String name;
        Color colour;
        bool drawBorder = false;

        public Color Colour
        {
            get { return colour; }
            set { colour = value; }
        }

        public ColourButton(ColourPicker parent, String name, Color colour)
        {
            parentPicker = parent;

            this.name = name;
            this.colour = colour;

            this.Width = 15;
            this.Height = 15;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(new Point(0,0), new Size(this.Width, this.Height));
            Brush brush = new SolidBrush(colour);
            e.Graphics.FillRectangle(brush, rect);

            if (this.drawBorder)
            {
                rect.Height -= 1;
                rect.Width -= 1;
                Pen pen = new Pen(new SolidBrush(Color.FromArgb(255, 255, 255)), 1);
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            drawBorder = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            drawBorder = false;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            parentPicker.Colour = Colour;
            parentPicker.Hide();
        }
    }
}
