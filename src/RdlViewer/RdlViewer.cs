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
using Reporting.Viewer;
using Reporting.Viewer.Canvas.Drawing;
using Reporting.Viewer.Dialogs;
using Reporting.Rdl.Utility;

namespace Reporting.Viewer
{
	/// <summary>
	/// RdlViewer displays RDL files or syntax. 
	/// </summary>
	public partial class RdlViewer : System.Windows.Forms.UserControl
    {
        public static string LicenseInfo()
        {
            return
@"

Some Icons are Copyright © Yusuke Kamiyamane. All rights reserved. 
Licensed under a Creative Commons Attribution 3.0 license.
    http://creativecommons.org/licenses/by/3.0/

Portions of this project are Copyright © 2008-2010 Emilio Simões.
Licensed under the Code Project Open License.
    http://www.codeproject.com/info/cpol10.aspx";
        }

        #region Delegates and Events
        public delegate void HyperlinkEventHandler(object source, HyperlinkEventArgs e);
        /// <summary>
        /// Hyperlink invoked when report item with hyperlink is clicked on.
        /// </summary>
        public event HyperlinkEventHandler Hyperlink;
        public event EventHandler<SubreportDataRetrievalEventArgs> SubreportDataRetrieval;

        public NeedPassword GetDataSourceReferencePassword = null;
        
        #endregion
        #region Private Variables
        bool _InPaint=false;
		bool _InLoading=false;
		private Uri _SourceFileName;		// file name to use
		private string _SourceRdl;			// source Rdl; if provided overrides filename
		private string _Parameters;			// parameters to run the report
		private Report _Report;				// the report
		private string _Folder;				// folder for DataSourceReference (if file name not provided)
		private Pages _pgs;					// the pages of the report to view
		//private Canvas _pd;			// draws the pages of a report
		private bool _loadFailed;			// last load of report failed
		private float _leftMargin;			// left margin; calculated based on size of window & scroll style
		// report information
		private float _PageWidth;			// width of page
		private float _PageHeight;			// height of page
		private string _ReportDescription;
		private string _ReportAuthor;
		private string _ReportName;
		private IList _errorMsgs;

		// Zoom
		private float _zoom;                // zoom factor
        private PointF Dpi;
		private Enums.ZoomMode _zoomMode=Enums.ZoomMode.FitWidth;
		private float _leftGap=10;			// left margin: 10 points
		private float _rightGap=10;			// right margin: 10 points
		private float _pageGap=10;			// gap between pages: 10 points

		// printing 
        private bool _UseTrueMargins = true;    // compensate for non-printable region
		private int printEndPage;			// end page
		private int printCurrentPage;		// current page to print

		// Scrollbars
		private Enums.ScrollMode _ScrollMode;

        private string _HighlightText=null;      // text that should be highlighted when drawn
        private bool _HighlightCaseSensitive = false;   // highlight text is case insensitive
        private bool _HighlightAll = false;     // highlight all instances of Highlight text
        private PageItem _HighlightItem = null; // page item to highlight

		private bool _ShowParameters=true;
        private bool _ShowWaitDialog = true;    // show wait dialog when running report
        #endregion

        #region Constructors
        // Josh: 6:24:10
        // Added overload for contructor so that the page offset can be 
        // used when drawing to account for the leftgap offset.
        public RdlViewer()
            : this(new PointF(0, 0))
        { }

		public RdlViewer(PointF pageOffset)
		{
            
			_SourceFileName=null;
			_SourceRdl=null;
			_Parameters=null;				// parameters to run the report
			_pgs=null;						// the pages of the report to view
			_loadFailed=false;	
			_PageWidth=0;
			_PageHeight=0;
			_ReportDescription=null;
			_ReportAuthor=null;
			_ReportName=null;
			_zoom=-1;						// force zoom to be calculated

			// Get our graphics DPI					   
			Graphics g = null;
			try
			{
				g = this.CreateGraphics();
                Dpi = new PointF(g.DpiX, g.DpiY);
			}
			catch
			{
                Dpi = new PointF(96, 96);
			}
			finally
			{
				if (g != null)
					g.Dispose();
			}

            // Josh: 6:24:10 added page offset.x so that the 
            // page will display/print correctly.
            // Without it, the controls will be offset by the 
            // DesignXmlDraw.Leftgap amount.
            _leftGap -= Measurement.PixelsFromPoints(pageOffset.X, Dpi.X);

			_ScrollMode = Enums.ScrollMode.Continuous;

            this.Layout += new LayoutEventHandler(RdlViewer_Layout);

            InitializeComponent();

            _FindCtl.Viewer = this;

		}
        #endregion

        void _ParameterPanel_RunReport(object sender, EventArgs e)
        {
            System.Threading.Thread t = null;
            try
            {
                if (_ShowWaitDialog)
                {
                    t = new System.Threading.Thread(new System.Threading.ThreadStart(showWait));
                    t.Start();
                }
                _pgs = GetPages(this._Report);
                _DrawPanel.Pgs = _pgs;
                _vScroll.Value = 0;
                CalcZoom();
                _DrawPanel.Invalidate();
            }
            catch
            {
            }
            finally
            {
                if (t != null)
                {
                    int i = 0;
                    while (t.ThreadState != System.Threading.ThreadState.AbortRequested 
                        && t.ThreadState != System.Threading.ThreadState.Aborted 
                        && t.ThreadState != System.Threading.ThreadState.Stopped 
                        && t.ThreadState != System.Threading.ThreadState.StopRequested)
                    {
                        t.Abort();
                        i++;
                    }
                }
            }
            
        }

        public new bool Focus()
        {
            return (this._DrawPanel.Focus());
        }

