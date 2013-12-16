/*
 * This file is part of UniERM ReportDesigner, based on reportFU by Josh Wilson,
 * the work of Kim Sheffield and the fyiReporting project. 
 * 
 * © 2012-2013 Inpro-Soft GmbH (http://www.unierm.de)
 * 
 * Prior Copyrights:
 * _________________________________________________________
 * |Copyright (C) 2010 devFU Pty Ltd, Josh Wilson and Others|
 * | (http://reportfu.org)                                  |
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
using System.Collections.Generic;
using Reporting.Viewer;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using Reporting.Rdl;
using Reporting.Viewer.Enums;
using System.Xml;
using System.Drawing.Printing;
using System.Collections.Specialized;
using Reporting.Viewer.Dialogs;

namespace Reporting.RdlDesign
{
    public partial class RdlDesignerForm : Form, IPasswordProvider//, IMessageFilter
    {
    	#region Variables
        private MDIChild printChild = null;
        List<Uri> _CurrentFiles = null;		// temporary variable for current files // Josh: Changed string to Uri
        List<string> _Toolbar = null;			// temporary variable for toolbar entries
        List<Uri> _TempReportFiles = null;		// temporary files created for report browsing // Josh: Changed string to Uri
        Process _ServerProcess = null;		// process for the RdlDesktop.exe --
        private Rdl.NeedPassword _GetPassword;

        private string _DataSourceReferencePassword = null;
        private bool bGotPassword = false;
        private bool bMono = DesignerUtility.IsMono();
        private readonly string DefaultHelpUrl = "http://www.unierm.de";
        private readonly string DefaultSupportUrl = "http://www.unierm.de";
        private string _SupportUrl;
        private DialogValidateRdl _ValidateRdl = null;
        private readonly string TEMPRDL = "_tempfile_.rdl";
        private int TEMPRDL_INC = 0;

        private ToolStripButton ctlInsertCurrent = null;
        //private TextBox ctlEditTextbox = null;
        //private Label ctlEditLabel = null;

        private Splitter mainSplitter;

        bool bSuppressChange = false;

        ToolStripPanel toolStripPanel;
        #endregion
        
		#region Constructor, set up, tear down and initialization
        public RdlDesignerForm()
        {
            InitializeComponent();
            InitToolbars();
            LoadStartupState();
        }

        public void InitToolbars()
        {
            //Initialize the toolstrip panel
            toolStripPanel = new ToolStripPanel();
            toolStripPanel.Dock = DockStyle.Top;

            //Initialize the splitter
            mainSplitter = new Splitter();
            mainSplitter.Parent = this;
            mainSplitter.Dock = DockStyle.Right;
            mainProperties.Parent = this;

            //Add toolbars to panel
            toolStripPanel.Join(tsZoom);
            toolStripPanel.Join(tsFont);
            toolStripPanel.Join(tsInsert);
            toolStripPanel.Join(tsStandard);

            //Add panel to the form
            this.Controls.Remove(mainProperties);
            this.Controls.Add(mainSplitter);
            this.Controls.Add(mainProperties);
            this.Controls.Add(toolStripPanel);            

            //Remove and add the menu, so it appears at the top
            this.Controls.Remove(msMain);
            this.Controls.Add(msMain);

            //Populate the Font combo boxes with Font Names, Sizes
            foreach (FontFamily ff in FontFamily.Families)
            {
                cmbFont.Items.Add(ff.Name);
            }
            string[] sizes = new string[] { "8", "9", "10", "11", "12", "14", "16", "18", "20", "22", "24", "26", "28", "36", "48", "72" };
            cmbFontSize.Items.AddRange(sizes);

            //Populate Zoom combo box with zoom levels
            cmbZoomLevel.Items.AddRange(StaticLists.ZoomList);

            if (Settings.Default.RecentFiles == null)
            {
                Settings.Default.RecentFiles = new StringCollection();
            }
            
            RecentFilesMenu();
        }
        
        private void LoadStartupState()
        {           
            //Load toolbar and form positions
            this.Size = Toolbars.Default.FormSize;
            this.Location = Toolbars.Default.FormPos;
            	
            tsZoom.Location = Toolbars.Default.ZoomPos;
            tsFont.Location = Toolbars.Default.FontPos;
            tsInsert.Location = Toolbars.Default.InsertPos;
            tsStandard.Location = Toolbars.Default.StandardPos;
            
            
            //Reload any previously open files
            LoadCurrentFiles();
        }

        private void SaveStartupState()
        {
            Toolbars.Default.StandardPos = tsStandard.Location;
            Toolbars.Default.InsertPos = tsInsert.Location;
            Toolbars.Default.FontPos = tsFont.Location;
            Toolbars.Default.ZoomPos = tsZoom.Location;
            Toolbars.Default.FormPos = this.Location;
            Toolbars.Default.FormSize = this.Size;
            

            //Initializes collection if null and saves any open file paths
            SaveCurrentFiles();
            
            Toolbars.Default.Save();
            Settings.Default.Save();
        }
        
		#endregion

		#region Properties
		
		static internal string[] MapSubtypes
        {
            get
            {
                if (Settings.Default.MapSubtypes == null)
	            {
	        		Settings.Default.MapSubtypes = new StringCollection(){"usa_map"};
	            }
                string[] result = new String[Settings.Default.MapSubtypes.Count];
                Settings.Default.MapSubtypes.CopyTo(result, 0);
                return result;
            }
            set
            {
            	Settings.Default.MapSubtypes = new StringCollection();
            	Settings.Default.MapSubtypes.AddRange(value);
            }
        }
		
		static internal int[] CustomColors
		{
			get 
			{
				int white = 16777215;	// default to white (magic number)
        	
        		if (Settings.Default.CustomColors == null)
            	{
        			Settings.Default.CustomColors = new List<int>(16){white, white, white, white, white, white, white, white,
								    								  white, white, white, white, white, white, white, white};
            	}
        	
				return Settings.Default.CustomColors.ToArray();
			}
			set { Settings.Default.CustomColors = new List<int>(value); }
			
		}
		
		#endregion

		#region Methods
		
		#region Private
		
		private MDIChild CreateMDIChild(Uri file, string rdl, bool bMenuUpdate)
        {
            MDIChild mcOpen = null;
            if (file != null)
            {
                //file = file.Trim();

                foreach (MDIChild mc in this.MdiChildren)
                {
                    if (mc.SourceFile != null && file.LocalPath == mc.SourceFile.LocalPath)
                    {							// we found it
                        mcOpen = mc;
                        break;
                    }
                }
            }
            if (mcOpen == null)
            {
                MDIChild mc = null;
                try
                {
                    mc = new MDIChild(this.ClientRectangle.Width * 3 / 5, this.ClientRectangle.Height * 3 / 5);
                    mc.OnSelectionChanged += new MDIChild.RdlChangeHandler(SelectionChanged);
                    mc.OnSelectionMoved += new MDIChild.RdlChangeHandler(SelectionMoved);
                    mc.OnReportItemInserted += new MDIChild.RdlChangeHandler(ReportItemInserted);
                    mc.OnDesignTabChanged += new MDIChild.RdlChangeHandler(DesignTabChanged);
                    mc.OnOpenSubreport += new DesignCtl.OpenSubreportEventHandler(OpenSubReportEvent);
                    mc.OnHeightChanged += new DesignCtl.HeightEventHandler(HeightChanged);
                    mc.Icon = Properties.Resources.unierm16;

                    mc.MdiParent = this;
                    if (Settings.Default.ShowTabbedInterface)
                        mc.WindowState = FormWindowState.Maximized;
                    mc.Viewer.GetDataSourceReferencePassword = _GetPassword;
                    if (file != null)
                    {
                        mc.Viewer.Folder = Path.GetDirectoryName(file.LocalPath);
                        mc.SourceFile = file;
                        mc.Text = Path.GetFileName(file.LocalPath);
                        mc.Viewer.Folder = Path.GetDirectoryName(file.LocalPath);
                        mc.Viewer.ReportName = Path.GetFileNameWithoutExtension(file.LocalPath);
                        NoteRecentFiles(file, bMenuUpdate);
                    }
                    else
                    {
                        mc.SourceRdl = rdl;
                        mc.Viewer.ReportName = mc.Text = "Untitled";
                    }
                    mc.ShowEditLines(Settings.Default.ShowEditLines);
                    mc.ShowReportItemOutline = Settings.Default.ShowReportItemOutline;
                    mc.ShowPreviewWaitDialog(Settings.Default.ShowPreviewWaitDialog);
                    // add to toolbar tab
                    TabPage tp = new TabPage();
                    tp.Location = new System.Drawing.Point(0, 0);
                    tp.Name = mc.Text;
                    tp.TabIndex = 1;
                    tp.Text = mc.Text;
                    tp.Tag = mc;                // tie the mdichild to the tabpage
                    tp.ToolTipText = file == null ? "" : file.LocalPath;
                    tcMain.Controls.Add(tp);
                    mc.Tab = tp;

                    mc.Show();
                    mcOpen = mc;

                    tcMain.Visible = true;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    if (mc != null)
                        mc.Close();
                    return null;
                }
            }
            else
            {
                mcOpen.Activate();
            }
            return mcOpen;
        }

        private void ReportItemInserted(object sender, System.EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            // turn off the current selection after an item is inserted
            if (ctlInsertCurrent != null)
            {
                ctlInsertCurrent.Checked = false;
                mc.CurrentInsert = null;
                ctlInsertCurrent = null;
            }
        }

        private void OpenSubReportEvent(object sender, SubReportEventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            Uri file = new Uri(mc.Viewer.Folder);
            if (e.SubReportName[0] == Path.DirectorySeparatorChar)
                file = new Uri(file.LocalPath + e.SubReportName);
            else
                file = new Uri(file.LocalPath + Path.DirectorySeparatorChar + e.SubReportName + ".rdl");

            CreateMDIChild(file, null, true);
        }

        private void HeightChanged(object sender, HeightEventArgs e)
        {
            if (e.Height == null)
            {
                SetProperties(this.ActiveMdiChild as MDIChild);

                statusPosition.Text = "";
                return;
            }

            RegionInfo rinfo = new RegionInfo(CultureInfo.CurrentCulture.LCID);
            float h = DesignXmlDraw.GetSize(e.Height);
            string sh;
            if (rinfo.IsMetric)
            {
                sh = string.Format("   height={0:0.00}cm        ",
                        h / (DesignXmlDraw.POINTSIZED / 2.54d));
            }
            else
            {
                sh = string.Format("   height={0:0.00}\"        ",
                        h / DesignXmlDraw.POINTSIZED);
            }
            statusPosition.Text = sh;
        }

        private void SelectionMoved(object sender, System.EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            SetStatusNameAndPosition();
        }
        
        private void SelectionChanged(object sender, System.EventArgs e)  
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;
            // handle edit tab first
            if (mc.RdlEditor.DesignTab == "edit")
            {
                SetStatusNameAndPosition();
                return;
            }

            bSuppressChange = true;			// don't process changes in status bar

            SetStatusNameAndPosition();
            this.EnableEditTextBox();	// handling enabling/disabling of textbox

            StyleInfo si = mc.SelectedStyle;
            if (si == null)
                return;

                btnCAlign.Checked = si.TextAlign == TextAlignEnum.Center ? true : false;
                btnLAlign.Checked = si.TextAlign == TextAlignEnum.Left ? true : false;
                btnRAlign.Checked = si.TextAlign == TextAlignEnum.Right ? true : false;
                btnBold.Checked = si.IsFontBold() ? true : false;
                btnItalic.Checked = si.FontStyle == FontStyleEnum.Italic ? true : false;
                btnUnderline.Checked = si.TextDecoration == TextDecorationEnum.Underline ? true : false;
                cmbFont.Text = si.FontFamily;

                string rs = string.Format(NumberFormatInfo.InvariantInfo, "{0:0.#}", si.FontSize);
                cmbFontSize.Text = rs;

                //btnApplyForeColour.Text = si.Color.IsEmpty ? si.ColorText : ColorTranslator.ToHtml(si.Color);

                //btnApplyBackColour.Text = si.BackgroundColor.IsEmpty ? si.BackgroundColorText : ColorTranslator.ToHtml(si.BackgroundColor);

            bSuppressChange = false;
        }

        //FS Tabindex wechsel nun möglich
        private void tcMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            MDIChild mcOpen = null;
            TabControl TBC = sender as TabControl;
            if (TBC.SelectedIndex > -1)
            {
                mcOpen = (MDIChild)this.MdiChildren[TBC.SelectedIndex];
                mcOpen.Activate();
            }
        }

        private void SetProperties(MDIChild mc)
        {
            if (mc == null || !Settings.Default.ShowProperties || mc.DesignTab != "design")
                mainProperties.ResetSelection(null, null);
            else
                mainProperties.ResetSelection(mc.RdlEditor.DrawCtl, mc.RdlEditor.DesignCtl);
        }

        private void NoteRecentFiles(Uri name, bool bResetMenu)
        {
            if (name == null)
                return;

            //name = name.Trim();
            if (Settings.Default.RecentFiles.Contains(name.LocalPath))
            {	// need to move it to top of list; so remove old one
                int loc = Settings.Default.RecentFiles.IndexOf(name.LocalPath);
                Settings.Default.RecentFiles.RemoveAt(loc);
            }
            if (Settings.Default.RecentFiles.Count >= Settings.Default.RecentFilesMax)
            {
                Settings.Default.RecentFiles.RemoveAt(0);	// remove the first entry
            }
            Settings.Default.RecentFiles.Add(name.LocalPath);
            if (bResetMenu)
                RecentFilesMenu();
            return;
        }

        private void DesignTabChanged(object sender, System.EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            string tab = "";
            if (mc != null)
                tab = mc.DesignTab;
            bool bEnableEdit = false;
            bool bEnableDesign = false;
            bool bEnablePreview = false;
            bool bShowProp = Settings.Default.ShowProperties;
            switch (tab)
            {
                case "edit":
                    bEnableEdit = true;
                    if (Settings.Default.PropertiesAutoHide)
                        bShowProp = false;
                    break;
                case "design":
                    bEnableDesign = true;
                    break;
                case "preview":
                    if (Settings.Default.PropertiesAutoHide)
                        bShowProp = false;
                    bEnablePreview = true;
                    break;
            }
            if (!bEnableEdit && this._ValidateRdl != null)
            {
                this._ValidateRdl.Close();
            }
            mainProperties.Visible = mainSplitter.Visible = bShowProp;
            btnLAlign.Enabled = bEnableDesign;
            btnCAlign.Enabled = bEnableDesign;
            btnRAlign.Enabled = bEnableDesign;
            btnBold.Enabled = bEnableDesign;
            btnItalic.Enabled = bEnableDesign;
            btnUnderline.Enabled = bEnableDesign;
            cmbFont.Enabled = bEnableDesign;
            cmbFontSize.Enabled = bEnableDesign;
            pickerForeColour.Enabled = bEnableDesign;
            pickerBackColour.Enabled = bEnableDesign;
            btnCut.Enabled = bEnableDesign | bEnableEdit;
            btnCopy.Enabled = bEnableDesign | bEnableEdit | bEnablePreview;
            btnUndo.Enabled = bEnableDesign | bEnableEdit;
            btnPaste.Enabled = bEnableDesign | bEnableEdit;
            btnPrint.Enabled = bEnablePreview;

            btnInsertTextbox.Enabled = bEnableDesign;

            btnSelect.Enabled = bEnablePreview;
            btnSelect.Checked = mc == null ? false : mc.SelectionTool;

            btnInsertChart.Enabled = bEnableDesign;
            btnInsertRectangle.Enabled = bEnableDesign;
            btnInsertTable.Enabled = bEnableDesign;
            btnInsertMatrix.Enabled = bEnableDesign;
            btnInsertList.Enabled = bEnableDesign;
            btnInsertLine.Enabled = bEnableDesign;
            btnInsertImage.Enabled = bEnableDesign;
            btnInsertSubreport.Enabled = bEnableDesign;
            btnExport.Enabled = bEnablePreview;

            this.EnableEditTextBox();

            cmbZoomLevel.Enabled = bEnablePreview;
            btnZoomIn.Enabled = bEnablePreview;
            btnZoomOut.Enabled = bEnablePreview;

            string zText = "Actual Size";
            if (mc != null)
            {
                switch (mc.ZoomMode)
                {
                    case ZoomMode.FitWidth:
                        zText = "Fit Width";
                        break;
                    case ZoomMode.FitPage:
                        zText = "Fit Page";
                        break;
                    case ZoomMode.UseZoom:
                        if (mc.Zoom == 1)
                            zText = "Actual Size";
                        else
                            zText = string.Format("{0:0}", mc.Zoom * 100f);
                        break;
                }
                cmbZoomLevel.Text = zText;

                // when no active sheet
                btnSave.Enabled = mc != null;

                // Update the status and position information
                SetStatusNameAndPosition();
            }
        }

        private void SetStatusNameAndPosition()
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;

            SetProperties(mc);

            if (mc == null)
            {
                statusPosition.Text = statusSelected.Text = "";
            }
            else if (mc.DesignTab == "design")
                SetStatusNameAndPositionDesign(mc);
            else if (mc.DesignTab == "edit")
                SetStatusNameAndPositionEdit(mc);
            else
            {
                statusPosition.Text = statusSelected.Text = "";
            }
            return;
        }

        private void EnableEditTextBox()
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            bool bEnable = false;

            if (this.txtExpression == null || mc == null ||
                mc.DesignTab != "design" || mc.DrawCtl.SelectedCount != 1)
            { }
            else
            {
                XmlNode tn = mc.DrawCtl.SelectedList[0] as XmlNode;
                if (tn != null && tn.Name == "Textbox")
                {
                    txtExpression.Text = mc.DrawCtl.GetElementValue(tn, "Value", "");
                    bEnable = true;
                }
            }
            if (txtExpression != null)
            {
                if (!bEnable)
                    txtExpression.Text = "";
                txtExpression.Enabled = bEnable;
                btnExpression.Enabled = bEnable;
            }
        }
        
       	private void SetStatusNameAndPositionDesign(MDIChild mc)
        {
            if (mc.DrawCtl.SelectedCount <= 0)
            {
                statusPosition.Text = statusSelected.Text = "";
                return;
            }

            // Handle position
            PointF pos = mc.SelectionPosition;
            SizeF sz = mc.SelectionSize;
            string spos;
            if (pos.X == float.MinValue)	// no item selected is probable cause
                spos = "";
            else
            {
                RegionInfo rinfo = new RegionInfo(CultureInfo.CurrentCulture.LCID);
                double m72 = DesignXmlDraw.POINTSIZED;
                if (rinfo.IsMetric)
                {
                    if (sz.Width == float.MinValue)	// item is in a table/matrix is probably cause
                        spos = string.Format("   x={0:0.00}cm, y={1:0.00}cm        ",
                            pos.X / (m72 / 2.54d), pos.Y / (m72 / 2.54d));
                    else
                        spos = string.Format("   x={0:0.00}cm, y={1:0.00}cm, w={2:0.00}cm, h={3:0.00}cm        ",
                            pos.X / (m72 / 2.54d), pos.Y / (m72 / 2.54d),
                            sz.Width / (m72 / 2.54d), sz.Height / (m72 / 2.54d));
                }
                else
                {
                    if (sz.Width == float.MinValue)
                        spos = string.Format("   x={0:0.00}\", y={1:0.00}\"        ",
                            pos.X / m72, pos.Y / m72);
                    else
                        spos = string.Format("   x={0:0.00}\", y={1:0.00}\", w={2:0.00}\", h={3:0.00}\"        ",
                            pos.X / m72, pos.Y / m72, sz.Width / m72, sz.Height / m72);
                }
            }
            if (spos != statusPosition.Text)
                statusPosition.Text = spos;

            // Handle text
            string sname = mc.SelectionName;
            if (sname != statusSelected.Text)
                statusSelected.Text = sname;
            return;
        }

        private void SetStatusNameAndPositionEdit(MDIChild mc)
        {
            string spos = string.Format("Ln {0}  Ch {1}", mc.CurrentLine, mc.CurrentCh);
            if (spos != statusSelected.Text)
                statusSelected.Text = spos;

            if (statusPosition.Text != "")
                statusPosition.Text = "";

            return;
        }
        
        private void SetMDIChildFocus(MDIChild mc)
        {
            // We don't want to be triggering any change events when the focus is changing
            bool bSuppress = bSuppressChange;
            bSuppressChange = true;
            mc.SetFocus();
            bSuppressChange = bSuppress;
        } 
         
        private void CleanupTempFiles()
        {
            if (_TempReportFiles == null)
                return;
            foreach (Uri file in _TempReportFiles)
            {
                try
                {	// It's ok for the delete to fail
                    File.Delete(file.LocalPath);
                }
                catch
                { }
            }
            _TempReportFiles = null;
        }

        private void LoadCurrentFiles()
        {
        	if (Settings.Default.CurrentFiles == null)
            {
                Settings.Default.CurrentFiles = new StringCollection();
            }
        	
        	string[] args = Environment.GetCommandLineArgs();
			for (int i=1; i < args.Length; i++)
			{
				string larg = args[i].ToLower();
				if (larg == "/m" || larg == "-m")
				{
					continue;
				}

				if (File.Exists(args[i]))			// only add it if it exists
				{
					//Place at the top of the list
					Settings.Default.CurrentFiles.Insert(0, new Uri(args[i]).LocalPath);
				}
			}
				
        	if (Settings.Default.CurrentFiles.Count > 0) 
        	{
				foreach (string file in Settings.Default.CurrentFiles)
				{
					CreateMDIChild(new Uri(file), null, false);
				}
				Settings.Default.CurrentFiles = null;		// don't need this any longer
			}
        }
        
        private void SaveCurrentFiles()
        {
        	Settings.Default.CurrentFiles = new StringCollection();
        	
        	foreach (MDIChild child in this.MdiChildren) 
        	{
        		Settings.Default.CurrentFiles.Add(child.SourceFile.LocalPath);
        	}
                    	
        }
        
		#endregion
		
		#region Public
		
		public string GetPassword()
        {
            if (_DataSourceReferencePassword != null)
                return _DataSourceReferencePassword;

            using (DataSourcePassword dlg = new DataSourcePassword())
            {
                DialogResult rc = dlg.ShowDialog();
                if (rc == DialogResult.OK)
                    _DataSourceReferencePassword = dlg.PassPhrase;

                return _DataSourceReferencePassword;
            }
        }

        public void ResetPassword()
        {
            _DataSourceReferencePassword = null;
        }
        
		#endregion
		
		#region Internal
		
		internal void RecentFilesMenu()
        {
            menuFileRecentFiles.DropDownItems.Clear();
            int mi = 1;
            for (int i = Settings.Default.RecentFiles.Count - 1; i >= 0; i--)
            {
                // add Item to menu and increment mi after
                string menuText = string.Format("&{0} {1}", mi++, Settings.Default.RecentFiles[i]);
                ToolStripMenuItem m = new ToolStripMenuItem(menuText);
                m.Click += new EventHandler(this.menuRecentItem_Click);
                menuFileRecentFiles.DropDownItems.Add(m);
            }
        }

     	internal RdlEditPreview GetEditor()
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return null;
            return mc.Editor;
        }              
        
        internal void ShowProperties(bool bShow)
        {
            Settings.Default.ShowProperties = bShow;
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || !Settings.Default.ShowProperties || mc.DesignTab != "design")
                mainProperties.ResetSelection(null, null);
            else
                mainProperties.ResetSelection(mc.RdlEditor.DrawCtl, mc.RdlEditor.DesignCtl);

            if (mc != null && !Settings.Default.ShowProperties)
                mc.SetFocus();
            mainProperties.Visible = mainSplitter.Visible = Settings.Default.ShowProperties;
            //menuViewPropertiesWindow.Checked = _ShowProperties;
        }
       
        internal void menuToolsStartProcess(bool bMsg)
        {
            if (_ServerProcess != null && !_ServerProcess.HasExited)
                return;

            string pswd = GetPassword();

            try
            {
                string filename = string.Format("{0}{1}",
                    AppDomain.CurrentDomain.BaseDirectory, "RdlDesktop.exe");

                ProcessStartInfo psi = new ProcessStartInfo(filename);
                if (pswd != null)
                    psi.Arguments = "/p" + pswd;
                psi.RedirectStandardError = psi.RedirectStandardInput = psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                //psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
                _ServerProcess = Process.Start(psi);
            }
            catch (Exception ex)
            {
                if (bMsg)
                    MessageBox.Show(ex.Message, "Unable to start Desktop");
            }

            return;
        }

        internal void menuToolsCloseProcess(bool bMsg)
        {
            if (_ServerProcess == null)
                return;
            if (!_ServerProcess.HasExited)
            {
                try
                {
                    _ServerProcess.StandardInput.WriteLine("x");	// x stops the server
                }
                catch (Exception ex)
                {
                    if (bMsg)
                        MessageBox.Show(ex.Message, "Error stopping process");
                }
            }
            _ServerProcess = null;
        }

		#endregion
		
		#endregion
		
      
       

		#region Form Events
        private void menuRecentItem_Click(object sender, System.EventArgs e)
        {
            ToolStripMenuItem m = (ToolStripMenuItem)sender;
            int si = m.Text.IndexOf(" ");
            Uri file = new Uri(m.Text.Substring(si + 1));

            CreateMDIChild(file, null, true);
        }

        private void menuFileNewReport_Click(object sender, System.EventArgs e)
        {
            using (DialogDatabase dlgDB = new DialogDatabase(this))
            {
                dlgDB.StartPosition = FormStartPosition.CenterParent;
                dlgDB.FormBorderStyle = FormBorderStyle.SizableToolWindow;

                // show modally
                dlgDB.ShowDialog();
                if (dlgDB.DialogResult == DialogResult.Cancel)
                    return;
                string rdl = dlgDB.ResultReport;

                // Create the MDI child using the RDL syntax the wizard generates
                MDIChild mc = CreateMDIChild(null, rdl, false);
                mc.Modified = true;
                // Force building of report names for new reports
                if (mc.DrawCtl.ReportNames == null) { }
            }
        }

        private void menuFileOpen_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            OpenFileDialog ofd = new OpenFileDialog();
            if (mc != null)
            {
                try
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(mc.SourceFile.LocalPath);
                }
                catch
                {
                }
            }
            ofd.DefaultExt = "rdl";
            ofd.Filter = "Report files (*.rdl;*rdlc)|*.rdl;*.rdlc|" +
                "All files (*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.CheckFileExists = true;
            ofd.Multiselect = true;
            try
            {
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    foreach (string file in ofd.FileNames)
                    {
                        CreateMDIChild(new Uri(file), null, false);
                    }
                    RecentFilesMenu();		// update the menu for recent files
                }
            }
            finally
            {
                ofd.Dispose();
            }
        }

        private void menuFileSave_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            if (!mc.FileSave())
                return;

            NoteRecentFiles(mc.SourceFile, true);

            if (mc.Editor != null)
                mc.Editor.ClearUndo();

            return;
        }

        private void menuFileSaveAs_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            if (!mc.FileSaveAs())
                return;

            mc.Viewer.Folder = Path.GetDirectoryName(mc.SourceFile.LocalPath);
            mc.Viewer.ReportName = Path.GetFileNameWithoutExtension(mc.SourceFile.LocalPath);
            mc.Text = Path.GetFileName(mc.SourceFile.LocalPath);

            NoteRecentFiles(mc.SourceFile, true);

            if (mc.Editor != null)
                mc.Editor.ClearUndo();

            return;
        }

        private void menuFilePrint_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;
            if (mc.SourceFile == null)
            {
                MessageBox.Show("The report must be saved prior to printing.", "RDL Design");
                return;
            }
            if (printChild != null)			// already printing
            {
                MessageBox.Show("Can only print one file at a time.", "RDL Design");
                return;
            }

            printChild = mc;

            PrintDocument pd = new PrintDocument();

            pd.DocumentName = mc.SourceFile.LocalPath;
            pd.PrinterSettings.FromPage = 1;
            pd.PrinterSettings.ToPage = mc.PageCount;
            pd.PrinterSettings.MaximumPage = mc.PageCount;
            pd.PrinterSettings.MinimumPage = 1;
            pd.DefaultPageSettings.Landscape = mc.PageWidth > mc.PageHeight ? true : false;

            using (PrintDialog dlg = new PrintDialog())
            {
                dlg.Document = pd;
                dlg.AllowSelection = true;
                dlg.AllowSomePages = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (pd.PrinterSettings.PrintRange == PrintRange.Selection)
                        {
                            pd.PrinterSettings.FromPage = mc.PageCurrent;
                        }
                        mc.Print(pd);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Print error: " + ex.Message, "RDL Design");
                    }
                }
                printChild = null;
            }
        }

        private void menuEditUndo_Click(object sender, System.EventArgs ea)
        {
            if (this.txtExpression != null && txtExpression.Focused)
            {
                txtExpression.Undo();
                return;
            }

            RdlEditPreview e = GetEditor();
            if (e == null)
                return;

            if (e.CanUndo == true)
            {
                e.Undo();

                MDIChild mc = this.ActiveMdiChild as MDIChild;
                if (mc != null && mc.DesignTab == "design")
                {
                    e.DesignCtl.SetScrollControls();
                }
                this.SelectionChanged(this, new EventArgs());
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            //This tool doesn't seem to do anything except give mc focus
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            SetMDIChildFocus(mc);
        }

        private void menuEditCut_Click(object sender, System.EventArgs ea)
        {
            if (this.txtExpression != null && txtExpression.Focused)
            {
                txtExpression.Cut();
                return;
            }

            RdlEditPreview e = GetEditor();
            if (e == null)
                return;

            if (e.SelectionLength > 0)
                e.Cut();
        }

        private void menuEditCopy_Click(object sender, System.EventArgs ea)
        {
            if (this.txtExpression != null && txtExpression.Focused)
            {
                txtExpression.Copy();
                return;
            }
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            RdlEditPreview e = mc.RdlEditor;
            if (e == null)
                return;

            if (e.SelectionLength > 0)
                e.Copy();
        }

        private void menuEditPaste_Click(object sender, System.EventArgs ea)
        {
            if (this.txtExpression != null && txtExpression.Focused)
            {
                txtExpression.Paste();
                return;
            }

            RdlEditPreview e = GetEditor();
            if (e == null)
                return;

            if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text) == true ||
                Clipboard.GetDataObject().GetDataPresent(DataFormats.Bitmap) == true)
                e.Paste();
        }

        private void menuEditDelete_Click(object sender, System.EventArgs ea)
        {

            RdlEditPreview e = GetEditor();
            if (e == null)
                return;

            if (e.SelectionLength > 0)
                e.SelectedText = "";
        }

        private void menuViewProperties_Click(object sender, System.EventArgs ea)
        {
            //RdlEditPreview e = GetEditor();
            //if (e == null)
            //    return;

            //e.DesignCtl.menuProperties_Click();
            ShowProperties(!Settings.Default.ShowProperties);
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (ctlInsertCurrent != null)
                ctlInsertCurrent.Checked = false;

            ToolStripButton ctl = (ToolStripButton)sender;
            ctlInsertCurrent = ctl.Checked ? ctl : null;

            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;
            mc.SetFocus();

            mc.CurrentInsert = ctlInsertCurrent == null ? null : (string)ctlInsertCurrent.Tag;
        }

        private void menuFileExit_Click(object sender, EventArgs e)
        {
            //if (!OkToSave())
            //    return;
            //SaveStartupState();
            //menuToolsCloseProcess(false);
            //CleanupTempFiles();
            Application.Exit();
            //			Environment.Exit(0);
        }

        private void pickerForeColour_SelectedColorChanged(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            if (!bSuppressChange)
            {
                if (pickerForeColour.Colour.IsNamedColor)
                {
                    mc.ApplyStyleToSelected("Color", pickerForeColour.Colour.Name);
                }
                else
                {
                    //Converts "AARRGGBB" -> "#RRGGBB"
                    mc.ApplyStyleToSelected("Color", "#" + pickerForeColour.Colour.Name.Substring(2));
                }
                SetProperties(mc);
            }
            SetMDIChildFocus(mc);
        }

        private void pickerBackColour_SelectedColorChanged(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;
            if (!bSuppressChange)
            {
                if (pickerBackColour.Colour.IsNamedColor)
                {
                    mc.ApplyStyleToSelected("BackgroundColor", pickerBackColour.Colour.Name);
                }
                else
                {
                    //Converts "AARRGGBB" -> "#RRGGBB"
                    mc.ApplyStyleToSelected("BackgroundColor", "#" + pickerBackColour.Colour.Name.Substring(2));
                }
                SetProperties(mc);
            }

            SetMDIChildFocus(mc);
        }

        private void cmbFontSize_TextChanged(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            if (!bSuppressChange)
            {
                mc.ApplyStyleToSelected("FontSize", cmbFontSize.Text + "pt");
                SetProperties(mc);
            }
            SetMDIChildFocus(mc);
        }

        private void cmbFont_TextChanged(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            if (!bSuppressChange)
            {
                mc.ApplyStyleToSelected("FontFamily", cmbFont.Text);
                SetProperties(mc);
            }
            SetMDIChildFocus(mc);
        }

        private void btnBold_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            mc.ApplyStyleToSelected("FontWeight", btnBold.Checked ? "Bold" : "Normal");
            SetProperties(mc);

            SetMDIChildFocus(mc);
        }

        private void btnItalic_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            mc.ApplyStyleToSelected("FontStyle", btnItalic.Checked ? "Italic" : "Normal");
            SetProperties(mc);

            SetMDIChildFocus(mc);
        }

        private void btnUnderline_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            mc.ApplyStyleToSelected("TextDecoration", btnUnderline.Checked ? "Underline" : "None");
            SetProperties(mc);

            SetMDIChildFocus(mc);
        }

        private void TextAlign_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            TextAlignEnum ta = TextAlignEnum.General;

            if (sender == btnLAlign)
            {
                ta = TextAlignEnum.Left;
                btnLAlign.Checked = true;
                btnRAlign.Checked = btnCAlign.Checked = false;
            }
            else if (sender == btnRAlign)
            {
                ta = TextAlignEnum.Right;
                btnRAlign.Checked = true;
                btnLAlign.Checked = btnCAlign.Checked = false;
            }
            else if (sender == btnCAlign)
            {
                ta = TextAlignEnum.Center;
                btnCAlign.Checked = true;
                btnRAlign.Checked = btnLAlign.Checked = false;
            }

            mc.ApplyStyleToSelected("TextAlign", ta.ToString());
            SetProperties(mc);

            SetProperties(mc);
            SetMDIChildFocus(mc);
        }

        private void cmbZoomLevel_TextChanged(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;
            mc.SetFocus();

            switch (cmbZoomLevel.Text)
            {
                case "Actual Size":
                    mc.Zoom = 1;
                    break;
                case "Fit Page":
                    mc.ZoomMode = ZoomMode.FitPage;
                    break;
                case "Fit Width":
                    mc.ZoomMode = ZoomMode.FitWidth;
                    break;
                default:
                    string s = cmbZoomLevel.Text.Substring(0, cmbZoomLevel.Text.Length - 1);
                    float z;
                    try
                    {
                        z = Convert.ToSingle(s) / 100f;
                        mc.Zoom = z;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Zoom Value Invalid");
                    }
                    break;
            }
        }

        private void menuEditSelectAll_Click(object sender, EventArgs e)
        {
            if (this.txtExpression != null && txtExpression.Focused)
            {
                txtExpression.SelectAll();
                return;
            }
            RdlEditPreview editor = GetEditor();
            if (editor == null)
                return;

            editor.SelectAll();
        }

        private void menuViewDesigner_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;
            mc.RdlEditor.DesignTab = "design";
        }

        private void menuViewRDLText_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;
            mc.RdlEditor.DesignTab = "edit";
        }

        private void menuViewPreview_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;
            mc.RdlEditor.DesignTab = "preview";
        }

        private void menuDataDataSets_DropDownOpening(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            bool bEnable = false;
            if (mc != null && mc.DesignTab == "design")
                bEnable = true;

            //this.menuDataDataSources.Enabled = this.menuDataDataSets.Enabled = this.menuDataEmbeddedImages.Enabled = bEnable;
            if (!bEnable)
                return;

            // Run thru all the existing DataSets
            menuDataDataSets.DropDownItems.Clear();
            menuDataDataSets.DropDownItems.Add(menuDataDataSetsNew);

            DesignXmlDraw draw = mc.DrawCtl;
            XmlNode rNode = draw.GetReportNode();
            XmlNode dsNode = draw.GetNamedChildNode(rNode, "DataSets");
            if (dsNode != null)
            {
                foreach (XmlNode dNode in dsNode)
                {
                    if (dNode.Name != "DataSet")
                        continue;
                    XmlAttribute nAttr = dNode.Attributes["Name"];
                    if (nAttr == null)	// shouldn't really happen
                        continue;
                    menuDataDataSets.DropDownItems.Add(new ToolStripMenuItem(nAttr.Value, null,
                        new EventHandler(this.menuDataDataSetsNew_Click)));
                }
            }
        }

        private void menuDataDataSetsNew_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.DrawCtl == null || mc.ReportDocument == null || mc.Editor == null) //new:  || mc.Editor == null because editing is not allowed e.g. n Preview mode
                return;

            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            if (menu == null)
                return;
            mc.Editor.StartUndoGroup("DataSet Dialog");

            string dsname = menu.Text;

            // Find the dataset we need
            List<XmlNode> ds = new List<XmlNode>();
            DesignXmlDraw draw = mc.DrawCtl;
            XmlNode rNode = draw.GetReportNode();
            XmlNode dsNode = draw.GetCreateNamedChildNode(rNode, "DataSets");
            XmlNode dataset = null;

            // find the requested dataset: the menu text equals the name of the dataset
            int dsCount = 0;		// count of the datasets
            string onlyOneDsname = null;
            foreach (XmlNode dNode in dsNode)
            {
                if (dNode.Name != "DataSet")
                    continue;
                XmlAttribute nAttr = dNode.Attributes["Name"];
                if (nAttr == null)	// shouldn't really happen
                    continue;
                if (dsCount == 0)
                    onlyOneDsname = nAttr.Value;		// we keep track of 1st name; 

                dsCount++;
                if (nAttr.Value == dsname)
                    dataset = dNode;
            }

            bool bNew = false;
            if (dataset == null)	// This must be the new menu item
            {
                dataset = draw.CreateElement(dsNode, "DataSet", null);	// create empty node
                bNew = true;
            }
            ds.Add(dataset);

            using (PropertyDialog pd = new PropertyDialog(mc.DrawCtl, ds, PropertyTypeEnum.DataSets))
            {
                DialogResult dr = pd.ShowDialog();
                if (pd.Changed || dr == DialogResult.OK)
                {
                    if (dsCount == 1)
                    // if we used to just have one DataSet we may need to fix up DataRegions 
                    //	that were defaulting to that name
                    {
                        dsCount = 0;
                        bool bUseName = false;
                        foreach (XmlNode dNode in dsNode)
                        {
                            if (dNode.Name != "DataSet")
                                continue;
                            XmlAttribute nAttr = dNode.Attributes["Name"];
                            if (nAttr == null)	// shouldn't really happen
                                continue;

                            dsCount++;
                            if (onlyOneDsname == nAttr.Value)
                                bUseName = true;
                        }
                        if (bUseName && dsCount > 1)
                        {
                            foreach (XmlNode drNode in draw.ReportNames.ReportItems)
                            {
                                switch (drNode.Name)
                                {
                                    // If a DataRegion doesn't have a dataset name specified use previous one
                                    case "Table":
                                    case "List":
                                    case "Matrix":
                                    case "Chart":
                                        XmlNode aNode = draw.GetNamedChildNode(drNode, "DataSetName");
                                        if (aNode == null)
                                            draw.CreateElement(drNode, "DataSetName", onlyOneDsname);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    mc.Modified = true;
                }
                else if (bNew)	// if canceled and new DataSet get rid of temp node
                {
                    dsNode.RemoveChild(dataset);
                }
                if (pd.Delete)	// user must have hit a delete button for this to get set
                    dsNode.RemoveChild(dataset);

                if (!dsNode.HasChildNodes)		// If no dataset exists we remove DataSets
                    draw.RemoveElement(rNode, "DataSets");

                mc.Editor.EndUndoGroup(pd.Changed || dr == DialogResult.OK);
            }
        }

        private void menuDataDataSources_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.StartUndoGroup("DataSources Dialog");
            using (DialogDataSources dlgDS = new DialogDataSources(mc.SourceFile, mc.DrawCtl))
            {
                dlgDS.StartPosition = FormStartPosition.CenterParent;
                DialogResult dr = dlgDS.ShowDialog();
                mc.Editor.EndUndoGroup(dr == DialogResult.OK);
                if (dr == DialogResult.OK)
                    mc.Modified = true;
            }
        }

        private void menuDataEmbeddedImages_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.StartUndoGroup("Embedded Images Dialog");
            using (DialogEmbeddedImages dlgEI = new DialogEmbeddedImages(mc.DrawCtl))
            {
                dlgEI.StartPosition = FormStartPosition.CenterParent;
                DialogResult dr = dlgEI.ShowDialog();
                mc.Editor.EndUndoGroup(dr == DialogResult.OK);
                if (dr == DialogResult.OK)
                    mc.Modified = true;
            }
        }

        private void menuDataCreateSharedDataSource_Click(object sender, EventArgs e)
        {
            using (DialogDataSourceRef dlgDS = new DialogDataSourceRef())
            {
                dlgDS.StartPosition = FormStartPosition.CenterParent;
                dlgDS.ShowDialog();
                if (dlgDS.DialogResult == DialogResult.Cancel)
                    return;
            }
        }

        private void menuFormatAlignLefts_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.AlignLefts();
            SetProperties(mc);
        }

        private void menuFormatAlignCenters_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.AlignCenters();
            SetProperties(mc);
        }

        private void menuFormatAlignRights_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.AlignRights();
            SetProperties(mc);
        }

        private void menuFormatAlignTops_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.AlignTops();
            SetProperties(mc);
        }

        private void menuFormatAlignMiddles_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.AlignMiddles();
            SetProperties(mc);
        }

        private void menuFormatAlignBottoms_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.AlignBottoms();
            SetProperties(mc);
        }

        private void menuFormatSizeWidth_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.SizeWidths();
            SetProperties(mc);
        }

        private void menuFormatSizeHeight_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.SizeHeights();
            SetProperties(mc);
        }

        private void menuFormatSizeBoth_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.SizeBoth();
            SetProperties(mc);
        }

        private void menuFormatHorizontalSpacingMakeEqual_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.HorzSpacingMakeEqual();
            SetProperties(mc);
        }

        private void menuFormatHorizontalSpacingIncrease_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.HorzSpacingIncrease();
            SetProperties(mc);
        }

        private void menuFormatHorizontalSpacingDecrease_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.HorzSpacingDecrease();
            SetProperties(mc);
        }

        private void menuFormatHorizontalSpacingZero_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.HorzSpacingMakeZero();
            SetProperties(mc);
        }

        private void menuFormatVerticalSpacingMakeEqual_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.VertSpacingMakeEqual();
            SetProperties(mc);
        }

        private void menuFormatVerticalSpacingIncrease_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.VertSpacingIncrease();
            SetProperties(mc);
        }

        private void menuFormatVerticalSpacingDecrease_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.VertSpacingDecrease();
            SetProperties(mc);
        }

        private void menuFormatVerticalSpacingMakeZero_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null || mc.Editor == null)
                return;

            mc.Editor.DesignCtl.VertSpacingMakeZero();
            SetProperties(mc);
        }

        private void menuFormatPadding_Click(object sender, System.EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            ToolStripMenuItem mi = sender as ToolStripMenuItem;

            string padname = null;
            int paddiff = 0;
            if (mi == menuFormatPaddingLeftIncrease)
            {
                padname = "PaddingLeft";
                paddiff = 4;
            }
            else if (mi == menuFormatPaddingLeftDecrease)
            {
                padname = "PaddingLeft";
                paddiff = -4;
            }
            else if (mi == menuFormatPaddingLeftZero)
            {
                padname = "PaddingLeft";
                paddiff = 0;
            }
            else if (mi == menuFormatPaddingRightIncrease)
            {
                padname = "PaddingRight";
                paddiff = 4;
            }
            else if (mi == menuFormatPaddingRightDecrease)
            {
                padname = "PaddingRight";
                paddiff = -4;
            }
            else if (mi == menuFormatPaddingRightZero)
            {
                padname = "PaddingRight";
                paddiff = 0;
            }
            else if (mi == menuFormatPaddingTopIncrease)
            {
                padname = "PaddingTop";
                paddiff = 4;
            }
            else if (mi == menuFormatPaddingTopDecrease)
            {
                padname = "PaddingTop";
                paddiff = -4;
            }
            else if (mi == menuFormatPaddingTopZero)
            {
                padname = "PaddingTop";
                paddiff = 0;
            }
            else if (mi == menuFormatPaddingBottomIncrease)
            {
                padname = "PaddingBottom";
                paddiff = 4;
            }
            else if (mi == menuFormatPaddingBottomDecrease)
            {
                padname = "PaddingBottom";
                paddiff = -4;
            }
            else if (mi == menuFormatPaddingBottomZero)
            {
                padname = "PaddingBottom";
                paddiff = 0;
            }

            if (padname != null)
            {
                mc.Editor.DesignCtl.SetPadding(padname, paddiff);
                SetProperties(mc);
            }

        }

        private void menuToolsValidateRDL_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            if (_ValidateRdl == null)
            {
                _ValidateRdl = new DialogValidateRdl(mc);
                _ValidateRdl.Show();
            }
            else
                _ValidateRdl.BringToFront();
            return;
        }

        private void menuToolsStartDesktop_Click(object sender, EventArgs e)
        {
            if (_ServerProcess == null)
                menuToolsStartProcess(true);
            else
                menuToolsCloseProcess(true);
        }

        private void menuToolsOptions_Click(object sender, EventArgs e)
        {
            //using (DialogToolOptions dlg = new DialogToolOptions())
            //{
            //    DialogResult rc = dlg.ShowDialog();
            //}
        }

        private void menuWindowCascade_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.Cascade);
        }

        private void menuWindowTileHorizontally_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void menuWindowTileVertically_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileVertical);
        }

        private void menuWindowCloseAll_Click(object sender, EventArgs e)
        {
            foreach (Form f in this.MdiChildren)
            {
                f.Close();
            }
        }

        private void menuHelpHelp_Click(object sender, EventArgs e)
        {

        }

        private void menuHelpSupport_Click(object sender, EventArgs e)
        {

        }

        private void menuHelpAbout_Click(object sender, EventArgs e)
        {
            using (DialogAbout dlg = new DialogAbout())
            {
                dlg.ShowDialog();
            }
        }        
        
        private void menuViewShowReportInBrowser_Click(object sender, EventArgs e)
        {

        }

        private void RdlDesignerForm_MdiChildActivate(object sender, EventArgs e)
        {
            if (this._ValidateRdl != null)		// don't keep the validation open when window changes
                this._ValidateRdl.Close();

            DesignTabChanged(sender, e);
            SelectionChanged(sender, e);
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;
            mc.SetFocus();
            if (mc.Tab != null)
                tcMain.SelectTab(mc.Tab);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            mc.Export();
            return;
        }

        private void menuView_DropDownOpening(object sender, EventArgs e)
        {
            menuViewPropertiesWindow.Checked = Settings.Default.ShowProperties;
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            mc.Zoom -= 0.25f;

            if (mc.Zoom <= 0.25f)
            {
                mc.Zoom = 0.25f;
                btnZoomOut.Enabled = false;
            }

            btnZoomIn.Enabled = true;
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null)
                return;

            mc.Zoom += 0.25f;

            if (mc.Zoom >= 4.0f)
            {
                mc.Zoom = 4.0f;
                btnZoomIn.Enabled = false;
            }

            btnZoomOut.Enabled = true;
        }

        private void RdlDesignerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveStartupState();
            menuToolsCloseProcess(false);
            CleanupTempFiles();
        }

        private void btnExpression_Click(object sender, EventArgs e)
        {
            MDIChild mc = this.ActiveMdiChild as MDIChild;
            if (mc == null ||
                mc.DesignTab != "design" || mc.DrawCtl.SelectedCount != 1 ||
                mc.Editor == null)
                return;

            XmlNode tn = mc.DrawCtl.SelectedList[0];

            using (DialogExprEditor de = new DialogExprEditor(mc.DrawCtl, txtExpression.Text, tn))
            {
                // Display the UI editor dialog
                if (de.ShowDialog(this) == DialogResult.OK)
                {
                    txtExpression.Text = de.Expression;
                    mc.Editor.SetSelectedText(de.Expression);
                    SetProperties(mc);
                }
            }
        }
		#endregion

        

           
        
    }
}
