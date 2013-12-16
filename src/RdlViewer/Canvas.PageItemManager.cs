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
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Reporting.Rdl;
using Reporting.Viewer;
using Reporting.Viewer.Canvas.Drawing;
using System.IO;
using Reporting.Rdl.Utility;

namespace Reporting.Viewer.Canvas
{
    /// <summary>
    /// This class contains the methods used to manage 
    /// the selected or "hit" items from the report.
    /// </summary>
    public class PageItemManager
    {
        private Control _parent = null;

        List<HitListEntry> _HitList;
        ToolTip _tt;
        bool _bSelect = false;              // use the selection tool
        private List<PageItem> _SelectList;


        /// <summary>
        /// The parent of this control. A Canvas object is typically the parent to a PageItemManager.
        /// </summary>
        public Control Parent
        {
            get { return _parent; }
        }

        /// <summary>
        /// The list of hit list entries indicating where the report was "hit" and which items were "hit".
        /// </summary>
        public List<HitListEntry> HitList
        {
            get { return _HitList; }
            set { _HitList = value; }
        }
        /// <summary>
        /// The tool tip related to the currenlty selected item or items.
        /// </summary>
        public ToolTip ToolTip
        {
            get { return _tt; }
            set { _tt = value; }
        }
        /// <summary>
        /// The list of selected items from the page.
        /// </summary>
        public List<PageItem> SelectedItemList
        {
            get { return _SelectList; }
            set { _SelectList = value; }
        }
        /// <summary>
        /// Enabling the SelectTool allows the user to select text and images.  Enabling or disabling
        /// the SelectTool also clears out the current selection.
        /// </summary>
        public bool SelectToolEnabled
        {
            get { return _bSelect; }
            set
            {
                _bSelect = value;
                _SelectList.Clear();        // clear out the selection list
                _parent.Invalidate();          // force repaint to create hitlist of items on screen
            }
        }

        /// <summary>
        /// Indicates if an item can be copied.
        /// </summary>
        /// <value>Returns true if items are selected and they can be copied.</value>
        public bool CanCopy
        {
            get
            {
                if (!SelectToolEnabled)
                    return false;
                return _SelectList.Count > 0;
            }
        }

        public PageItemManager(Control parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// When the only selected object is a PageImage then SelectImage holds
        /// the Image value; otherwise it will be null;
        /// </summary>
        public Image SelectImage
        {
            get
            {
                if (!SelectToolEnabled || _SelectList.Count != 1)
                    return null;
                PageImage pi = _SelectList[0] as PageImage;
                if (pi == null)
                    return null;

                Stream strm = null;
                System.Drawing.Image im = null;
                try
                {
                    strm = new MemoryStream(pi.ImageData);
                    im = Image.FromStream(strm);
                }
                finally
                {
                    if (strm != null)
                        strm.Close();
                }
                return im;
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
                if (!SelectToolEnabled || _SelectList.Count == 0)
                    return null;

                StringBuilder sb = new StringBuilder();
                float lastY = float.MinValue;

                _SelectList.Sort(CanvasPainter.ComparePageItemByPageXY);
                foreach (PageItem pi in _SelectList)
                {
                    PageText pt = pi as PageText;
                    if (pt == null)
                        continue;
                    if (pt.HtmlParent != null)              // if an underlying PageText is selected we'll get it through the PageTextHtml
                        continue;
                    if (lastY != float.MinValue)            // if first time then don't need separator
                    {
                        if (pt.Y == lastY)
                            sb.Append('\t');                // same line put in a tab between values
                        else
                            sb.Append(Environment.NewLine); // force a new line
                    }
                    if (pt is PageTextHtml)
                        CanvasPainter.SelectTextHtml(pt as PageTextHtml, sb);
                    else
                        sb.Append(pt.Text);
                    lastY = pt.Y;
                }
                return sb.ToString();
            }
        }
        
        public void CreateSelectionList(Rectangle rc, bool bCtrlOn, float zoom)
        {
            if (_HitList.Count <= 0)
                return;

            RectangleF rf = new RectangleF(rc.X / zoom, rc.Y / zoom, rc.Width / zoom, rc.Height / zoom);

            try
            {
                foreach (HitListEntry hle in this._HitList)
                {
                    if (!hle.AreaRectangle.IntersectsWith(rf))
                        continue;
                    bool bInList = _SelectList.Contains(hle.PageItem);
                    if (bCtrlOn)
                    {
                        if (bInList)       // When ctrl is on we allow user to deselect item in list
                            _SelectList.Remove(hle.PageItem);
                        else
                            _SelectList.Add(hle.PageItem);
                    }
                    else
                    {
                        if (!bInList)
                            _SelectList.Add(hle.PageItem);
                    }
                }
            }
            catch
            {
                // can get synchronization error due to multi-threading; just skip the error
            }
            return;
        }

        public void SetHitListCursor(HitListEntry hle)
        {
            Cursor c = Cursors.Default;
            if (hle == null)
            { }
            else if (hle.PageItem.HyperLink != null || hle.PageItem.BookmarkLink != null)
                c = Cursors.Hand;

            if (_parent.Cursor != c)
                _parent.Cursor = c;
        }

        public void SetHitListTooltip(HitListEntry hle)
        {
            if (hle == null || hle.PageItem.Tooltip == null)
                _tt.Active = false;
            else
            {
                _tt.SetToolTip(this.Parent, hle.PageItem.Tooltip);
                _tt.Active = true;
            }
        }

        public HitListEntry GetHitListEntry(MouseEventArgs mea, float zoom)
        {
            if (_HitList.Count <= 0)
                return null;

            PointF p = new PointF(mea.X / zoom, mea.Y / zoom);
            try
            {
                foreach (HitListEntry hle in this._HitList)
                {
                    if (hle.Contains(p))
                        return hle;
                }
            }
            catch
            {
                // can get synchronization error due to multi-threading; just skip the error
            }
            return null;
        }       
    }