        #region Public Properties
        /// <summary>
        /// When true printing will compensate for non-printable area of paper
        /// </summary>
        public bool UseTrueMargins
        {
            get { return _UseTrueMargins; }
            set { _UseTrueMargins = value; }
        }
        /// <summary>
        /// Show the Wait Dialog when retrieving and rendering report when true.
        /// </summary>
        public bool ShowWaitDialog
        {
            get { return _ShowWaitDialog; }
            set { _ShowWaitDialog = value; }
        }
		/// <summary>
		/// True if Parameter panel should be shown. 
		/// </summary>
		public bool ShowParameterPanel
		{
			get 
			{
				LoadPageIfNeeded();
				return _ShowParameters;
			}
			set 
			{
				_ShowParameters = value;
				RdlViewer_Layout(this, null);				// re layout based on new report
			}
		}
        /// <summary>
        /// True when find panel is visible
        /// </summary>
        public bool ShowFindPanel
        {
            get
            {
                return _FindCtl.Visible;
            }
            set
            {
                _FindCtl.Visible = value;
                RdlViewer_Layout(this, null);				// re layout based on new report
            }
        }
        /// <summary>
        /// Causes the find panel to find the next item
        /// </summary>
        public void FindNext()
        {
            _FindCtl.FindNext();
        }
        /// <summary>
        /// The color to use when highlighting the current found item
        /// </summary>
        public Color HighlightItemColor
        {
            get { return _DrawPanel.Properties.HighlightItemColor; }
            set { _DrawPanel.Properties.HighlightItemColor = value; }
        }
        /// <summary>
        /// The color to use when highlighting all
        /// </summary>
        public Color HighlightAllColor
        {
            get { return _DrawPanel.Properties.HighlightAllColor; }
            set { _DrawPanel.Properties.HighlightAllColor = value; }
        }

        /// <summary>
        /// The text to highlight when either HighLightAll is on or the HighLightItem is on.
        /// </summary>
        public string HighlightText
        {
            get { return _HighlightText; }
            set 
            {
                _HighlightText = value;
                _DrawPanel.Invalidate();    // force redraw
            }
        }

        /// <summary>
        /// When HighlightText has a value; HighlightAll controls whether
        /// all page items with that text will be highlighted
        /// </summary>
        public bool HighlightAll
        {
            get { return _HighlightAll; }
            set
            {
                _HighlightAll = value;
                if (_HighlightText != null && _HighlightText.Length > 0)
                    _DrawPanel.Invalidate();    // force redraw when need to
            }
        }

        /// <summary>
        /// When HighlightText has a value; HighlightCaseSensitive controls whether
        /// the comparison is case sensitive.
        /// </summary>
        public bool HighlightCaseSensitive
        {
            get { return _HighlightCaseSensitive; }
            set
            {
                _HighlightCaseSensitive = value;
                if (_HighlightText != null && _HighlightText.Length > 0)
                    _DrawPanel.Invalidate();    // force redraw when need to
            }
        }
        /// <summary>
        /// When used with HighlightText; HighlightPageItem will only highlight the selected item.
        /// </summary>
        public PageItem HighlightPageItem
        {
            get { return _HighlightItem; }
            set 
            { 
                _HighlightItem = value;
                _DrawPanel.Invalidate();    // force redraw
            }
        }
        /// <summary>
        /// Returns the number of pages in the report.  0 is returned if no report has been loaded.
        /// </summary>
		public int PageCount
		{
			get 
			{
				LoadPageIfNeeded();
				if (_pgs == null) 
					return 0;
				else
					return _pgs.PageCount;
			}
		}

		/// <summary>
		/// Sets/Returns the page currently showing
		/// </summary>
		public int PageCurrent
		{
			get 
			{
                if (_pgs == null)
                    return 0;
				int pc = (int) (_pgs.PageCount * (long) _vScroll.Value / (double) _vScroll.Maximum)+1; 
				if (pc > _pgs.PageCount)
					pc = _pgs.PageCount;
				return pc;
			}
			set 
			{
                if (_pgs == null)
                    return;
				// Contributed by Henrique (h2a) 07/14/2006
				if(value <= _pgs.PageCount && value >= 1) 
				{ 
//					_vScroll.Value = (int)((double)_vScroll.Maximum / _pgs.PageCount * (value -1)); 

                    double scrollValue = ((double)_vScroll.Maximum * (value - 1)) / _pgs.PageCount;
                    _vScroll.Value = (int) Math.Round( scrollValue);

					string tt = string.Format("Page {0} of {1}", 
						(int) (_pgs.PageCount * (long) _vScroll.Value / (double) _vScroll.Maximum)+1, 
						_pgs.PageCount); 

					_vScrollToolTip.SetToolTip(_vScroll, tt); 

					_DrawPanel.Invalidate(); 
				}
				else
					throw new ArgumentOutOfRangeException("PageCurrent", value, String.Format("Value must be between 1 and {0}.", _pgs.PageCount));
			}
		}

		/// <summary>
		/// Gets the report definition.
		/// </summary>
		public Report Report
		{
			get 
			{
				LoadPageIfNeeded();
				return _Report; 
			}
		}

		/// <summary>
		/// Forces the report to get rebuilt especially after changing parameters or data.
		/// </summary>
        public void Rebuild()
        {
            // Aulofee customization - start. Code added (2 lines) to avoid to execute twice GetPages and so the SQL query (custo end). 
            if (_pgs == null)
            {
                LoadPageIfNeeded();

                if (_Report == null)
                    throw new Exception("Report must be loaded prior to Rebuild being called.");
                // Aulofee customization - start. Code added (2 lines) to avoid to execute twice GetPages and so the SQL query (custo end). 
            }
            else
                _pgs = GetPages(this._Report);
            _DrawPanel.Pgs = _pgs;
            _vScroll.Value = 0;
            CalcZoom();
            _DrawPanel.Invalidate();
        }

