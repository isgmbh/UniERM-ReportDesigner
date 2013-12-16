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

namespace Reporting.Viewer.Canvas
{
    /// <summary>
    /// This class contains the common properties to the 
    /// canvas object. Many of these properties are used 
    /// in drawing and other functions as well.
    /// </summary>
    public class CanvasProperties
    {
        #region Constructor
        public CanvasProperties(Control parent)
        {
            _parent = parent;
        }

        public CanvasProperties(Control parent, Color highlightItemColor, Color highlightAllColor,
            Color selectItemColor) : this(parent)
        {
            _HighlightItemColor = highlightItemColor;
            _HighlightAllColor = HighlightAllColor;
            _SelectItemColor = selectItemColor;
        }
        #endregion

        #region Private variables
        private Control _parent = null;

        // During drawing these are set
        private float _left;
        private float _top;
        private PointF _scroll = new PointF(0,0);

        private PointF _offset = new PointF(0,0);

        // Used in Canvas MouseHandling
        private float _LastZoom;
        private float _zoom;

        private PointF _dpi = new PointF(96.0F, 96.0F);

        private PageItem _HighlightItem = null;
        private string _HighlightText = null;
        private bool _HighlightCaseSensitive = false;
        private bool _HighlightAll = false;
        private Color _HighlightItemColor = Color.Aqua;
        private Color _HighlightAllColor = Color.Fuchsia;
        private Color _SelectItemColor = Color.DarkBlue;
        #endregion

        #region Public properties
        /// <summary>
        /// The parent control of the CanvasProperties, typically a Canvas object.
        /// </summary>
        public Control Parent
        {
            get { return _parent; }
        }

        /// <summary>
        /// The left of the report area.
        /// </summary>
        public float Left
        {
            get { return _left; }
            set { _left = value; }
        }
        /// <summary>
        /// The top edge of the report.
        /// </summary>
        public float Top
        {
            get { return _top; }
            set { _top = value; }
        }
        /// <summary>
        /// The scroll value of the report. Scroll.X is the horizontal scroll value and Scroll.Y is the vertical scroll value.
        /// </summary>
        public PointF Scroll
        {
            get { return _scroll; }
            set { _scroll = value; }
        }

        /// <summary>
        /// The amount of offset used to draw the report. Offset.X if the left offset and Offset.Y is the top offset.
        /// </summary>
        public PointF Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        /// <summary>
        /// The last zoom level set.
        /// </summary>
        public float LastZoom
        {
            get { return _LastZoom; }
        }
        /// <summary>
        /// The current zoom level.
        /// </summary>
        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _LastZoom = _zoom;
                _zoom = value;
            }
        }

        /// <summary>
        /// The Dpi for the Canvas. The X and Y values relate to the DpiX and DpiY of a Graphics object.
        /// </summary>
        public PointF Dpi
        {
            get { return _dpi; }
            set
            {
                if (value.X <= 0)
                {
                    value.X = 96;
                }

                if (value.Y <= 0)
                {
                    value.Y = 96;
                }
                _dpi = value;
            }
        }

        /// <summary>
        /// The page item to highlight.
        /// </summary>
        public PageItem HighlightItem
        {
            get { return _HighlightItem; }
            set { _HighlightItem = value; }
        }
        /// <summary>
        /// The text to be highlighted.
        /// </summary>
        public string HighlightText
        {
            get { return _HighlightText; }
            set { _HighlightText = value; }
        }
        /// <summary>
        /// Indicates if the items should be highlighted on a case sensitive basis.
        /// </summary>
        public bool HighlightCaseSensitive
        {
            get { return _HighlightCaseSensitive; }
            set { _HighlightCaseSensitive = value; }
        }
        /// <summary>
        /// Indicates if all items should be highlighted when selected.
        /// </summary>
        public bool HighlightAll
        {
            get { return _HighlightAll; }
            set { _HighlightAll = value; }
        }
        /// <summary>
        /// The color used when selecting an item.
        /// </summary>
        public Color SelectItemColor
        {
            get { return _SelectItemColor; }
            set { _SelectItemColor = value; }
        }
        /// <summary>
        /// The color to use when highlighting the page item.
        /// </summary>
        public Color HighlightItemColor
        {
            get { return _HighlightItemColor; }
            set { _HighlightItemColor = value; }
        }
        /// <summary>
        /// The color to use for highlighting all.
        /// </summary>
        public Color HighlightAllColor
        {
            get { return _HighlightAllColor; }
            set { _HighlightAllColor = value; }
        }

        #endregion


    }
}
