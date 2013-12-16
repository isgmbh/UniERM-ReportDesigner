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
    public partial class TextboxParameter : Reporting.Viewer.ParameterBase
    {
        public TextboxParameter() : this(null) {}
        public TextboxParameter(UserReportParameter rp) : base(rp)
        {
            InitializeComponent();

            if (rp != null)
            {
                queryLabel.Text = rp.Prompt;
                if (rp.DefaultValue != null)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < rp.DefaultValue.Length; i++)
                    {
                        if (i > 0)
                            sb.Append(", ");
                        sb.Append(rp.DefaultValue[i].ToString());
                    }
                    answerTextbox.Text = sb.ToString();
                }
            }
            ClearWarnings();
        }

        private void PositionErrorLabel(object sender, EventArgs e)
        {
            PositionControls(this.answerTextbox, this.errorLabel);
        }

        private void PositionAnswerField(object sender, EventArgs e)
        {
            PositionControls(this.queryLabel, this.answerTextbox);
        }

        private void answerTextbox_TextChanged(object sender, EventArgs e)
        {
            this.Value = answerTextbox.Text;
        }
    }

}