        /// <summary>
		/// Gets/Sets the ScrollMode.  
		///		SinglePage: Shows a single page shows in pane.
		///		Continuous: Shows pages as a continuous vertical column.
		///		Facing: Shows first page on right side of pane, then alternating
		///				with single page scrolling.
		///		ContinuousFacing: Shows 1st page on right side of pane, then alternating 
		///				with continuous scrolling.
		/// </summary>
		public Enums.ScrollMode ScrollMode
		{
			get { return _ScrollMode; }
			set 
			{ 
				_ScrollMode = value; 
				CalcZoom(); 
				this._DrawPanel.Invalidate();
			}
		}
        /// <summary>
        /// Enables/Disables the selection tool.  The selection tool allows the user
        /// to select text and images on the display and copy it to the clipboard.
        /// </summary>
        public bool SelectTool
        {
            get { return _DrawPanel.ItemManager.SelectToolEnabled; }
            set { _DrawPanel.ItemManager.SelectToolEnabled = value; }
        }
        /// <summary>
        /// Returns true when one or more PageItems are selected.
        /// </summary>
        public bool CanCopy
        {
            get { return _DrawPanel.ItemManager.CanCopy; }
        }
        /// <summary>
        /// Copies the current selection (if any) to the clipboard.
        /// </summary>
        public void Copy()
        {
            if (!CanCopy)
                return;

            Image im = _DrawPanel.ItemManager.SelectImage;
            if (im == null)
                Clipboard.SetDataObject(SelectText, true);
            else
            {
                Clipboard.SetImage(im);
                im.Dispose();
            }
        }
        /// <summary>
        /// The contents of the selected text.  Tab separate items on same y coordinate;
        /// newline separate items when y coordinate changes.   Order is based on user
        /// selection order.
        /// </summary>
        public string SelectText
        {
            get
            {
                return _DrawPanel.ItemManager.SelectText;
            }
        }

        /// <summary>
		/// Holds a file name that contains the RDL (Report Specification Language).  Setting
		/// this field will cause a new report to be loaded into the viewer.
		/// SourceFile is mutually exclusive with SourceRdl.  Setting SourceFile will nullify SourceRdl.
		/// </summary>
		public Uri SourceFile
		{
			get 
			{
				return _SourceFileName;
			}
			set 
			{
				_SourceFileName=value;
                if (value != null)
				    _SourceRdl = null;
				_vScroll.Value = _hScroll.Value = 0;
				_pgs = null;				// reset pages, only if SourceRdl is also unavailable
				_DrawPanel.Pgs = null;
				_loadFailed=false;			// attempt to load the report
				if (this.Visible)
				{
					LoadPageIfNeeded();			// force load of report
					this._DrawPanel.Invalidate();
				}
			}
		}

		/// <summary>
		/// Holds the XML source of the report in RDL (Report Specification Language).
		/// SourceRdl is mutually exclusive with SourceFile.  Setting SourceRdl will nullify SourceFile.
		/// </summary>
		public string SourceRdl
		{
			get {return _SourceRdl;}
			set 
			{
				_SourceRdl=value;
                if (value != null)
				    _SourceFileName=null;
				_pgs = null;				// reset pages
				_DrawPanel.Pgs = null;
				_loadFailed=false;			// attempt to load the report	
				_vScroll.Value = _hScroll.Value = 0;
				if (this.Visible)
				{
					LoadPageIfNeeded();			// force load of report
					this._DrawPanel.Invalidate();
				}
			}
		}
		
		/// <summary>
		/// Holds the folder to data source reference files when SourceFileName not available.
		/// </summary>
		public string Folder
		{
			get {return _Folder;}
			set {_Folder = value;}
		}

		/// <summary>
		/// Parameters passed to report when run.  Parameters are separated by '&'.  For example,
		/// OrderID=10023&OrderDate=10/14/2002
		/// Note: these parameters will override the user specified ones.
		/// </summary>
		public string Parameters
		{
			get {return _Parameters;}
			set {_Parameters=value;}
		}

		/// <summary>
		/// The height of the report page (in points) as defined within the report.
		/// </summary>
		public float PageHeight
		{
			get 
			{
				LoadPageIfNeeded();
				return _PageHeight;
			}
		}

		/// <summary>
		/// The width of the report page (in points) as defined within the report.
		/// </summary>
		public float PageWidth
		{
			get 
			{
				LoadPageIfNeeded();
				return _PageWidth;
			}
		}

		/// <summary>
		/// Description of the report.
		/// </summary>
		public string ReportDescription
		{
			get 
			{
				LoadPageIfNeeded();
				return _ReportDescription;
			}
		}

		/// <summary>
		/// Author of the report.
		/// </summary>
		public string ReportAuthor
		{
			get 
			{
				LoadPageIfNeeded();
				return _ReportAuthor;
			}
		}

		/// <summary>
		/// Name of the report.
		/// </summary>
		public string ReportName
		{
			get 
			{
				return _ReportName;
			}
			set {_ReportName = value;}
		}

		/// <summary>
		/// Zoom factor.  For example, .5 is a 50% reduction, 2 is 200% increase.
		/// Setting this value will force ZoomMode to UseZoom.
		/// </summary>
		public float Zoom
		{
			get {return _zoom;}
			set 
			{
				_zoom = value;
				this._zoomMode = Enums.ZoomMode.UseZoom;
				CalcZoom();			// this adjust any scrolling issues
				this._DrawPanel.Invalidate();
			}
		}

		/// <summary>
		/// ZoomMode.  Optionally, allows zoom to dynamically change depending on pane size.
		/// </summary>
		public Enums.ZoomMode ZoomMode
		{
			get {return _zoomMode; }
			set 
			{
				_zoomMode = value; 
				CalcZoom();				// force zoom calculation
				this._DrawPanel.Invalidate();
			}
        }

        #endregion

        #region Printing
        /// <summary>
		/// Print the report.
		/// </summary>
		public void Print(PrintDocument pd)
		{
			LoadPageIfNeeded();

			pd.PrintPage += new PrintPageEventHandler(PrintPage);
			printCurrentPage=-1;
			switch (pd.PrinterSettings.PrintRange)
			{
				case PrintRange.AllPages:
					printCurrentPage = 0;
					printEndPage = _pgs.PageCount - 1;
					break;
				case PrintRange.Selection:
					printCurrentPage = pd.PrinterSettings.FromPage - 1;
					printEndPage = pd.PrinterSettings.FromPage - 1;
					break;
				case PrintRange.SomePages:
					printCurrentPage = pd.PrinterSettings.FromPage - 1;
					if (printCurrentPage < 0)
						printCurrentPage = 0;
					printEndPage = pd.PrinterSettings.ToPage - 1;
					if (printEndPage >= _pgs.PageCount)
						printEndPage = _pgs.PageCount - 1;
					break;
			}
			pd.Print();
		}

