namespace Reporting.Viewer
{
    partial class ParameterPanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.parameterMenu = new System.Windows.Forms.MenuStrip();
            this._RunButton = new System.Windows.Forms.ToolStripMenuItem();
            this._WarningButton = new System.Windows.Forms.ToolStripMenuItem();
            this.flowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.parameterMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // parameterMenu
            // 
            this.parameterMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._RunButton,
            this._WarningButton});
            this.parameterMenu.Location = new System.Drawing.Point(0, 0);
            this.parameterMenu.Name = "parameterMenu";
            this.parameterMenu.Size = new System.Drawing.Size(358, 24);
            this.parameterMenu.TabIndex = 0;
            this.parameterMenu.Text = "menuStrip1";
            // 
            // _RunButton
            // 
            this._RunButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._RunButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._RunButton.Name = "_RunButton";
            this._RunButton.Size = new System.Drawing.Size(74, 20);
            this._RunButton.Text = "Run Report";
            this._RunButton.ToolTipText = "Click to generate the report.";
            this._RunButton.Click += new System.EventHandler(this.RunReportClick);
            // 
            // _WarningButton
            // 
            this._WarningButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._WarningButton.AutoToolTip = true;
            this._WarningButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._WarningButton.Image = global::Reporting.Viewer.Properties.Resources.exclamation;
            this._WarningButton.Name = "_WarningButton";
            this._WarningButton.Size = new System.Drawing.Size(28, 20);
            this._WarningButton.Text = "View Warnings";
            this._WarningButton.ToolTipText = "Click to see Report Warnings.";
            this._WarningButton.Visible = false;
            this._WarningButton.Click += new System.EventHandler(this.WarningClick);
            // 
            // flowPanel
            // 
            this.flowPanel.AutoScroll = true;
            this.flowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPanel.Location = new System.Drawing.Point(0, 24);
            this.flowPanel.Name = "flowPanel";
            this.flowPanel.Size = new System.Drawing.Size(358, 49);
            this.flowPanel.TabIndex = 1;
            this.flowPanel.WrapContents = false;
            // 
            // ParameterPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.flowPanel);
            this.Controls.Add(this.parameterMenu);
            this.Name = "ParameterPanel";
            this.Size = new System.Drawing.Size(358, 73);
            this.parameterMenu.ResumeLayout(false);
            this.parameterMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip parameterMenu;
        private System.Windows.Forms.ToolStripMenuItem _RunButton;
        private System.Windows.Forms.ToolStripMenuItem _WarningButton;
        private System.Windows.Forms.FlowLayoutPanel flowPanel;
    }
}
