using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Reporting.Rdl;

namespace Reporting.Viewer
{
    public partial class ParameterBase : UserControl
    {
        public event CancelEventHandler SettingWarnings;
        public event EventHandler WarningsSet;
        public event EventHandler WarningsCleared;

        protected bool _isValid = false;
        public bool IsParameterValid
        {
            get { return _isValid; }
        }

        protected string _value = string.Empty;
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public string Warnings
        {
            
            get { return tip.GetToolTip(this.errorLabel).ToString(); }
        }

        protected UserReportParameter _reportParameter = null;
        public UserReportParameter ReportParameter 
        {
            get { return _reportParameter; }
            set { _reportParameter = value; }
        }

        public ParameterBase():this(null) { }
        public ParameterBase(UserReportParameter rp)
        {
            ReportParameter = rp;
            InitializeComponent();
        }
        
        public void SetWarnings(string warning)
        {
            
            if (OnSettingWarnings(new CancelEventArgs()))
                return;

            this.errorLabel.Visible = true;
            this.tip.SetToolTip(this.errorLabel, warning);
            
            OnWarningsSet(new EventArgs());            
        }
        public void ClearWarnings()
        {
            this.errorLabel.Visible = false;
            tip.RemoveAll();

            OnWarningsCleared(new EventArgs());
        }

        public bool OnSettingWarnings(CancelEventArgs ce)
        {
            if (SettingWarnings != null)
                SettingWarnings(this, ce);

            return ce.Cancel;
        }
        public void OnWarningsSet(EventArgs e)
        {
            if (WarningsSet != null)
                WarningsSet(this, e);
        }
        public void OnWarningsCleared(EventArgs e)
        {
            if (WarningsCleared != null)
                WarningsCleared(this, e);
        }

        public new void Validate()
        {
            CancelEventArgs ce = new CancelEventArgs();
            this.OnValidating(ce);
            if (ce.Cancel)
                return;

            CheckParameterValidation();

            this.OnValidated(new EventArgs());
        }

        protected void CheckParameterValidation()
        {
            ClearWarnings();

            if (_reportParameter == null)
            {
                this._isValid = false;
                SetWarnings("This report parameter does not exist in this report.");
                return;
            }

            try
            {
                _reportParameter.Value = GetUserInput();
                this._isValid = true;
            }
            catch (ArgumentException ae)
            {
                this._isValid = false;
                SetWarnings(string.Format("Invalid Report Parameter: {0}", ae.Message));
            }

            if ((_reportParameter.Value == null || _reportParameter.Value.ToString() == string.Empty) 
                && (!_reportParameter.Nullable))
            {
                this._isValid = false;
                SetWarnings("This parameter is required but not provided.");
                return;
            }
        }

        protected virtual void PositionControls(Control leftControl, Control rightControl)
        {
            rightControl.Location =
                new Point(
                    (leftControl.Location.X + leftControl.Width + 3),
                    leftControl.Location.Y);
        }

        public virtual string GetUserInput() { return this.Value; }

        private void ParameterBase_ParentChanged(object sender, EventArgs e)
        {
            this.Width = this.Parent.Width;
        }
    }
}
