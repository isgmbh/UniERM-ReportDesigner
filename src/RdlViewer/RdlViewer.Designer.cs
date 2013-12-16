namespace Reporting.Viewer
{
    partial class RdlViewer
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
            this.components = new System.ComponentModel.Container();
            this._vScroll = new System.Windows.Forms.VScrollBar();
            this._hScroll = new System.Windows.Forms.HScrollBar();
            this._vScrollToolTip = new System.Windows.Forms.ToolTip(this.components);
            this._DrawPanel = new Reporting.Viewer.Canvas.Canvas();
            this._ParameterPanel = new Reporting.Viewer.ParameterPanel();
            this._FindCtl = new Reporting.Viewer.ViewerSearchBar();
            this.SuspendLayout();
            // 
            // _vScroll
            // 
            this._vScroll.Dock = System.Windows.Forms.DockStyle.Right;
            this._vScroll.Enabled = false;
            this._vScroll.Location = new System.Drawing.Point(599, 0);
            this._vScroll.Name = "_vScroll";
            this._vScroll.Size = new System.Drawing.Size(17, 318);
            this._vScroll.TabIndex = 2;
            this._vScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.bar_VerticalScroll);
            // 
            // _hScroll
            // 
            this._hScroll.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._hScroll.Enabled = false;
            this._hScroll.Location = new System.Drawing.Point(0, 301);
            this._hScroll.Name = "_hScroll";
            this._hScroll.Size = new System.Drawing.Size(599, 17);
            this._hScroll.TabIndex = 3;
            this._hScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.bar_HorizontalScroll);
            // 
            // _vScrollToolTip
            // 
            this._vScrollToolTip.AutomaticDelay = 100;
            this._vScrollToolTip.AutoPopDelay = 1000;
            this._vScrollToolTip.InitialDelay = 10;
            this._vScrollToolTip.ReshowDelay = 100;
            // 
            // _DrawPanel
            // 
            this._DrawPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this._DrawPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._DrawPanel.Location = new System.Drawing.Point(0, 30);
            this._DrawPanel.Name = "_DrawPanel";
            this._DrawPanel.Size = new System.Drawing.Size(599, 271);
            this._DrawPanel.TabIndex = 5;
            this._DrawPanel.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DrawPanelMouseWheel);
            this._DrawPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.DrawPanelPaint);
            this._DrawPanel.Resize += new System.EventHandler(this.DrawPanelResize);
            this._DrawPanel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DrawPanelKeyDown);
            // 
            // _ParameterPanel
            // 
            this._ParameterPanel.AutoScroll = true;
            this._ParameterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._ParameterPanel.Location = new System.Drawing.Point(0, 0);
            this._ParameterPanel.Name = "_ParameterPanel";
            this._ParameterPanel.Size = new System.Drawing.Size(599, 30);
            this._ParameterPanel.TabIndex = 4;
            this._ParameterPanel.RunReport += new System.EventHandler(this._ParameterPanel_RunReport);
            // 
            // _FindCtl
            // 
            this._FindCtl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._FindCtl.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._FindCtl.Location = new System.Drawing.Point(0, 318);
            this._FindCtl.Name = "_FindCtl";
            this._FindCtl.Size = new System.Drawing.Size(616, 27);
            this._FindCtl.TabIndex = 0;
            this._FindCtl.Viewer = null;
            this._FindCtl.Visible = false;
            this._FindCtl.Closed += new System.EventHandler(this._FindCtl_Closed);
            // 
            // RdlViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._DrawPanel);
            this.Controls.Add(this._ParameterPanel);
            this.Controls.Add(this._hScroll);
            this.Controls.Add(this._vScroll);
            this.Controls.Add(this._FindCtl);
            this.Name = "RdlViewer";
            this.Size = new System.Drawing.Size(616, 345);
            this.ResumeLayout(false);

        }

        #endregion

        private Reporting.Viewer.ViewerSearchBar _FindCtl;
        private System.Windows.Forms.VScrollBar _vScroll;
        private System.Windows.Forms.HScrollBar _hScroll;
        private ParameterPanel _ParameterPanel;
        private Reporting.Viewer.Canvas.Canvas _DrawPanel;
        private System.Windows.Forms.ToolTip _vScrollToolTip;

    }
}