		private void PrintPage(object sender, PrintPageEventArgs e)
		{
			System.Drawing.Rectangle r = new System.Drawing.Rectangle(0, 0, int.MaxValue, int.MaxValue);
            // account for the non-printable area of the paper
            PointF pageOffset;
            if (this.UseTrueMargins && this._Report != null)
            {
                // The page offset is set in pixels as the Draw method changes the graphics object to use pixels
                // (the origin transform does not get changed by the change in units.  PrintableArea returns
                // numbers in the hundredths of an inch.

                float x = ((e.PageSettings.PrintableArea.X * e.Graphics.DpiX) / 100.0F) - e.Graphics.Transform.OffsetX;
                float y = ((e.PageSettings.PrintableArea.Y * e.Graphics.DpiY) / 100.0F) - e.Graphics.Transform.OffsetY;
                
                // Get the margins in printer pixels (don't use the function!)
                // Points to pixels conversion ((double)x * DpiX / POINTSIZEF)
                //float lm = (float)((double)_Report.LeftMarginPoints * e.Graphics.DpiX / POINTSIZEF);
                float leftMargin = Measurement.PixelsFromPoints(_Report.LeftMarginPoints, e.Graphics.DpiX);
                //float tm = (float)((double)_Report.TopMarginPoints * e.Graphics.DpiY / POINTSIZEF);
                float topMargin = Measurement.PixelsFromPoints(_Report.TopMarginPoints, e.Graphics.DpiY);
                // Correct based on the report margin
                if (x > leftMargin)      // left margin is less than the minimum left margin
                    x = 0;
                if (y > topMargin)      // top margin is less than the minimum top margin
                    y = 0;
                pageOffset = new PointF(-x, -y);
            }
            else
            {
                pageOffset = PointF.Empty;
            }

            _DrawPanel.Draw(e.Graphics, printCurrentPage, r, false, pageOffset);	 

			printCurrentPage++;
			if (printCurrentPage > printEndPage)
				e.HasMorePages = false;
			else
				e.HasMorePages = true;
        }
        #endregion

        #region Viewer Shell Tie-Ins
        /// <summary>
		/// Save the file.  The extension determines the type of file to save.
		/// </summary>
		/// <param name="FileName">Name of the file to be saved to.</param>
		/// <param name="ext">Type of file to save.  Should be "pdf", "xml", "html", "mhtml", "csv", "rtf", "excel", "tif".</param>
		public void SaveAs(string FileName, string type)
		{
            //TODO: Come back and make this section work off of a plugin Report.Render method.
			LoadPageIfNeeded();

			string ext = type.ToLower();
			OneFileStreamGen sg = new OneFileStreamGen(FileName, true);	// overwrite with this name
            if (!(ext == "pdf" || ext == "tif" || ext == "tiff" || ext == "tifbw"))
            {
                ListDictionary ld = GetParameters();		// split parms into dictionary
                _Report.RunGetData(ld);                     // obtain the data (again)
            }
			try
			{
				switch(ext)
				{
					case "pdf":	
						_Report.RunRenderPdf(sg, _pgs);
						break;
                    case "tif":
                    case "tiff":
                        _Report.RunRenderTif(sg, _pgs, true);
                        break;
                    case "tifbw":
                        _Report.RunRenderTif(sg, _pgs, false);
                        break;
					case "csv":
						_Report.RunRender(sg, OutputPresentationType.CSV);
						break;
                    case "doc":
                    case "rtf":
                        _Report.RunRender(sg, OutputPresentationType.RTF);
                        break;
                    case "excel":
                    case "xlsx":
                        _Report.RunRender(sg, OutputPresentationType.Excel);
                        break;
                    case "xml":
                        _Report.RunRender(sg, OutputPresentationType.XML);
                        break;
                    case "html":
                    case "htm":
						_Report.RunRender(sg, OutputPresentationType.HTML);
						break;
					case "mhtml": case "mht":
						_Report.RunRender(sg, OutputPresentationType.MHTML);
						break;
					default:
						throw new Exception("Unsupported file extension for SaveAs");
				}
			}
			finally
			{
				if (sg != null)
				{
					sg.CloseMainStream();
				}

			}
			return;
		}
        
        /// <summary>
        /// Finds the first instance of the search string.
        /// </summary>
        /// <param name="search"></param>
        /// <returns>null if not found</returns>
        public PageItem Find(string search)
        {
            return Find(search, null, Enums.ViewerSearchOptions.None);
        }

        /// <summary>
        /// Find locates the next string after the passed location.  Use ScrollToPageItem to then
        /// reposition the Viewer on that item
        /// </summary>
        /// <param name="search">Text to search for</param>
        /// <param name="position">PageItem after which to start search.  null starts at beginning</param>
        /// <param name="options">Multiple options can be or'ed together.</param>
        /// <returns>null if not found</returns>
		public PageItem Find(string search, PageItem position, Enums.ViewerSearchOptions options)
		{
            LoadPageIfNeeded();

            if (_pgs == null || _pgs.Count == 0)       // no report nothing to find
                return null;

            // initialize the loop direction and starting point
            int increment;
            int sPage;
            int sItem;
            if (((options & Enums.ViewerSearchOptions.Backward) == Enums.ViewerSearchOptions.Backward))
            {   // set to backward direction
                increment = -1;                 // go backwards
                sPage = _pgs.PageCount - 1;     // start at last page
                sItem = _pgs[sPage].Count - 1;  // start at bottom of last page
            }
            else
            {   // set to forward direction
                increment = 1;
                sPage = 0;
                sItem = 0;
            }

            bool bFirst = true;
            if (position != null)
            {
                sPage = position.Page.PageNumber - 1;   // start on same page as current
                sItem = position.ItemNumber + increment;  //   but on the item after/before the current one
            }

            if (!((options & Enums.ViewerSearchOptions.MatchCase) == Enums.ViewerSearchOptions.MatchCase))
                search = search.ToLower();          // should use Culture!!! todo

            PageItem found = null;
            for (int pi = sPage; pi < _pgs.Count && found == null && pi >= 0; pi = pi + increment)
            {
                Page p = _pgs[pi];
                if (bFirst)         // The first time sItem is already set
                    bFirst = false;
                else
                {
                    if (increment < 0)  // we're going backwards?
                        sItem = p.Count - 1;    // yes, start at bottom of page
                    else
                        sItem = 0;              // no, start at top of page
                }
                for (int pii = sItem; pii < p.Count && found == null && pii >= 0; pii = pii + increment)
                {
                    PageText pt = p[pii] as PageText;
                    if (pt == null)
                        continue;

                    if ((options & Enums.ViewerSearchOptions.MatchCase) == Enums.ViewerSearchOptions.MatchCase)
                    {
                        if (pt.Text.Contains(search))
                            found = pt;
                    }
                    else
                    {
                        if (pt.Text.ToLower().Contains(search))
                            found = pt;
                    }
                }
            }

            return found;
        }
        
