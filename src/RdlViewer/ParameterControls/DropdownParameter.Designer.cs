namespace Reporting.Viewer
{
    partial class DropdownParameter
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.answerDropdown = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // queryLabel
            // 
            this.queryLabel.LocationChanged += new System.EventHandler(this.PositionAnswerField);
            this.queryLabel.Resize += new System.EventHandler(this.PositionAnswerField);
            // 
            // errorLabel
            // 
            this.errorLabel.Location = new System.Drawing.Point(264, 0);
            // 
            // answerDropdown
            // 
            this.answerDropdown.FormattingEnabled = true;
            this.answerDropdown.Location = new System.Drawing.Point(103, 0);
            this.answerDropdown.Name = "answerDropdown";
            this.answerDropdown.Size = new System.Drawing.Size(155, 21);
            this.answerDropdown.TabIndex = 2;
            // 
            // DropdownParameter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.answerDropdown);
            this.Name = "DropdownParameter";
            this.Size = new System.Drawing.Size(318, 24);
            this.Controls.SetChildIndex(this.answerDropdown, 0);
            this.Controls.SetChildIndex(this.errorLabel, 0);
            this.Controls.SetChildIndex(this.queryLabel, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox answerDropdown;

    }
}
