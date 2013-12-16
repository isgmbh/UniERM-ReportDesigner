using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Reporting.Viewer.Dialogs
{
    public partial class DialogWait : Form
    {
        private DateTime Started;
        private RdlViewer _viewer;

        public DialogWait(RdlViewer viewer)
        {
            InitializeComponent();
            _viewer = viewer;
            Started = DateTime.Now;
            timer1.Interval = 1000;
            timer1_Tick(null,null);
            timer1.Start();
        }

        //Josh: 7/2/10 Changed from a standard progress bar to a 
        // circular progress bar obtained from Code Project.
        // See Changelog.txt, entry 1 for more details.
        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan time = DateTime.Now - Started;
            progressIndicator1.Text = 
                string.Format("Time Elapsed:{0}{1}", 
                                Environment.NewLine,
                                (((time.Days * 24 + time.Hours) * 60) + 
                                time.Minutes) + " Minutes " + 
                                time.Seconds + " Seconds");
            //lblStatus.Text = _viewer.ReportStatus();
            Application.DoEvents();
        }

        /// <summary>
        /// A method added for future possible use when updating the current activity on the dialog.
        /// </summary>
        public void UpdateActivityDisplay(string value)
        {

        }
    }
}