        /// <summary>
        /// Scrolls to a specific page item in the report.
        /// </summary>
        /// <param name="pi">The page item to scroll to.</param>
        public void ScrollToPageItem(PageItem pi)
        {
            LoadPageIfNeeded();
            if (_pgs == null || _pgs.PageCount <= 0)    // nothing to scroll to
                return;

            int sPage = 0;
            int sItem = 0;
            int itemVerticalOffset = 0;
            int itemHorzOffset = 0;
            int height = 0;
            int width = 0;
            if (pi != null)
            {
                sPage = pi.Page.PageNumber-1;
                sItem = pi.ItemNumber;
                RectangleF rect = new RectangleF(
                    Measurement.PixelsFromPoints(pi.X + _leftMargin, Dpi.X),
                    Measurement.PixelsFromPoints(pi.Y, Dpi.Y),
                    Measurement.PixelsFromPoints(pi.W, Dpi.X),
                    Measurement.PixelsFromPoints(pi.H, Dpi.Y));
                itemVerticalOffset = (int) (rect.Top);
                itemHorzOffset = (int)rect.Left;
                width = (int)rect.Width;
                height = (int) (rect.Height);
            }

            // set the vertical scroll
            int scroll = (int)((double)_vScroll.Maximum * sPage / _pgs.PageCount) + itemVerticalOffset;
            
            // do we need to scroll vertically?
            if (!(_vScroll.Value <= scroll && _vScroll.Value + _DrawPanel.Height/this.Zoom >= scroll + height))
            {   // item isn't on visible part of window; force scroll
                _vScroll.Value = Math.Min(scroll, Math.Max(0,_vScroll.Maximum - _DrawPanel.Height));
                SetScrollControlsV();
                ScrollEventArgs sa = new ScrollEventArgs(ScrollEventType.ThumbPosition, _vScroll.Maximum + 1); // position is intentionally wrong
                bar_VerticalScroll(_vScroll, sa);
            }

            // set the horizontal scroll
            scroll = itemHorzOffset;

            // do we need to scroll horizontally?
            if (!(_hScroll.Value <= scroll && _hScroll.Value + _DrawPanel.Width / this.Zoom >= scroll + width))
            {   // item isn't on visible part of window; force scroll
                _hScroll.Value = Math.Min(scroll, Math.Max(0, _hScroll.Maximum-_DrawPanel.Width));
                SetScrollControlsH();
                ScrollEventArgs sa = new ScrollEventArgs(ScrollEventType.ThumbPosition, _hScroll.Maximum + 1); // position is intentionally wrong
                bar_HorizontalScroll(_hScroll, sa);
            }
        }
        #endregion

		private void DrawPanelPaint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			// Only handle one paint at a time
			lock (this)
			{
				if (_InPaint)
					return;
				_InPaint=true;
			}

			Graphics g = e.Graphics;
			try			// never want to die in here
			{
				if (!_InLoading && (_DrawPanel.Height > 5 && _DrawPanel.Width > 5))				// If we're in the process of loading don't paint
				{
					LoadPageIfNeeded();				// make sure we have something to show
				
					if (_zoom < 0)
						CalcZoom();				// new report or resize client requires new zoom factor
				
					// Draw the page
					_DrawPanel.Draw(g,_zoom, _leftMargin, _pageGap,
                        _hScroll.Value, 
                        _vScroll.Value,	
						e.ClipRectangle, 
                        _HighlightItem, _HighlightText, _HighlightCaseSensitive, _HighlightAll);
				}
			}
			catch (Exception ex)
			{	// don't want to kill process if we die
                //Warning: Minimizing the screen smaller
                // than the size of the control can cause
                // another "Generic GDI+ exception".
				using (Font font = new Font("Arial", 8))	
					g.DrawString(ex.Message+"\r\n"+ex.StackTrace, font, Brushes.Black,0,0);
			}		
			
			lock (this)
			{
				_InPaint=false;
			}
		}

		private void DrawPanelResize(object sender, EventArgs e)
		{
			CalcZoom();							// calc zoom
			_DrawPanel.Refresh();
        }

        // 15052008 AJM - Updating Render notification window - This could be improved to show current action in the future
        private void showWait()
        {
            DialogWait wait = new DialogWait(this);
            wait.ShowDialog();
        }

        #region Zoom
        private void CalcZoom()
		{
			switch (_zoomMode)
			{
				case Enums.ZoomMode.UseZoom:
					if (_zoom <= 0)			// normalize invalid values
						_zoom = 1;
					break;					// nothing to calculate
				case Enums.ZoomMode.FitWidth:
					CalcZoomFitWidth();
					break;
				case Enums.ZoomMode.FitPage:
					CalcZoomFitPage();
					break;
			}
			if (_zoom <= 0)
				_zoom = 1;
            float w = Measurement.PointsFromPixels(_DrawPanel.Width, Dpi.X);	// convert to points
            
			if (w > (this._PageWidth + _leftGap + _rightGap)*_zoom)
                //If viewing region is larger than pagewidth, center page.
				_leftMargin = ((w -(this._PageWidth + _leftGap + _rightGap)*_zoom)/2)/_zoom;
			else
				_leftMargin = _leftGap;
			if (_leftMargin < 0)
				_leftMargin = 0;
			SetScrollControls();			// zoom affects the scroll bars
			return;
		}
		
