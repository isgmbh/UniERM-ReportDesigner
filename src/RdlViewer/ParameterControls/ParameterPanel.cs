using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Reporting.Rdl;
using Reporting.Viewer;
using Reporting.Viewer.Dialogs;
using System.Collections;


namespace Reporting.Viewer
{
    public partial class ParameterPanel : UserControl
    {
        public event EventHandler ValidationFailed;
        public event EventHandler RunReport;
        private IList _errorMsgs = null;

        public ParameterPanel()
        {
            InitializeComponent();
        }

        public void BuildParameters(Report r)
        {
            // Remove all previous controls
            this.flowPanel.Controls.Clear();
            this.flowPanel.AutoScroll = true;

            foreach (UserReportParameter rp in r.UserReportParameters)
            {
                if (rp.Prompt == null || rp.Prompt.Length == 0)		// skip parameters that don't have a prompt
                    continue;

                //create parameter.
                if (rp.DisplayValues == null)
                {
                    TextboxParameter tb = new TextboxParameter(rp);
                    tb.Parent = flowPanel;
                    tb.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
                    flowPanel.Controls.Add(tb);
                }
                else
                {
                    DropdownParameter dd = new DropdownParameter(rp);
                    dd.Parent = flowPanel;
                    dd.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
                    flowPanel.Controls.Add(dd);
                }
            }

            if (this.flowPanel.Controls.Count > 3)
            {
                this.flowPanel.AutoScroll = true;
                this.Height =
                    (this.flowPanel.Controls[0].Height * 3) + this.parameterMenu.Height + 15;
            }
            else
            {
                if (this.flowPanel.Controls.Count > 0)
                {
                    this.flowPanel.AutoScroll = false;
                    this.Height =
                        (this.flowPanel.Controls[0].Height * this.flowPanel.Controls.Count)
                        + this.parameterMenu.Height + 10;
                }
                else
                {
                    this.flowPanel.Visible = false;
                    this.Height = this.parameterMenu.Height;
                }
            }
        }

        private void ValidateParameters()
        {

        }

        private void RunReportClick(object sender, System.EventArgs e)
        {
            CancelEventArgs ce = new CancelEventArgs();
            this.OnValidating(ce);
            if (ce.Cancel)
                return;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                _RunButton.Enabled = false;
                _errorMsgs = null;			// reset the error message
                //if (this._Report == null)
                //    return;
                
                bool bFail = false;
                // Force parameters to get built
                foreach (ParameterBase ctl in this.flowPanel.Controls)
                {
                    ctl.Validate();
                    if (!ctl.IsParameterValid)
                    {
                        this.flowPanel.ScrollControlIntoView(ctl);
                        bFail = true;
                    }
                }


                if (bFail)
                {
                    OnValidationFailed(new EventArgs());
                    return;
                }
                else
                {
                    OnRunReport(new EventArgs());
                }

            }
            catch
            {
                // don't fail out;  occasionally get thread abort exception
            }
            finally
            {
                _RunButton.Enabled = true;
                Cursor.Current = Cursors.Default;
            }

        }

        private void OnValidationFailed(EventArgs e)
        {
            if (ValidationFailed != null)
                ValidationFailed(this, e);
        }

        private void OnRunReport(EventArgs e)
        {
            if (RunReport != null)
                RunReport(this, e);
        }

        private void WarningClick(object sender, System.EventArgs e)
		{
			if (_errorMsgs == null)
				return;						// shouldn't even be visible if no warnings

			DialogMessages dm = new DialogMessages(_errorMsgs);
			dm.ShowDialog();
			return;
        }
    }
}
