using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ColourPicker
{
    public partial class ColourPicker : Form
    {
        int cols;

        int buttonMargin = 1;

        Color colour;

        ColourButton[] colours;

        public Color Colour
        {
            get { return colour; }
            set
            {
                colour = value;
                OnSelectedColorChanged(EventArgs.Empty);
            }
        }

        public event EventHandler SelectedColourChanged;

        public ColourPicker(int cols)
        {
            this.cols = cols;
            InitializeComponent();
            InitColours();
            InitColourButtons();
            colour = colours[0].Colour;
        }

        private void InitColours()
        {
            colours =  new ColourButton[]
            {
                new ColourButton(this, "Black", Color.Black),
                new ColourButton(this, "Brown", Color.FromArgb(153, 51, 0)),
                new ColourButton(this, "OliveGreen", Color.FromArgb(51, 51, 0)),
                new ColourButton(this, "DarkGreen", Color.FromArgb(0, 51, 0)),
                new ColourButton(this, "DarkTeal", Color.FromArgb(0, 51, 102)),
                new ColourButton(this, "DarkBlue", Color.FromArgb(0, 0, 128)),
                new ColourButton(this, "Indigo", Color.FromArgb(51, 51, 153)),
                new ColourButton(this, "Gray80", Color.FromArgb(51, 51, 51)),
                // ------------------ Second Row ----------------------------
                new ColourButton(this, "DarkRed", Color.FromArgb(128, 0, 0)),
                new ColourButton(this, "Orange", Color.FromArgb(255, 102, 0)),
                new ColourButton(this, "DarkYellow", Color.FromArgb(128, 128, 0)),
                new ColourButton(this, "Green", Color.Green),
                new ColourButton(this, "Teal", Color.Teal),
                new ColourButton(this, "Blue", Color.Blue),
                new ColourButton(this, "BlueGray", Color.FromArgb(102, 102, 153)),
                new ColourButton(this, "Gray50", Color.FromArgb(128, 128, 128)),
                // ------------------ Third Row -----------------------------       
                new ColourButton(this, "Red", Color.Red),
                new ColourButton(this, "LightOrange", Color.FromArgb(255, 153, 0)),
                new ColourButton(this, "Lime", Color.FromArgb(153, 204, 0)),
                new ColourButton(this, "SeaGreen", Color.FromArgb(51, 153, 102)),
                new ColourButton(this, "Aqua", Color.FromArgb(51, 204, 204)),
                new ColourButton(this, "LightBlue", Color.FromArgb(51, 102, 255)),
                new ColourButton(this, "Violet", Color.FromArgb(128, 0, 128)),
                new ColourButton(this, "Gray40", Color.FromArgb(153, 153, 153)),
                // ----------------- Forth Row ------------------------------
                new ColourButton(this, "Pink", Color.FromArgb(255, 0, 255)),
                new ColourButton(this, "Gold", Color.FromArgb(255, 204, 0)),
                new ColourButton(this, "Yellow", Color.FromArgb(255, 255, 0)),
                new ColourButton(this, "BrightGreen", Color.FromArgb(0, 255, 0)),
                new ColourButton(this, "Turquoise", Color.FromArgb(0, 255, 255)),
                new ColourButton(this, "SkyBlue", Color.FromArgb(0, 204, 255)),
                new ColourButton(this, "Plum", Color.FromArgb(153, 51, 102)),
                new ColourButton(this, "Gray25", Color.FromArgb(192, 192, 192)),     
                // ----------------- Fifth Row ------------------------------
                new ColourButton(this, "Rose", Color.FromArgb(255, 153, 204)),
                new ColourButton(this, "Tan", Color.FromArgb(255, 204, 153)),
                new ColourButton(this, "LightYellow", Color.FromArgb(255, 255, 153)),
                new ColourButton(this, "LightGreen", Color.FromArgb(204, 255, 204)),
                new ColourButton(this, "LightTurquoise", Color.FromArgb(204, 255, 255)),
                new ColourButton(this, "PaleBlue", Color.FromArgb(153, 204, 255)),
                new ColourButton(this, "Lavender", Color.FromArgb(204, 153, 255)),
                new ColourButton(this, "White", Color.White)
            };
        }

        private void InitColourButtons()
        {
            colours[0].Location = new Point(buttonMargin, buttonMargin);
            this.Controls.Add(colours[0]);
            for (int i=1; i<colours.Length; i++)
            {
                this.Controls.Add(colours[i]);
                if (i % cols == 0)
                {
                    colours[i].Location = new Point(buttonMargin, colours[i - 1].Location.Y + colours[i - 1].Height + buttonMargin);
                }
                else
                {
                    colours[i].Location = new Point(colours[i - 1].Location.X + colours[i - 1].Width + buttonMargin, colours[i - 1].Location.Y);
                }
            }

            this.Width = colours[0].Width * cols + (cols + 1) * buttonMargin;

            int rows = (int)Math.Ceiling((double)colours.Length / (double)cols);
            this.Height = colours[0].Height * rows + (rows + 1) * buttonMargin;
        }

        public void Show(Point startLocation)
        {
            this.Location = startLocation;
            Show();
        }

        private void ColourPicker_Leave(object sender, EventArgs e)
        {
            Hide();
        }

        private void ColourPicker_Deactivate(object sender, EventArgs e)
        {
            Hide();
        }

        public void OnSelectedColorChanged(EventArgs e)
        {
            if (SelectedColourChanged != null)
                SelectedColourChanged(this, e);
        }
    }
}