		private void CalcZoomFitPage()
		{
			try
			{
                float w = Measurement.PointsFromPixels(_DrawPanel.Width, Dpi.X);	// convert to points
                float h = Measurement.PointsFromPixels(_DrawPanel.Height, Dpi.Y);
                float xratio = w / (this._PageWidth + _leftGap + _rightGap + Measurement.PointsFromPixels(_vScroll.Width, Dpi.X));
				float yratio = h / (this._PageHeight + this._pageGap +  Measurement.PointsFromPixels(_hScroll.Height, Dpi.Y));	
				_zoom = Math.Min(xratio, yratio);
			}
			catch
			{
				_zoom = 1;			// shouldn't ever happen but this routine must never throw exception
			}
		}

		private void CalcZoomFitWidth()
		{
			try
			{
                float w = Measurement.PointsFromPixels(_DrawPanel.Width, Dpi.X);	// convert to points
				_zoom = w / (this._PageWidth + _leftGap + _rightGap 
                    + Measurement.PointsFromPixels(this._vScroll.Width, Dpi.X));

			}
			catch
			{
				_zoom = 1;			// shouldn't ever happen but this routine must never throw exception
			}
        }
        #endregion

        // Obtain the Pages by running the report
		private Report GetReport()
		{
			string prog;

			// Obtain the source
			if (_loadFailed)	
				prog = Rdl.Report.ErrorMessage(_errorMsgs);
			else if (_SourceRdl != null)
				prog = _SourceRdl;
			else if (_SourceFileName != null)
				prog = Rdl.Report.RdlSource(_SourceFileName);
			else	
				prog = Rdl.Report.EmptyMessage();

			// Compile the report
			// Now parse the file
			RDLParser rdlp;
			Report r;
			try
			{
				_errorMsgs = null;
				rdlp =  new RDLParser(prog);
				rdlp.DataSourceReferencePassword = GetDataSourceReferencePassword;
				if (_SourceFileName != null)
					rdlp.Folder = Path.GetDirectoryName(_SourceFileName.LocalPath);
				else
					rdlp.Folder = this.Folder;

				r = rdlp.Parse();
				if (r.ErrorMaxSeverity > 0) 
				{
					_errorMsgs = r.ErrorItems;		// keep a copy of the errors

					int severity = r.ErrorMaxSeverity;
					r.ErrorReset();
					if (severity > 4)
					{
						r = null;			// don't return when severe errors
						_loadFailed=true;
					}
				}
				// If we've loaded the report; we should tell it where it got loaded from
				if (r != null && !_loadFailed)
				{	// Don't care much if this fails; and don't want to null out report if it does
					try 
					{
						if (_SourceFileName != null)
						{
							r.Name = Path.GetFileNameWithoutExtension(_SourceFileName.LocalPath);
							r.Folder = Path.GetDirectoryName(_SourceFileName.LocalPath);
						}
						else
						{
							r.Folder = this.Folder;
							r.Name = this.ReportName;
						}
					}
					catch {}
				}
			}
			catch (Exception ex)
			{
				_loadFailed=true;
				_errorMsgs = new List<string>();		// create new error list
				_errorMsgs.Add(ex.Message);			// put the message in it
				_errorMsgs.Add(ex.StackTrace);		//   and the stack trace
				r = null;
			}

			if (r != null)
			{
				_PageWidth = r.PageWidthPoints;
				_PageHeight = r.PageHeightPoints;
				_ReportDescription = r.Description;
				_ReportAuthor = r.Author;
                r.SubreportDataRetrieval += new EventHandler<SubreportDataRetrievalEventArgs>(r_SubreportDataRetrieval);
				this._ParameterPanel.BuildParameters(r);
			}
			else
			{
				_PageWidth = 0;
				_PageHeight = 0;
				_ReportDescription = null;
				_ReportAuthor = null;
				_ReportName = null;
			}
			return r;
		}

        void r_SubreportDataRetrieval(object sender, SubreportDataRetrievalEventArgs e)
        {
            if (this.SubreportDataRetrieval != null)
                SubreportDataRetrieval(this, e);
        }

		private Pages GetPages()
		{
			this._Report = GetReport();
			if (_loadFailed)			// retry on failure; this will get error report
				this._Report = GetReport();

			return GetPages(this._Report);
		}

		private Pages GetPages(Report report)
		{
			Pages pgs=null;

			ListDictionary ld = GetParameters();		// split parms into dictionary

			try
			{
				report.RunGetData(ld);

				pgs = report.BuildPages();

				if (report.ErrorMaxSeverity > 0) 
				{
					if (_errorMsgs == null)
						_errorMsgs = report.ErrorItems;		// keep a copy of the errors
					else
					{
						foreach (string err in report.ErrorItems)
							_errorMsgs.Add(err);
					}

					report.ErrorReset();
				}

			}
			catch (Exception e)
			{
				string msg = e.Message;
			}
			
			return pgs;
		}

		private ListDictionary GetParameters()
		{
			ListDictionary ld= new ListDictionary();
			if (_Parameters == null)
				return ld;				// dictionary will be empty in this case

			// parms are separated by &
			char[] breakChars = new char[] {'&'};
            string parm = _Parameters.Replace("&amp;", '\ufffe'.ToString());    // handle &amp; as user wanted '&'
            string[] ps = parm.Split(breakChars);
			foreach (string p in ps)
			{
				int iEq = p.IndexOf("=");
				if (iEq > 0)
				{
					string name = p.Substring(0, iEq);
					string val = p.Substring(iEq+1);
                    ld.Add(name, val.Replace('\ufffe', '&'));
				}
			}
			return ld;
		}

