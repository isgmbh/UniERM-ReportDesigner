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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using System.Text;
using Reporting.Rdl;
using Reporting.Viewer.Canvas.Drawing;
using Reporting.Rdl.Utility;


namespace Reporting.Viewer.Canvas
{
    /// <summary>
    /// Canvas represents the "canvas" the report is drawn on to.
    /// This class handles the interaction between the user and the 
    /// underlying report. This class is usually created
    /// from running a RDL file through the renderer.
    /// </summary>
    public class Canvas : UserControl
    {
        #region Internal Variables

        private Pages _pgs;					// the pages of the report to view

        /// <summary>
        /// An object containing the fields considered properties of a Canvas object, the Canvas contains the mouse handling and the actual pages.
        /// </summary>
        private CanvasProperties cp;
        /// <summary>
        /// An object containing the fields and methods neccessary to handle the selected items and the hit list.
        /// </summary>
        private PageItemManager cpim;
        

        // Selection handling
        bool _bHaveMouse = false;       // have we captured the mouse
        private Point _ptRBOriginal = new Point();	// starting position of the mouse (rubber banding)
        private Point _ptRBLast = new Point();		//   last position of mouse (rubber banding)
        private Point _MousePosition = new Point();		// position of the mouse

        // Mouse handling
        

        #endregion

        #region Constructor

        public Canvas() : this(null) { }

        /// <summary>
        /// A Canvas object contains the actual pages of the report.
        /// </summary>
        public Canvas(Pages pgs)
        {
            cp = new CanvasProperties(this);
            cpim = new PageItemManager(this);

            // Set up the tooltip
            cpim.ToolTip = new ToolTip();
            cpim.ToolTip.Active = false;
            cpim.ToolTip.ShowAlways = true;

            cpim.HitList = new List<HitListEntry>();
            cpim.SelectedItemList = new List<PageItem>();
            cp.Zoom = 1;

            _pgs = pgs;

            // Get our graphics DPI					   
            Graphics ga = null;
            try
            {
                ga = this.CreateGraphics();
                cp.Dpi = new PointF(ga.DpiX, ga.DpiY);
            }
            catch
            {
                cp.Dpi = new PointF(96.0F, 96.0F);
            }
            finally
            {
                if (ga != null)
                    ga.Dispose();
            }
            // force to double buffering for smoother drawing
            //this.SetStyle(ControlStyles.DoubleBuffer | 
            //    ControlStyles.UserPaint | 
            //    ControlStyles.AllPaintingInWmPaint,
            //    true);

            this.DoubleBuffered = true;
        }
        #endregion

        #region Internal Properties
        /// <summary>
        /// The pages of the report.
        /// </summary>
        internal Pages Pgs
        {
            get { return _pgs; }
            set { _pgs = value; }
        }

        //internal Color HighlightItemColor
        //{
        //    get { return cp.HighlightItemColor; }
        //    set { cp.HighlightItemColor = value; }
        //}
        //internal Color HighlightAllColor
        //{
        //    get { return cp.HighlightAllColor; }
        //    set { cp.HighlightAllColor = value; }
        //}

