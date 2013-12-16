using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace ColourPicker
{
    public class ToolStripColourPicker : ToolStripSplitButton
    {
        public event EventHandler SelectedColourChanged;
        public event EventHandler ButtonPortionClicked;

        ColourPicker picker;

        public Color Colour
        {
            get { return picker.Colour; }
            set
            {
                picker.Colour = value;
                OnSelectedColorChanged(EventArgs.Empty);
            }
        }

        public ToolStripColourPicker() : base()
        {
            picker = new ColourPicker(8);
            picker.SelectedColourChanged += new EventHandler(HandleSelectedColourChanged);
        }

        protected override void OnButtonClick(EventArgs e)
        {
            base.OnButtonClick(e);
            OnButtonPortionClicked(e);
        }

        protected override void OnDropDownShow(EventArgs e)
        {
            Point p;
            if (this.Owner == null)
                p = new Point(5, 5);
            else
                p = this.Owner.PointToScreen(new Point(Bounds.Left, Bounds.Bottom));

            picker.Show(p);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Color lineColor = picker.Colour;
            if (!Enabled)
                lineColor = Color.Gray;
            using (Brush brush = new SolidBrush(lineColor))
            {
                Rectangle colourRect = new Rectangle(2, this.Height - 6, this.Width - 16, 4);
                e.Graphics.FillRectangle(brush, colourRect);
            }
        }

        private void HandleSelectedColourChanged(object sender, EventArgs e)
        {
            this.Invalidate();
            OnSelectedColorChanged(EventArgs.Empty);
        }

        public void OnSelectedColorChanged(EventArgs e)
        {
            if (SelectedColourChanged != null)
                SelectedColourChanged(this, e);
        }

        public void OnButtonPortionClicked(EventArgs e)
        {
            if (ButtonPortionClicked != null)
                ButtonPortionClicked(this, e);
        }
    }
}