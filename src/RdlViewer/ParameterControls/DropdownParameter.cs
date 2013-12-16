using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Reporting.Rdl;

namespace Reporting.Viewer
{
    public partial class DropdownParameter : Reporting.Viewer.ParameterBase
    {
        public DropdownParameter() : this(null) {}
        public DropdownParameter(UserReportParameter rp) : base(rp)
        {
            InitializeComponent();

            if (rp != null)
            {
                queryLabel.Text = rp.Prompt;

                // create a label to autosize the combobox
                Label l = new Label();
                l.AutoSize = true;
                l.Visible = false;
                
                int width = 0;
                foreach (string s in rp.DisplayValues)
                {
                    l.Text = s;
                    if (width < l.Width)
                        width = l.Width;
                    answerDropdown.Items.Add(s);
                }
                if (width > 0)
                {
                    l.Text = "XX";
                    width += l.Width;		// give some extra room for the drop down arrow
                }
                else
                    width = 155;				// just force the default

                if (rp.DefaultValue != null)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < rp.DefaultValue.Length; i++)
                    {
                        if (i > 0)
                            sb.Append(", ");
                        sb.Append(rp.DefaultValue[i].ToString());
                    }
                    answerDropdown.Text = sb.ToString();
                }
            }
            ClearWarnings();
        }

        private void PositionErrorLabel(object sender, EventArgs e)
        {
            PositionControls(this.answerDropdown, this.errorLabel);
        }

        private void PositionAnswerField(object sender, EventArgs e)
        {
            PositionControls(this.queryLabel, this.answerDropdown);
        }

        public override string GetUserInput()
        {
            return this.answerDropdown.Text;
        }
    }

}

