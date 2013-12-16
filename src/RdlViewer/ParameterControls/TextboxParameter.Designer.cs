namespace Reporting.Viewer
{
    partial class TextboxParameter
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
            this.answerTextbox = new System.Windows.Forms.TextBox();
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
            // answerTextbox
            // 
            this.answerTextbox.Location = new System.Drawing.Point(103, 0);
            this.answerTextbox.Name = "answerTextbox";
            this.answerTextbox.Size = new System.Drawing.Size(155, 20);
            this.answerTextbox.TabIndex = 2;
            this.answerTextbox.LocationChanged += new System.EventHandler(this.PositionErrorLabel);
            this.answerTextbox.TextChanged += new System.EventHandler(this.answerTextbox_TextChanged);
            this.answerTextbox.Move += new System.EventHandler(this.PositionErrorLabel);
            this.answerTextbox.SizeChanged += new System.EventHandler(this.PositionErrorLabel);
            // 
            // TextboxParameter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.answerTextbox);
            this.Name = "TextboxParameter";
            this.Size = new System.Drawing.Size(318, 24);
            this.Controls.SetChildIndex(this.errorLabel, 0);
            this.Controls.SetChildIndex(this.answerTextbox, 0);
            this.Controls.SetChildIndex(this.queryLabel, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox answerTextbox;
    }
}