        /// <summary>
        /// The CanvasProperties of the report, containing fields related to the report and not specific pages or page items.
        /// </summary>
        internal CanvasProperties Properties
        {
            get { return cp; }
        }
        /// <summary>
        /// The PageItemManager used to keep track of and handle the events related to the selected items.
        /// </summary>
        internal PageItemManager ItemManager
        {
            get { return cpim; }
        }
        #endregion
        
        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Left ||
                keyData == Keys.Right ||
                keyData == Keys.Up ||
                keyData == Keys.Down ||
                keyData == Keys.Home ||
                keyData == Keys.End ||
                keyData == Keys.PageDown ||
                keyData == Keys.PageUp)
                return true;
            return base.IsInputKey(keyData);
        }

        /// <summary>
        /// A draw method accounting for scrolling and zoom factors.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="zoom"></param>
        /// <param name="leftOffset"></param>
        /// <param name="pageGap"></param>
        /// <param name="hScroll"></param>
        /// <param name="vScroll"></param>
        /// <param name="clipRectangle"></param>
        public void Draw(Graphics g, float zoom, float leftOffset, 
                        float pageGap, float hScroll, float vScroll,
                        Rectangle clipRectangle, PageItem highLightItem,
                        string highLight, bool highLightCaseSensitive, 
                        bool highLightAll)
        {

            cpim.HitList.Clear();			// remove all items from list

            // init for mouse handling
            cp.Zoom = zoom;
            cp.Offset = new PointF(leftOffset, cp.Offset.Y);
            cp.HighlightItem = highLightItem;
            cp.HighlightText = highLight;
            cp.HighlightCaseSensitive = highLightCaseSensitive;
            cp.HighlightAll = highLightAll;
            cp.Scroll = new PointF(hScroll, vScroll);

            CanvasPainter.Draw(g, _pgs, pageGap, clipRectangle, ref cp, ref cpim);
        }

        /// <summary>
        /// A simple, quick draw method.
        /// </summary>
        public void Draw(Graphics g, int page, Rectangle clipRectangle, 
                          bool drawBackground, PointF pageOffset)
        {
            cp.Offset = pageOffset;
            CanvasPainter.Draw(g, page, _pgs, clipRectangle, drawBackground, ref cp, ref cpim);
        }
        override protected void OnMouseDown(MouseEventArgs mea)
        {
            base.OnMouseDown(mea);			// allow base to process first
            _MousePosition = new Point(mea.X, mea.Y);

            if (MouseDownRubberBand(mea))
                return;

            HitListEntry hle = cpim.GetHitListEntry(mea, cp.Zoom);
            cpim.SetHitListCursor(hle);			// set the cursor based on the hit list entry

            if (mea.Button != MouseButtons.Left || hle == null)
                return;

            if (hle.PageItem.HyperLink != null)
            {
                RdlViewer rv = this.Parent as RdlViewer;
                bool bInvoke = true;
                if (rv != null)
                {
                    HyperlinkEventArgs hlea = new HyperlinkEventArgs(hle.PageItem.HyperLink);
                    rv.InvokeHyperlink(hlea);     // reset any mousemove
                    bInvoke = !hlea.Cancel;
                }
                try
                {
                    if (bInvoke)
                        System.Diagnostics.Process.Start(hle.PageItem.HyperLink);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Unable to link to {0}{1}{2}",
                        hle.PageItem.HyperLink, Environment.NewLine, ex.Message), "HyperLink Error");
                }
            }
        }

        private bool MouseDownRubberBand(MouseEventArgs e)
        {
            if (!cpim.SelectToolEnabled)
                return false;

            if (e.Button != MouseButtons.Left)
            {
                return true;		// well no rubber band but it's been handled
            }

            // We have a rubber band operation
            _bHaveMouse = true;
            // keep the starting point of the rectangular rubber band
            this._ptRBOriginal.X = e.X;
            this._ptRBOriginal.Y = e.Y;
            // -1 indicates no previous rubber band
            this._ptRBLast.X = this._ptRBLast.Y = -1;
            this.Cursor = Cursors.Cross;		// use cross hair to indicate drawing

            return true;
        }
        
        private void RubberBand(Point p1, Point p2)
        {
            // Convert the points to screen coordinates
            p1 = PointToScreen(p1);
            p2 = PointToScreen(p2);

            // Get a rectangle from the two points
            Rectangle rc = Measurement.RectFromPoints(p1, p2);

            // Draw reversibleFrame
            ControlPaint.DrawReversibleFrame(rc, Color.Black, FrameStyle.Dashed);

            return;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (!this._bHaveMouse)
                return;

            // Handle the end of the rubber banding
            _bHaveMouse = false;
            // remove last rectangle if necessary
            bool bCtrlOn = (Control.ModifierKeys & Keys.Control) == Keys.Control;
            if (this._ptRBLast.X != -1)
            {
                this.RubberBand(this._ptRBOriginal, this._ptRBLast);
                // Process the rectangle
                Rectangle r = Measurement.RectFromPoints(this._ptRBOriginal, this._ptRBLast);
                if (!bCtrlOn)	                // we allow addition to selection
                {
                    cpim.SelectedItemList.Clear();        // remove prior selection when ctrl key not on
                }
                cpim.CreateSelectionList(r, bCtrlOn, cp.Zoom);         // create the selection list
            }
            else
            {   // no last rectangle but use still might have clicked on a pageItem
                if (!bCtrlOn)
                    cpim.SelectedItemList.Clear();        // remove prior selection when ctrl key not on (even when no prior mouse movement)
                Rectangle r = Measurement.RectFromPoints(this._ptRBOriginal, this._ptRBOriginal);
                cpim.CreateSelectionList(r, bCtrlOn, cp.Zoom);         // create the selection list
            }
            // clear out the points for the next time
            _ptRBOriginal.X = _ptRBOriginal.Y = _ptRBLast.X = _ptRBLast.Y = -1;
            this.Invalidate();      // need to repaint since selection might have changed
        }

        override protected void OnMouseLeave(EventArgs ea)
        {
            cpim.ToolTip.Active = false;
        }

        override protected void OnMouseMove(MouseEventArgs mea)
        {
            if (cpim.SelectToolEnabled)
            {   // When selection we skip other cursor processing
                if (this.Cursor != Cursors.Cross)
                    this.Cursor = Cursors.Cross;

                if (!_bHaveMouse)
                    return;

                Point newMousePosition = new Point(mea.X, mea.Y);

                // we're rubber banding
                // If we drew previously; we'll draw again to remove old rectangle
                if (this._ptRBLast.X != -1)
                {
                    this.RubberBand(this._ptRBOriginal, _ptRBLast);
                }
                _MousePosition = newMousePosition;
                // Update last point;  but don't rubber band outside our client area
                if (newMousePosition.X < 0)
                    newMousePosition.X = 0;
                if (newMousePosition.X > this.Width)
                    newMousePosition.X = this.Width;
                if (newMousePosition.Y < 0)
                    newMousePosition.Y = 0;
                if (newMousePosition.Y > this.Height)
                    newMousePosition.Y = this.Height;
                _ptRBLast = newMousePosition;
                if (_ptRBLast.X < 0)
                    _ptRBLast.X = 0;
                if (_ptRBLast.Y < 0)
                    _ptRBLast.Y = 0;
                // Draw new lines.
                this.RubberBand(_ptRBOriginal, newMousePosition);
                this.Cursor = Cursors.Cross;		// use cross hair to indicate drawing
                return;
            }

            HitListEntry hle = cpim.GetHitListEntry(mea, cp.Zoom);
            cpim.SetHitListCursor(hle);
            cpim.SetHitListTooltip(hle);
            return;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Canvas
            // 
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Name = "Canvas";
            this.ResumeLayout(false);

        }
    }
}