		/// <summary>
		/// Call LoadPageIfNeeded when a routine requires the report to be loaded in order
		/// to fulfill the request.
		/// </summary>
		private void LoadPageIfNeeded()
		{
			if (_pgs == null)
			{
				Cursor savec=null;
                System.Threading.Thread t=null;
				try
				{
                    // 15052008 AJM - Updating Render notification window - This could be improved to show current action in the future
                    if (_ShowWaitDialog)
                    {
                        t = new System.Threading.Thread(new System.Threading.ThreadStart(showWait));
                        t.Start();
                    }
					_InLoading = true;
                    savec = this.Cursor;				// this could take a while so put up wait cursor
					this.Cursor = Cursors.WaitCursor;
					_pgs = GetPages();
					_DrawPanel.Pgs = _pgs;
					CalcZoom();							// this could affect zoom
				}
				finally
				{
					_InLoading = false;
					if (savec != null)
						this.Cursor = savec;
                    if (t != null)
                    {
                        int i = 0;
                        while ((t.ThreadState & System.Threading.ThreadState.AbortRequested) != System.Threading.ThreadState.AbortRequested &&
                            (t.ThreadState & System.Threading.ThreadState.Aborted) != System.Threading.ThreadState.Aborted &&
                            (t.ThreadState & System.Threading.ThreadState.Stopped) != System.Threading.ThreadState.Stopped &&
                            (t.ThreadState & System.Threading.ThreadState.StopRequested) != System.Threading.ThreadState.StopRequested)
                        {
                            try
                            {
                                t.Abort();
                            }
                            catch //(Exception e) PJR don't declare variable as we aren't using it anyway.
                            {
                            }
                            i++;
                        }
                    }
                    
				}
				RdlViewer_Layout(this, null);				// re layout based on new report
			}
        }

        #region Scrolling
        private void SetScrollControls()
		{
			if (_pgs == null)		// nothing loaded; nothing to do
			{
				_vScroll.Enabled = _hScroll.Enabled = false;
				_vScroll.Value = _hScroll.Value = 0;
				return;
			}
			SetScrollControlsV();
			SetScrollControlsH();
		}

		private void SetScrollControlsV()
		{
			// calculate the vertical scroll needed
            float h = _DrawPanel.Height;	// height of pane
            if (_zoom * (Measurement.PixelsFromPoints(((this._PageHeight + this._pageGap) * _pgs.PageCount) + this._pageGap, Dpi.Y) + _hScroll.Height) <= h)
			{
				_vScroll.Enabled = false;
				_vScroll.Value = 0;
				return;
			}
			_vScroll.Minimum = 0;
            _vScroll.Maximum = (int)(Measurement.PixelsFromPoints(((this._PageHeight + this._pageGap) * _pgs.PageCount)  + this._pageGap, Dpi.Y) + _hScroll.Height);
			_vScroll.Value = Math.Min(_vScroll.Value, _vScroll.Maximum);
			if (this._zoomMode == Enums.ZoomMode.FitPage)
			{
				_vScroll.LargeChange = (int) (_vScroll.Maximum / _pgs.PageCount);
				_vScroll.SmallChange = _vScroll.LargeChange;
			}
			else
			{
				_vScroll.LargeChange = (int) (Math.Max(_DrawPanel.Height,0) / _zoom);
				_vScroll.SmallChange = _vScroll.LargeChange / 5;
			}
			_vScroll.Enabled = true;
			string tt = string.Format("Page {0} of {1}", 
					(int) (_pgs.PageCount * (long) _vScroll.Value / (double) _vScroll.Maximum)+1, 
					_pgs.PageCount);

			_vScrollToolTip.SetToolTip(_vScroll, tt);
//			switch (_ScrollMode)
//			{
//				case ScrollModeEnum.SinglePage:
//					break;
//				case ScrollModeEnum.Continuous:
//				case ScrollModeEnum.ContinuousFacing:
//				case ScrollModeEnum.Facing:
//					break;
//			}
			return;
		}

		private void SetScrollControlsH()
			{
			// calculate the horizontal scroll needed
                float w =_DrawPanel.Width;	// width of pane
			if (_zoomMode == Enums.ZoomMode.FitPage || 
				_zoomMode == Enums.ZoomMode.FitWidth ||
                _zoom * (Measurement.PixelsFromPoints(this._PageWidth + 
                                                this._leftGap + 
                                                this._rightGap, Dpi.X)
                                                + _vScroll.Width)
                                                 
                                            <= w)
			{
				_hScroll.Enabled = false;
				_hScroll.Value = 0;
				return;
			}
            
			_hScroll.Minimum = 0;
            _hScroll.Maximum = (int)(Measurement.PixelsFromPoints(this._PageWidth + this._leftGap + this._rightGap, Dpi.X) +_vScroll.Width);
			_hScroll.Value = Math.Min(_hScroll.Value, _hScroll.Maximum);
			_hScroll.LargeChange = (int) (Math.Max(_DrawPanel.Width,0) / _zoom);
			_hScroll.SmallChange = _hScroll.LargeChange / 5;
			_hScroll.Enabled = true;

			return;
		}

		private void bar_HorizontalScroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{
            if (_hScroll.IsDisposed)
                return;

			if (e.NewValue == _hScroll.Value)	// don't need to scroll if already there
				return;

			_DrawPanel.Invalidate();   
		}

		private void bar_VerticalScroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{
            if (_vScroll.IsDisposed)
                return;

			if (e.NewValue == _vScroll.Value)	// don't need to scroll if already there
				return;

			string tt = string.Format("Page {0} of {1}", 
				(int) (_pgs.PageCount * (long) _vScroll.Value / (double) _vScroll.Maximum)+1, 
				_pgs.PageCount);
			
			_vScrollToolTip.SetToolTip(_vScroll, tt);

			_DrawPanel.Invalidate();
        }
        #endregion

