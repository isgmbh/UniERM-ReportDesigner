namespace Reporting.Viewer
{
    partial class ParameterBase
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
            this.queryLabel = new System.Windows.Forms.Label();
            this.tip = new System.Windows.Forms.ToolTip(this.components);
            this.errorLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // queryLabel
            // 
            this.queryLabel.AutoSize = true;
            this.queryLabel.Location = new System.Drawing.Point(0, 0);
            this.queryLabel.Name = "queryLabel";
            this.queryLabel.Size = new System.Drawing.Size(97, 13);
            this.queryLabel.TabIndex = 0;
            this.queryLabel.Text = "Sample text query?";
            // 
            // tip
            // 
            this.tip.IsBalloon = true;
            this.tip.ToolTipTitle = "Parameter Error";
            // 
            // errorLabel
            // 
            this.errorLabel.Image = global::Reporting.Viewer.Properties.Resources.cross;
            this.errorLabel.Location = new System.Drawing.Point(103, 0);
            this.errorLabel.Name = "errorLabel";
            this.errorLabel.Size = new System.Drawing.Size(20, 22);
            this.errorLabel.TabIndex = 1;
            this.errorLabel.Visible = false;
            // 
            // ParameterBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.queryLabel);
            this.Controls.Add(this.errorLabel);
            this.Name = "ParameterBase";
            this.Size = new System.Drawing.Size(232, 22);
            this.ParentChanged += new System.EventHandler(this.ParameterBase_ParentChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.Label queryLabel;
        private System.Windows.Forms.ToolTip tip;
        protected System.Windows.Forms.Label errorLabel;


    }
}