    public class HitListEntry
    {
        private RectangleF rect;
        private PageItem pi;
        private PointF[] poly;

        /// <summary>
        /// The page item that was hit.
        /// </summary>
        public PageItem PageItem
        {
            get { return pi; }
            set { pi = value; }
        }
        /// <summary>
        /// The points in the polygon containing the hit page item.
        /// </summary>
        public PointF[] PolygonPoints
        {
            get { return poly; }
            set { poly = value; }
        }
        /// <summary>
        /// The rectangle that contains the hit page item.
        /// </summary>
        public RectangleF AreaRectangle
        {
            get { return rect; }
            set { rect = value; }
        }
        /// <summary>
        /// This class represents an entry into the hit list, the list containing the items that have been "hit" or selected.
        /// </summary>
        public HitListEntry(RectangleF r, PageItem pitem)
        {
            rect = r;
            pi = pitem;
            poly = null;
        }
        /// <summary>
        /// This class represents an entry into the hit list, the list containing the items that have been "hit" or selected.
        /// </summary>
        public HitListEntry(PagePolygon pp, float x, float y, Canvas pd)
        {
            pi = pp;
            poly = new PointF[pp.Points.Length];
            for (int i = 0; i < pp.Points.Length; i++)
            {
                poly[i].X = Measurement.PixelsFromPoints(pp.Points[i].X + x, pd.Properties.Dpi.X);
                poly[i].Y = Measurement.PixelsFromPoints(pp.Points[i].Y + y, pd.Properties.Dpi.Y);
            }
            rect = RectangleF.Empty;
        }
        /// <summary>
        /// Determines whether point in the pageitem
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Contains(PointF p)
        {
            return (pi is PagePolygon) ? IsPointInPolygon(p) : rect.Contains(p);
        }

        /// <summary>
        /// IsPointInPolygon: uses ray casting algorithm ( http://en.wikipedia.org/wiki/Point_in_polygon )
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool IsPointInPolygon(PointF p)
        {
            PointF p1, p2;
            bool bIn = false;
            if (poly.Length < 3)
            {
                return false;
            }

            PointF op = new PointF(poly[poly.Length - 1].X, poly[poly.Length - 1].Y);
            for (int i = 0; i < poly.Length; i++)
            {
                PointF np = new PointF(poly[i].X, poly[i].Y);
                if (np.X > op.X)
                {
                    p1 = op;
                    p2 = np;
                }
                else
                {
                    p1 = np;
                    p2 = op;
                }

                if ((np.X < p.X) == (p.X <= op.X)
                    && (p.Y - p1.Y) * (p2.X - p1.X) < (p2.Y - p1.Y) * (p.X - p1.X))
                {
                    bIn = !bIn;
                }
                op = np;
            }
            return bIn;
        }

    }
}