        private void DrawPanelMouseWheel(object sender, MouseEventArgs e)
		{
			int wvalue;
            bool bCtrlOn = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            if (bCtrlOn)
            {   // when ctrl key on and wheel rotated we zoom in or out
                float zoom = Zoom;

                if (e.Delta < 0)
                {
                    zoom -= .1f;
                    if (zoom < .1f)
                        zoom = .1f;
                }
                else
                {
                    zoom += .1f;
                    if (zoom > 10)
                        zoom = 10;
                }
                Zoom = zoom;
                _DrawPanel.Refresh();
                return;
            }

			if (e.Delta < 0)
			{
				if (_vScroll.Value < _vScroll.Maximum)
				{
					wvalue = _vScroll.Value + _vScroll.SmallChange;

					//_vScroll.Value = (int) Math.Min(_vScroll.Maximum - (_DrawPanel.Height / _zoom), wvalue);
                    //Changed from forum, User: robertopisati http://www.fyireporting.com/forum/viewtopic.php?t=863
                    float value = Math.Min(_vScroll.Maximum - (_DrawPanel.Height / _zoom), wvalue);
                    _vScroll.Value = (int)Math.Max(_vScroll.Minimum, value);
					_DrawPanel.Refresh();
				}
			}
			else 
			{
				if (_vScroll.Value > _vScroll.Minimum)
				{
					wvalue = _vScroll.Value - _vScroll.SmallChange;

					_vScroll.Value = Math.Max(_vScroll.Minimum, wvalue);
					_DrawPanel.Refresh();
				}
			}
		}

		private void DrawPanelKeyDown(object sender, KeyEventArgs e)
		{
			// Force scroll up and down
			if (e.KeyCode == Keys.Down)
			{
                if (!_vScroll.Enabled)
                    return;
                int wvalue = _vScroll.Value + _vScroll.SmallChange;

                _vScroll.Value = (int)Math.Min(_vScroll.Maximum - (_DrawPanel.Height / _zoom), wvalue);
				_DrawPanel.Refresh();
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Up)
			{
                if (!_vScroll.Enabled)
                    return;
                _vScroll.Value = Math.Max(_vScroll.Value - _vScroll.SmallChange, 0);
				_DrawPanel.Refresh();
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.PageDown)
			{
                if (!_vScroll.Enabled)
                    return;
                _vScroll.Value = Math.Min(_vScroll.Value + _vScroll.LargeChange,
                                        _vScroll.Maximum - _DrawPanel.Height);
				_DrawPanel.Refresh();
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.PageUp)
			{
                if (!_vScroll.Enabled)
                    return;
                _vScroll.Value = Math.Max(_vScroll.Value - _vScroll.LargeChange, 0);
				_DrawPanel.Refresh();
				e.Handled = true;
			}
            else if (e.KeyCode == Keys.Home)
            {
                if (!_vScroll.Enabled)
                    return;
                _vScroll.Value = 0;
                _DrawPanel.Refresh();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.End)
            {
                if (!_vScroll.Enabled)
                    return;
                if (_pgs != null && _pgs.Count > 0)
                {
                    Page last = _pgs[_pgs.Count - 1];
                    if (last.Count > 0)
                    {
                        PageItem lastItem = last[last.Count - 1];
                        this.ScrollToPageItem(lastItem);
                        e.Handled = true;
                    }
                }
            }
            else if (e.KeyCode == Keys.Left)
            {
                if (!_hScroll.Enabled)
                    return;
                if (e.Control)
                    _hScroll.Value = 0;
                else
                    _hScroll.Value = Math.Max(_hScroll.Value - _hScroll.SmallChange, 0);
                _DrawPanel.Refresh();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (!_hScroll.Enabled)
                    return;
                if (e.Control)
                    _hScroll.Value = _hScroll.Maximum - _DrawPanel.Width;
                else
                    _hScroll.Value = Math.Min(_hScroll.Value + _hScroll.SmallChange,
                                                _hScroll.Maximum - _DrawPanel.Width);
                _DrawPanel.Refresh();
                e.Handled = true;
            }

		}

		private bool WarningVisible()
		{
			if (!_ShowParameters)
				return false;

			return _errorMsgs != null;
		}

		private void RdlViewer_Layout(object sender, LayoutEventArgs e)
		{
            this.SuspendLayout();
			//int fHeight = _FindCtl.Visible? _FindCtl.Height: 0;
            //int pHeight;
            _ParameterPanel.Visible = _ShowParameters;
            _ParameterPanel.SendToBack();
            _FindCtl.SendToBack();
            //if (_ShowParameters)
			//{	// Only the parameter panel is visible
			//	_ParameterPanel.Visible = true;
				//_RunButton.Visible = true;
				
				//_WarningButton.Visible = WarningVisible();

				//_ParameterPanel.Location = new Point(0,0);
                //_ParameterPanel.Width = this.Width; //- _RunButton.Width - _WarningButton.Width - 5;
                //pHeight = _ParameterPanel.Height;// + 15; //this.Height / 3;
                //if (pHeight > _ParametersMaxHeight)
                //    pHeight = _ParametersMaxHeight;
                //if (pHeight < _RunButton.Height + 15)
                //    pHeight = _RunButton.Height + 15;
				//_ParameterPanel.Height = pHeight;
			//}
			//else
			//{
//				pHeight=_RunButton.Height + 15;
				//pHeight=0;
				//_RunButton.Visible = false;
				//_WarningButton.Visible = false;
			//	_ParameterPanel.Visible = false;
			//}
			//_DrawPanel.Location = new Point(0, pHeight);
			//_DrawPanel.Width = this.Width - _vScroll.Width;
			//_DrawPanel.Height = this.Height - _hScroll.Height - pHeight - fHeight;
			//_hScroll.Location = new Point(0, this.Height - _hScroll.Height - fHeight);
			//_hScroll.Width = _DrawPanel.Width;
			//_vScroll.Location = new Point(this.Width - _vScroll.Width, _DrawPanel.Location.Y);
			//_vScroll.Height = _DrawPanel.Height;

            //if (_FindCtl.Visible)
            //{
                //_FindCtl.Location = new Point(0, this.Height - _FindCtl.Height);
                //_FindCtl.Width = this.Width;
                
            //}

			//_RunButton.Location = new Point(this.Width - _RunButton.Width - 2 - _WarningButton.Width, 10);
			//_WarningButton.Location = new Point(_RunButton.Location.X + _RunButton.Width + 2, 13);
            this.ResumeLayout();
		}

        internal void InvokeHyperlink(HyperlinkEventArgs hlea)
        {
            if (Hyperlink != null)
                Hyperlink(this, hlea);
        }

        private void _FindCtl_Closed(object sender, EventArgs e)
        {
            
        }
    }
}
