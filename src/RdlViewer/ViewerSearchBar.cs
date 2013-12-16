/*
 * This file is part of UniERM ReportDesigner, based on reportFU by Josh Wilson,
 * the work of Kim Sheffield and the fyiReporting project. 
 * 
 * Prior Copyrights:
 * _________________________________________________________
 * |Copyright (C) 2010 devFU Pty Ltd, Josh Wilson and Others|
 * | (http://reportfu.org)                                  |
 * =========================================================
 * _________________________________________________________
 * |Copyright (C) 2004-2008  fyiReporting Software, LLC     |
 * |For additional information, email info@fyireporting.com |
 * |or visit the website www.fyiReporting.com.              |
 * =========================================================
 *
 * License:
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Text;
using Reporting.Rdl;

using Reporting.Viewer.Enums;

namespace Reporting.Viewer
{
	/// <summary>
	/// ViewerSearchBar finds text inside of the RdlViewer control
	/// </summary>
	public class ViewerSearchBar : System.Windows.Forms.UserControl
    {
        public event EventHandler Opened;
        public event EventHandler Closed;

        private Button bClose;
        private Button bFindNext;
        private Button bFindPrevious;
        private CheckBox ckHighlightAll;
        private CheckBox ckMatchCase;
        private Label lFind;
        private Label lStatus;
        private TextBox tbFind;
        private PageItem position = null;

        private RdlViewer _Viewer;

        /// <summary>
        /// The RdlViewer to search.
        /// </summary>
        public RdlViewer Viewer
        {
            get { return _Viewer; }
            set { _Viewer = value; }
        }

        public ViewerSearchBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.tbFind = new System.Windows.Forms.TextBox();
            this.ckHighlightAll = new System.Windows.Forms.CheckBox();
            this.ckMatchCase = new System.Windows.Forms.CheckBox();
            this.lFind = new System.Windows.Forms.Label();
            this.lStatus = new System.Windows.Forms.Label();
            this.bFindPrevious = new System.Windows.Forms.Button();
            this.bFindNext = new System.Windows.Forms.Button();
            this.bClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbFind
            // 
            this.tbFind.Location = new System.Drawing.Point(53, 4);
            this.tbFind.Name = "tbFind";
            this.tbFind.Size = new System.Drawing.Size(118, 20);
            this.tbFind.TabIndex = 1;
            this.tbFind.TextChanged += new System.EventHandler(this.tbFind_TextChanged);
            // 
            // ckHighlightAll
            // 
            this.ckHighlightAll.Image = global::Reporting.Viewer.Properties.Resources.highlighter_text;
            this.ckHighlightAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ckHighlightAll.Location = new System.Drawing.Point(330, 0);
            this.ckHighlightAll.Name = "ckHighlightAll";
            this.ckHighlightAll.Size = new System.Drawing.Size(99, 26);
            this.ckHighlightAll.TabIndex = 4;
            this.ckHighlightAll.Text = "Highlight All";
            this.ckHighlightAll.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ckHighlightAll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ckHighlightAll.UseVisualStyleBackColor = true;
            this.ckHighlightAll.CheckedChanged += new System.EventHandler(this.ckHighlightAll_CheckedChanged);
            // 
            // ckMatchCase
            // 
            this.ckMatchCase.Image = global::Reporting.Viewer.Properties.Resources.document_attribute;
            this.ckMatchCase.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ckMatchCase.Location = new System.Drawing.Point(435, 0);
            this.ckMatchCase.Name = "ckMatchCase";
            this.ckMatchCase.Size = new System.Drawing.Size(102, 27);
            this.ckMatchCase.TabIndex = 5;
            this.ckMatchCase.Text = "Match Case";
            this.ckMatchCase.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ckMatchCase.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ckMatchCase.UseVisualStyleBackColor = true;
            this.ckMatchCase.CheckedChanged += new System.EventHandler(this.ckMatchCase_CheckedChanged);
            // 
            // lFind
            // 
            this.lFind.AutoSize = true;
            this.lFind.Location = new System.Drawing.Point(20, 7);
            this.lFind.Name = "lFind";
            this.lFind.Size = new System.Drawing.Size(30, 13);
            this.lFind.TabIndex = 6;
            this.lFind.Text = "Find:";
            // 
            // lStatus
            // 
            this.lStatus.AutoSize = true;
            this.lStatus.ForeColor = System.Drawing.Color.Salmon;
            this.lStatus.Location = new System.Drawing.Point(501, 7);
            this.lStatus.Name = "lStatus";
            this.lStatus.Size = new System.Drawing.Size(0, 13);
            this.lStatus.TabIndex = 7;
            // 
            // bFindPrevious
            // 
            this.bFindPrevious.Image = global::Reporting.Viewer.Properties.Resources.arrow_continue_000_top;
            this.bFindPrevious.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bFindPrevious.Location = new System.Drawing.Point(242, 2);
            this.bFindPrevious.Name = "bFindPrevious";
            this.bFindPrevious.Size = new System.Drawing.Size(72, 24);
            this.bFindPrevious.TabIndex = 3;
            this.bFindPrevious.Text = "Previous";
            this.bFindPrevious.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bFindPrevious.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bFindPrevious.UseVisualStyleBackColor = true;
            this.bFindPrevious.Click += new System.EventHandler(this.bFindPrevious_Click);
            // 
            // bFindNext
            // 
            this.bFindNext.Image = global::Reporting.Viewer.Properties.Resources.arrow_continue;
            this.bFindNext.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bFindNext.Location = new System.Drawing.Point(177, 2);
            this.bFindNext.Name = "bFindNext";
            this.bFindNext.Size = new System.Drawing.Size(59, 24);
            this.bFindNext.TabIndex = 2;
            this.bFindNext.Text = "Next";
            this.bFindNext.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bFindNext.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bFindNext.UseVisualStyleBackColor = true;
            this.bFindNext.Click += new System.EventHandler(this.bFindNext_Click);
            // 
            // bClose
            // 
            this.bClose.BackColor = System.Drawing.Color.Transparent;
            this.bClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.bClose.FlatAppearance.BorderSize = 0;
            this.bClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bClose.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bClose.Image = global::Reporting.Viewer.Properties.Resources.cross_button;
            this.bClose.Location = new System.Drawing.Point(2, 4);
            this.bClose.Margin = new System.Windows.Forms.Padding(0);
            this.bClose.Name = "bClose";
            this.bClose.Size = new System.Drawing.Size(18, 18);
            this.bClose.TabIndex = 0;
            this.bClose.UseVisualStyleBackColor = false;
            this.bClose.Click += new System.EventHandler(this.bClose_Click);
            // 
            // ViewerSearchBar
            // 
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.lStatus);
            this.Controls.Add(this.lFind);
            this.Controls.Add(this.ckMatchCase);
            this.Controls.Add(this.ckHighlightAll);
            this.Controls.Add(this.bFindPrevious);
            this.Controls.Add(this.bFindNext);
            this.Controls.Add(this.tbFind);
            this.Controls.Add(this.bClose);
            this.Name = "ViewerSearchBar";
            this.Size = new System.Drawing.Size(740, 27);
            this.VisibleChanged += new System.EventHandler(this.RdlViewerFind_VisibleChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void bClose_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        private void bFindNext_Click(object sender, EventArgs e)
        {
            FindNext();
        }

        /// <summary>
        /// Searches for the next matching item.
        /// </summary>
        public void FindNext()
        {
            Find(ViewerSearchDirection.Next);
        }

        private void bFindPrevious_Click(object sender, EventArgs e)
        {
            FindPrevious();
        }

        /// <summary>
        /// Searches for the previous matching item.
        /// </summary>
        public void FindPrevious()
        {
            Find(ViewerSearchDirection.Previous);
        }

        /// <summary>
        /// Generates the neccessary search options and performs a find in the specified direction.
        /// </summary>
        public void Find(ViewerSearchDirection direction)
        {
            string searchDirection = string.Empty;
            string beginSearchPoint = string.Empty;
            string endSearchPoint = string.Empty;
            ViewerSearchOptions searchOptions = (ckMatchCase.Checked ? ViewerSearchOptions.MatchCase : ViewerSearchOptions.None);

            switch (direction)
            {
	            case ViewerSearchDirection.Previous:
                    searchDirection = "FindPrevious";
                    beginSearchPoint = "top";
                    endSearchPoint = "end";
                    searchOptions = ViewerSearchOptions.Backward | searchOptions;
                    break;
                case ViewerSearchDirection.Next:
                default:
                    searchDirection = "FindNext";
                    beginSearchPoint = "end";
                    endSearchPoint = "top";
                    //searchOptions Search Options already set, no change needed.
                    break;
            }

            Find(searchOptions, searchDirection, beginSearchPoint, endSearchPoint);
        }

        /// <summary>
        /// Performs a find using the specified options and direction from the public Find method.
        /// </summary>
        private void Find(ViewerSearchOptions searchOptions, string searchDirection, string beginSearchPoint, string endSearchPoint)
        {
            if (_Viewer == null)
                throw new ApplicationException(string.Format("Viewer property must be set prior to issuing {0}.", searchDirection));

            if (tbFind.Text.Length == 0)    // must have something to find
                return;

            bool begin = position == null;
            position = _Viewer.Find(tbFind.Text, position, searchOptions);
            if (position == null)
            {
                if (!begin)     // if we didn't start from beginning already; try from bottom
                    position = _Viewer.Find(tbFind.Text, position, searchOptions);

                lStatus.Text = position == null ?
                    "Phrase not found" : string.Format("Reached {0} of report, continued from {1}", beginSearchPoint, endSearchPoint);

                _Viewer.HighlightPageItem = position;
                if (position != null)
                    _Viewer.ScrollToPageItem(position);
            }
            else
            {
                lStatus.Text = "";
                _Viewer.HighlightPageItem = position;
                _Viewer.ScrollToPageItem(position);
            }
        }

        private void RdlViewerFind_VisibleChanged(object sender, EventArgs e)
        {
            if (!DesignMode && this.IsHandleCreated)
            {
                lStatus.Text = "";
                if (this.Visible)
                {
                    if (tbFind.Text != string.Empty)
                    {
                        _Viewer.HighlightText = tbFind.Text;
                        tbFind.Focus();
                        FindNext();         // and go find the contents of the textbox
                    }
                }
                else
                {   // turn off any highlighting when find control not visible
                    _Viewer.HighlightPageItem = position = null;
                    _Viewer.HighlightText = null;
                    _Viewer.HighlightAll = false;
                    ckHighlightAll.Checked = false;
                }
            }
        }

        private void tbFind_TextChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                lStatus.Text = "";
                position = null;        // reset position when edit changes?? todo not really
                _Viewer.HighlightText = tbFind.Text;
                ckHighlightAll.Enabled = bFindNext.Enabled = bFindPrevious.Enabled =
                        tbFind.Text.Length > 0;
                if (tbFind.Text.Length > 0)
                    FindNext();
            }
        }

        private void ckHighlightAll_CheckedChanged(object sender, EventArgs e)
        {
            _Viewer.HighlightAll = ckHighlightAll.Checked;
        }

        private void ckMatchCase_CheckedChanged(object sender, EventArgs e)
        {
            _Viewer.HighlightCaseSensitive = ckMatchCase.Checked;
        }

        private void OnOpened(EventArgs e)
        {
            if (Opened != null)
                Opened(this, e);
        }

        private void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }
    }
}
