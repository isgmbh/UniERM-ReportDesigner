namespace Reporting.Viewer.Dialogs
{
    partial class DialogWait
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.progressIndicator1 = new ProgressControls.ProgressIndicator();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 132);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(192, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Please Wait While Report is Rendered!";
            this.label1.UseWaitCursor = true;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // progressIndicator1
            // 
            this.progressIndicator1.AnimationSpeed = 85;
            this.progressIndicator1.AutoStart = true;
            this.progressIndicator1.BackColor = System.Drawing.Color.Transparent;
            this.progressIndicator1.CircleColor = System.Drawing.Color.Green;
            this.progressIndicator1.CircleSize = 0.5F;
            this.progressIndicator1.Location = new System.Drawing.Point(40, 0);
            this.progressIndicator1.Name = "progressIndicator1";
            this.progressIndicator1.NumberOfCircles = 23;
            this.progressIndicator1.NumberOfVisibleCircles = 20;
            this.progressIndicator1.Percentage = 0F;
            this.progressIndicator1.ShowText = true;
            this.progressIndicator1.Size = new System.Drawing.Size(129, 129);
            this.progressIndicator1.TabIndex = 4;
            this.progressIndicator1.Text = "Time Elapsed:";
            this.progressIndicator1.TextDisplay = ProgressControls.TextDisplayModes.Text;
            // 
            // DialogWait
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(216, 154);
            this.ControlBox = false;
            this.Controls.Add(this.progressIndicator1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DialogWait";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Rendering Report";
            this.TopMost = true;
            this.UseWaitCursor = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;
        private ProgressControls.ProgressIndicator progressIndicator1;
    }
}