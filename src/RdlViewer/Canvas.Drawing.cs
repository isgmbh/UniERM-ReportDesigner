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
using System.Collections;
using System.Drawing.Drawing2D;
using System.IO;
using Reporting.Rdl;
using Reporting.Viewer;
using Reporting.Rdl.Utility;


namespace Reporting.Viewer.Canvas.Drawing
{
    /// <summary>
    /// This class contains the methods used to draw to a Canvas object.
    /// </summary>
    public static class CanvasPainter
    {
        #region Drawing methods

        /// <summary>
        /// A simple draw of an entire page.  Useful when printing or creating an image.
        /// </summary>
        public static void Draw(Graphics g, int page, Pages pages, Rectangle clipRectangle, bool drawBackground, ref CanvasProperties cp, ref PageItemManager cpim)
        {
            cp.Dpi = new PointF(g.DpiX, g.DpiY);			 // this can change (e.g. printing graphics context)
            cp.HighlightText = null;
            cp.HighlightItem = null;
            cp.HighlightAll = false;
            cp.HighlightCaseSensitive = false;

            //			g.InterpolationMode = InterpolationMode.HighQualityBilinear;	// try to unfuzz charts
            g.PageUnit = GraphicsUnit.Pixel;
            g.ScaleTransform(1, 1);

            if (!cp.Offset.IsEmpty)    // used when correcting for non-printable area on paper
            {
                g.TranslateTransform(cp.Offset.X, cp.Offset.Y);
            }

            cp.Left = 0;
            cp.Top = 0;
            cp.Scroll = new PointF(0, 0);

            RectangleF r = new RectangleF(clipRectangle.X, clipRectangle.Y,
                                            clipRectangle.Width, clipRectangle.Height);

            if (drawBackground)
                g.FillRectangle(
                    Brushes.White,
                    Measurement.PixelsFromPoints(cp.Left, cp.Dpi.X),
                    Measurement.PixelsFromPoints(cp.Top, cp.Dpi.Y),
                    Measurement.PixelsFromPoints(pages.PageWidth, cp.Dpi.X),
                    Measurement.PixelsFromPoints(pages.PageHeight, cp.Dpi.Y));

            ProcessPage(g, pages[page], r, false, cp, ref cpim);
        }

        /// <summary>
        /// A draw accounting for scrolling and zoom factors.
        /// </summary>
        public static void Draw(Graphics g, Pages pages, float pageGap, Rectangle clipRectangle,
                                ref CanvasProperties cp, ref PageItemManager cpim)
        {
            if (pages == null)
            {	// No pages; means nothing to draw
                g.FillRectangle(Brushes.White, clipRectangle);
                return;
            }

            g.PageUnit = GraphicsUnit.Pixel;
            g.ScaleTransform(cp.Zoom, cp.Zoom);
            cp.Dpi = new PointF(g.DpiX, g.DpiY);

            // Zoom affects how much will show on the screen.  Adjust our perceived clipping rectangle
            //  to account for it.
            RectangleF r;
            r = new RectangleF((clipRectangle.X) / cp.Zoom, (clipRectangle.Y) / cp.Zoom,
                (clipRectangle.Width) / cp.Zoom, (clipRectangle.Height) / cp.Zoom);

            // Calculate the top of the page
            int fpage = (int)(cp.Scroll.Y / Measurement.PixelsFromPoints(pages.PageHeight + pageGap, cp.Dpi.Y));
            int lpage = (int)((cp.Scroll.Y + r.Height) / Measurement.PixelsFromPoints(pages.PageHeight + pageGap, cp.Dpi.X)) + 1;
            if (fpage >= pages.PageCount)
                return;
            if (lpage >= pages.PageCount)
                lpage = pages.PageCount - 1;

            cp.Left = cp.Offset.X;
            cp.Top = pageGap;
            // Loop thru the visible pages
            for (int p = fpage; p <= lpage; p++)
            {
                cp.Scroll = new PointF(
                                cp.Scroll.X, 
                                (cp.Scroll.Y - Measurement.PixelsFromPoints(p * (pages.PageHeight + pageGap), cp.Dpi.Y))
                                );

                System.Drawing.Rectangle pr =
                    new System.Drawing.Rectangle(
                    (int)(Measurement.PixelsFromPoints(cp.Left, cp.Dpi.X) - cp.Scroll.X),
                    (int)(Measurement.PixelsFromPoints(cp.Top, cp.Dpi.Y) - cp.Scroll.Y),
                    (int)Measurement.PixelsFromPoints(pages.PageWidth, cp.Dpi.X),
                    (int)Measurement.PixelsFromPoints(pages.PageHeight, cp.Dpi.Y));
                g.FillRectangle(Brushes.White, pr);

                ProcessPage(g, pages[p], r, true, cp, ref cpim);

                // Draw the page outline
                using (Pen pn = new Pen(Brushes.Black, 1))
                {
                    int z3 = Math.Min((int)(3f / cp.Zoom), 3);
                    if (z3 <= 0)
                        z3 = 1;
                    int z4 = Math.Min((int)(4f / cp.Zoom), 4);
                    if (z4 <= 0)
                        z4 = 1;
                    g.DrawRectangle(pn, pr);					// outline of page
                    g.FillRectangle(Brushes.Black,
                        pr.X + pr.Width, pr.Y + z3, z3, pr.Height);		// right side of page
                    g.FillRectangle(Brushes.Black,
                        pr.X + z3, pr.Y + pr.Height, pr.Width, z4);		// bottom of page
                }
            }
        }


        /// <summary>
        /// Renders all the objects in a Page or composite object.
        /// </summary>
        private static void ProcessPage(Graphics g, IEnumerable p, RectangleF clipRect, bool bHitList, CanvasProperties cp, ref PageItemManager cpim)
        {
            foreach (PageItem pi in p)
            {
                if (pi is PageTextHtml)
                {	// PageTextHtml is actually a composite object (just like a page)
                    if (cpim.SelectToolEnabled && bHitList)
                    {
                        RectangleF hr = new RectangleF(
                            Measurement.PixelsFromPoints(pi.X + cp.Left - cp.Scroll.X, cp.Dpi.X),
                            Measurement.PixelsFromPoints(pi.Y + cp.Scroll.X - cp.Scroll.Y, cp.Dpi.Y),
                            Measurement.PixelsFromPoints(pi.W, cp.Dpi.X),
                            Measurement.PixelsFromPoints(pi.H, cp.Dpi.Y));

                        cpim.HitList.Add(new HitListEntry(hr, pi));
                    }
                    ProcessHtml(pi as PageTextHtml, g, clipRect, bHitList, cp, ref cpim);
                    continue;
                }

                if (pi is PageLine)
                {
                    PageLine pl = pi as PageLine;
                    CanvasPainter.DrawLine(
                        pl.SI.BColorLeft,
                        pl.SI.BStyleLeft,
                        pl.SI.BWidthLeft,
                        g,
                        Measurement.PixelsFromPoints(pl.X + cp.Left - cp.Scroll.X, cp.Dpi.X),
                        Measurement.PixelsFromPoints(pl.Y + cp.Top - cp.Scroll.Y, cp.Dpi.Y),
                        Measurement.PixelsFromPoints(pl.X2 + cp.Left - cp.Scroll.X, cp.Dpi.X),
                        Measurement.PixelsFromPoints(pl.Y2 + cp.Top - cp.Scroll.Y, cp.Dpi.Y));
                    continue;
                }


                RectangleF rect = new RectangleF(
                    Measurement.PixelsFromPoints(pi.X + cp.Left - cp.Scroll.X, cp.Dpi.X),
                    Measurement.PixelsFromPoints(pi.Y + cp.Top - cp.Scroll.Y, cp.Dpi.Y),
                    Measurement.PixelsFromPoints(pi.W, cp.Dpi.X),
                    Measurement.PixelsFromPoints(pi.H, cp.Dpi.Y));

                // Maintain the hit list
                if (bHitList)
                {
                    if (cpim.SelectToolEnabled)
                    {   // we need all PageText and PageImage items that have been displayed
                        if (pi is PageText || pi is PageImage)
                        {
                            cpim.HitList.Add(new HitListEntry(rect, pi));
                        }
                    }
                    // Only care about items with links and tips
                    else if (pi.HyperLink != null || pi.BookmarkLink != null || pi.Tooltip != null)
                    {
                        HitListEntry hle;
                        if (pi is PagePolygon)
                            hle = new HitListEntry(pi as PagePolygon, cp.Left - cp.Scroll.X, cp.Top - cp.Scroll.Y, ((Canvas)cp.Parent));
                        else
                            hle = new HitListEntry(rect, pi);
                        cpim.HitList.Add(hle);
                    }
                }

                if ((pi is PagePolygon) || (pi is PageCurve))
                { // intentionally empty; polygon's rectangles aren't calculated
                }
                else if (!rect.IntersectsWith(clipRect))
                    continue;

                if (pi.SI.BackgroundImage != null)
                {	// put out any background image
                    PageImage i = pi.SI.BackgroundImage;
                    CanvasPainter.DrawImageBackground(i, pi.SI, g, rect);
                }

                if (pi is PageText)
                {
                    PageText pt = pi as PageText;
                    CanvasPainter.DrawString(pt, g, rect, cp, cpim);
                }
                else if (pi is PageImage)
                {
                    PageImage i = pi as PageImage;
                    CanvasPainter.DrawImage(i, g, rect, cp, cpim);
                }
                else if (pi is PageRectangle)
                {
                    CanvasPainter.DrawBackground(g, rect, pi.SI);
                }
                else if (pi is PageEllipse)
                {
                    PageEllipse pe = pi as PageEllipse;
                    CanvasPainter.DrawEllipse(pe, g, rect);
                }
                else if (pi is PagePie)
                {
                    PagePie pp = pi as PagePie;
                    CanvasPainter.DrawPie(pp, g, rect);
                }
                else if (pi is PagePolygon)
                {
                    PagePolygon ppo = pi as PagePolygon;
                    CanvasPainter.FillPolygon(ppo, g, rect, cp);
                }
                else if (pi is PageCurve)
                {
                    PageCurve pc = pi as PageCurve;
                    CanvasPainter.DrawCurve(pc.SI.BColorLeft, pc.SI.BStyleLeft, pc.SI.BWidthLeft,
                        g, pc.Points, pc.Offset, pc.Tension, cp);
                }


                CanvasPainter.DrawBorder(pi, g, rect);
            }
        }
        /// <summary>
        /// Pre-Processes the HTML prior to rendering it in a page.
        /// </summary>
        public static void ProcessHtml(PageTextHtml pth, Graphics g, RectangleF clipRect, bool bHitList, CanvasProperties cp, ref PageItemManager cpim)
        {
            pth.Build(g);				// Builds the subobjects that make up the html
            CanvasPainter.ProcessPage(g, pth, clipRect, bHitList, cp, ref cpim);
        }
        /// <summary>
        /// Parses out the text from the HTML selected form a web page.
        /// </summary>
        public static void SelectTextHtml(PageTextHtml ph, StringBuilder sb)
        {
            bool bFirst = true;
            float lastY = float.MaxValue;
            foreach (PageItem pi in ph)
            {
                if (bFirst)                 // we ignore the contents of the first item
                {
                    bFirst = false;
                    continue;
                }
                PageText pt = pi as PageText;
                if (pt == null)
                    continue;
                if (pt.Y > lastY)           // we've gone to a new line; put a blank in between the text
                    sb.Append(' ');         //   this isn't always ideal: if user was just selecting html text
                //   then they might want to retain the new lines; but when selecting
                //   html page items and other page items new lines affect the table building

                sb.Append(pt.Text);         // append on this text
                lastY = pt.Y;
            }
            return;
        }

        #region Internal Static
        private static void DrawBackground(Graphics g, System.Drawing.RectangleF rect, StyleInfo si)
        {
            LinearGradientBrush linGrBrush = null;
            SolidBrush sb = null;
            HatchBrush hb = null;
            try
            {
                if (si.BackgroundGradientType != BackgroundGradientTypeEnum.None &&
                    !si.BackgroundGradientEndColor.IsEmpty &&
                    !si.BackgroundColor.IsEmpty)
                {
                    Color c = si.BackgroundColor;
                    Color ec = si.BackgroundGradientEndColor;

                    switch (si.BackgroundGradientType)
                    {
                        case BackgroundGradientTypeEnum.LeftRight:
                            linGrBrush = new LinearGradientBrush(rect, c, ec, LinearGradientMode.Horizontal);
                            break;
                        case BackgroundGradientTypeEnum.TopBottom:
                            linGrBrush = new LinearGradientBrush(rect, c, ec, LinearGradientMode.Vertical);
                            break;
                        case BackgroundGradientTypeEnum.Center:
                            linGrBrush = new LinearGradientBrush(rect, c, ec, LinearGradientMode.Horizontal);
                            break;
                        case BackgroundGradientTypeEnum.DiagonalLeft:
                            linGrBrush = new LinearGradientBrush(rect, c, ec, LinearGradientMode.ForwardDiagonal);
                            break;
                        case BackgroundGradientTypeEnum.DiagonalRight:
                            linGrBrush = new LinearGradientBrush(rect, c, ec, LinearGradientMode.BackwardDiagonal);
                            break;
                        case BackgroundGradientTypeEnum.HorizontalCenter:
                            linGrBrush = new LinearGradientBrush(rect, c, ec, LinearGradientMode.Horizontal);
                            break;
                        case BackgroundGradientTypeEnum.VerticalCenter:
                            linGrBrush = new LinearGradientBrush(rect, c, ec, LinearGradientMode.Vertical);
                            break;
                        default:
                            break;
                    }
                }
                if (si.PatternType != patternTypeEnum.None)
                {
                    switch (si.PatternType)
                    {
                        case patternTypeEnum.BackwardDiagonal:
                            hb = new HatchBrush(HatchStyle.BackwardDiagonal, si.Color, si.BackgroundColor);
                            break;
                        case patternTypeEnum.CheckerBoard:
                            hb = new HatchBrush(HatchStyle.LargeCheckerBoard, si.Color, si.BackgroundColor);
                            break;
                        case patternTypeEnum.Cross:
                            hb = new HatchBrush(HatchStyle.Cross, si.Color, si.BackgroundColor);
                            break;
                        case patternTypeEnum.DarkDownwardDiagonal:
                            hb = new HatchBrush(HatchStyle.DarkDownwardDiagonal, si.Color, si.BackgroundColor);
                            break;
                        case patternTypeEnum.DarkHorizontal:
                            hb = new HatchBrush(HatchStyle.DarkHorizontal, si.Color, si.BackgroundColor);
                            break;
                        case patternTypeEnum.DiagonalBrick:
                            hb = new HatchBrush(HatchStyle.DiagonalBrick, si.Color, si.BackgroundColor);
                            break;
                        case patternTypeEnum.HorizontalBrick:
                            hb = new HatchBrush(HatchStyle.HorizontalBrick, si.Color, si.BackgroundColor);
                            break;
                        case patternTypeEnum.LargeConfetti:
                            hb = new HatchBrush(HatchStyle.LargeConfetti, si.Color, si.BackgroundColor);
                            break;
                        case patternTypeEnum.OutlinedDiamond:
                            hb = new HatchBrush(HatchStyle.OutlinedDiamond, si.Color, si.BackgroundColor);
                            break;
                        case patternTypeEnum.SmallConfetti:
                            hb = new HatchBrush(HatchStyle.SmallConfetti, si.Color, si.BackgroundColor);
                            break;
                        case patternTypeEnum.SolidDiamond:
                            hb = new HatchBrush(HatchStyle.SolidDiamond, si.Color, si.BackgroundColor);
                            break;
                        case patternTypeEnum.Vertical:
                            hb = new HatchBrush(HatchStyle.Vertical, si.Color, si.BackgroundColor);
                            break;
                    }
                }

                if (linGrBrush != null)
                {
                    g.FillRectangle(linGrBrush, rect);
                    linGrBrush.Dispose();
                }
                else if (hb != null)
                {
                    g.FillRectangle(hb, rect);
                    hb.Dispose();
                }
                else if (!si.BackgroundColor.IsEmpty)
                {
                    sb = new SolidBrush(si.BackgroundColor);
                    g.FillRectangle(sb, rect);
                    sb.Dispose();
                }
            }
            finally
            {
                if (linGrBrush != null)
                    linGrBrush.Dispose();
                if (sb != null)
                    sb.Dispose();
            }
            return;
        }

        private static void DrawBorder(PageItem pi, Graphics g, RectangleF r)
        {
            if (pi.GetType().Name.Equals("PagePie")) return;
            if (r.Height <= 0 || r.Width <= 0)		// no bounding box to use
                return;

            StyleInfo si = pi.SI;

            DrawLine(si.BColorTop, si.BStyleTop, si.BWidthTop, g, r.X, r.Y, r.Right, r.Y);

            DrawLine(si.BColorRight, si.BStyleRight, si.BWidthRight, g, r.Right, r.Y, r.Right, r.Bottom);

            DrawLine(si.BColorLeft, si.BStyleLeft, si.BWidthLeft, g, r.X, r.Y, r.X, r.Bottom);

            DrawLine(si.BColorBottom, si.BStyleBottom, si.BWidthBottom, g, r.X, r.Bottom, r.Right, r.Bottom);

            return;

        }

        private static void DrawImage(PageImage pi, Graphics g, RectangleF r, CanvasProperties cp, PageItemManager cpim)
        {
            Stream strm = null;
            System.Drawing.Image im = null;
            try
            {
                strm = new MemoryStream(pi.ImageData);
                im = Image.FromStream(strm);
                DrawImageSized(pi, im, g, r, cp, cpim);
            }
            finally
            {
                if (strm != null)
                    strm.Close();
                if (im != null)
                    im.Dispose();
            }

        }
        private static void DrawImageBackground(PageImage pi, StyleInfo si, Graphics g, RectangleF r)
        {
            Stream strm = null;
            System.Drawing.Image im = null;
            try
            {
                strm = new MemoryStream(pi.ImageData);
                im = System.Drawing.Image.FromStream(strm);

                //RectangleF r2 = new RectangleF(r.Left + PixelsX(si.PaddingLeft),
                //    r.Top + PixelsY(si.PaddingTop),
                //    r.Width - PixelsX(si.PaddingLeft + si.PaddingRight),
                //    r.Height - PixelsY(si.PaddingTop + si.PaddingBottom));
                // http://www.fyireporting.com/forum/viewtopic.php?t=892
                //A.S.> convert pt to px if needed(when printing we need px, when draw preview - pt)
                RectangleF r2;
                if (g.PageUnit == GraphicsUnit.Pixel)
                {
                    r2 = new RectangleF(r.Left + (si.PaddingLeft * g.DpiX) / 72,
                        r.Top + (si.PaddingTop * g.DpiX) / 72,
                        r.Width - ((si.PaddingLeft + si.PaddingRight) * g.DpiX) / 72,
                        r.Height - ((si.PaddingTop + si.PaddingBottom) * g.DpiX) / 72);
                }
                else
                {
                    // adjust drawing rectangle based on padding
                    r2 = new RectangleF(r.Left + si.PaddingLeft,
                        r.Top + si.PaddingTop,
                        r.Width - si.PaddingLeft - si.PaddingRight,
                        r.Height - si.PaddingTop - si.PaddingBottom);
                }

                int repeatX = 0;
                int repeatY = 0;
                switch (pi.Repeat)
                {
                    case ImageRepeat.Repeat:
                        repeatX = (int)Math.Floor(r2.Width / pi.SamplesW);
                        repeatY = (int)Math.Floor(r2.Height / pi.SamplesH);
                        break;
                    case ImageRepeat.RepeatX:
                        repeatX = (int)Math.Floor(r2.Width / pi.SamplesW);
                        repeatY = 1;
                        break;
                    case ImageRepeat.RepeatY:
                        repeatY = (int)Math.Floor(r2.Height / pi.SamplesH);
                        repeatX = 1;
                        break;
                    case ImageRepeat.NoRepeat:
                    default:
                        repeatX = repeatY = 1;
                        break;
                }

                //make sure the image is drawn at least 1 times 
                repeatX = Math.Max(repeatX, 1);
                repeatY = Math.Max(repeatY, 1);

                float startX = r2.Left;
                float startY = r2.Top;

                Region saveRegion = g.Clip;
                Region clipRegion = new Region(g.Clip.GetRegionData());
                clipRegion.Intersect(r2);
                g.Clip = clipRegion;

                for (int i = 0; i < repeatX; i++)
                {
                    for (int j = 0; j < repeatY; j++)
                    {
                        float currX = startX + i * pi.SamplesW;
                        float currY = startY + j * pi.SamplesH;
                        g.DrawImage(im, new RectangleF(currX, currY, pi.SamplesW, pi.SamplesH));
                    }
                }
                g.Clip = saveRegion;
            }
            finally
            {
                if (strm != null)
                    strm.Close();
                if (im != null)
                    im.Dispose();
            }
        }
        private static void DrawImageSized(PageImage pi, Image im, Graphics g, RectangleF r, CanvasProperties cp, PageItemManager cpim)
        {
            float height, width;		// some work variables
            StyleInfo si = pi.SI;

            // adjust drawing rectangle based on padding
            //RectangleF r2 = new RectangleF(r.Left + PixelsX(si.PaddingLeft),
            //    r.Top + PixelsY(si.PaddingTop),
            //    r.Width - PixelsX(si.PaddingLeft + si.PaddingRight),
            //    r.Height - PixelsY(si.PaddingTop + si.PaddingBottom));
            // http://www.fyireporting.com/forum/viewtopic.php?t=892
            //A.S.> convert pt to px if needed(when printing we need px, when draw preview - pt)
            RectangleF r2;
            if (g.PageUnit == GraphicsUnit.Pixel)
            {
                r2 = new RectangleF(r.Left + (si.PaddingLeft * g.DpiX) / 72,
                    r.Top + (si.PaddingTop * g.DpiX) / 72,
                    r.Width - ((si.PaddingLeft + si.PaddingRight) * g.DpiX) / 72,
                    r.Height - ((si.PaddingTop + si.PaddingBottom) * g.DpiX) / 72);
            }
            else
            {
                // adjust drawing rectangle based on padding
                r2 = new RectangleF(r.Left + si.PaddingLeft,
                    r.Top + si.PaddingTop,
                    r.Width - si.PaddingLeft - si.PaddingRight,
                    r.Height - si.PaddingTop - si.PaddingBottom);
            }

            Rectangle ir;	// int work rectangle
            ir = new Rectangle(Convert.ToInt32(r2.Left), Convert.ToInt32(r2.Top),
                               Convert.ToInt32(r2.Width), Convert.ToInt32(r2.Height));
            switch (pi.Sizing)
            {
                case ImageSizingEnum.AutoSize:
                    // Note: GDI+ will stretch an image when you only provide
                    //  the left/top coordinates.  This seems pretty stupid since
                    //  it results in the image being out of focus even though
                    //  you don't want the image resized.
                    if (g.DpiX == im.HorizontalResolution &&
                        g.DpiY == im.VerticalResolution)
                    {
                        ir = new Rectangle(Convert.ToInt32(r2.Left), Convert.ToInt32(r2.Top),
                                                        im.Width, im.Height);
                    }
                    g.DrawImage(im, ir);

                    break;
                case ImageSizingEnum.Clip:
                    Region saveRegion = g.Clip;
                    Region clipRegion = new Region(g.Clip.GetRegionData());
                    clipRegion.Intersect(r2);
                    g.Clip = clipRegion;
                    if (g.DpiX == im.HorizontalResolution &&
                        g.DpiY == im.VerticalResolution)
                    {
                        ir = new Rectangle(Convert.ToInt32(r2.Left), Convert.ToInt32(r2.Top),
                                                        im.Width, im.Height);
                    }
                    g.DrawImage(im, ir);
                    g.Clip = saveRegion;
                    break;
                case ImageSizingEnum.FitProportional:
                    float ratioIm = (float)im.Height / (float)im.Width;
                    float ratioR = r2.Height / r2.Width;
                    height = r2.Height;
                    width = r2.Width;
                    if (ratioIm > ratioR)
                    {	// this means the rectangle width must be corrected
                        width = height * (1 / ratioIm);
                    }
                    else if (ratioIm < ratioR)
                    {	// this means the ractangle height must be corrected
                        height = width * ratioIm;
                    }
                    r2 = new RectangleF(r2.X, r2.Y, width, height);
                    g.DrawImage(im, r2);
                    break;
                case ImageSizingEnum.Fit:
                default:
                    g.DrawImage(im, r2);
                    break;
            }

            if (cpim.SelectToolEnabled && pi.AllowSelect && cpim.SelectedItemList.Contains(pi))
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(50, cp.SelectItemColor)), ir);
            }

            return;
        }

        private static void DrawLine(Color c, BorderStyleEnum bs, float w, Graphics g,
                                float x, float y, float x2, float y2)
        {
            if (bs == BorderStyleEnum.None || c.IsEmpty || w <= 0)	// nothing to draw
                return;

            Pen p = null;
            try
            {

                //Changed from forum, user :Aleksey http://www.fyireporting.com/forum/viewtopic.php?t=892
                float tmpW = w;
                if (g.PageUnit == GraphicsUnit.Pixel)
                    tmpW = (tmpW * g.DpiX) / 72;
                p = new Pen(c, tmpW);
                switch (bs)
                {
                    case BorderStyleEnum.Dashed:
                        p.DashStyle = DashStyle.Dash;
                        break;
                    case BorderStyleEnum.Dotted:
                        p.DashStyle = DashStyle.Dot;
                        break;
                    case BorderStyleEnum.Double:
                    case BorderStyleEnum.Groove:
                    case BorderStyleEnum.Inset:
                    case BorderStyleEnum.Solid:
                    case BorderStyleEnum.Outset:
                    case BorderStyleEnum.Ridge:
                    case BorderStyleEnum.WindowInset:
                    default:
                        p.DashStyle = DashStyle.Solid;
                        break;
                }

                g.DrawLine(p, x, y, x2, y2);
            }
            finally
            {
                if (p != null)
                    p.Dispose();
            }

        }

        private static void DrawCurve(Color c, BorderStyleEnum bs, float w, Graphics g,
                                PointF[] points, int Offset, float Tension, CanvasProperties cp)
        {
            if (bs == BorderStyleEnum.None || c.IsEmpty || w <= 0)	// nothing to draw
                return;

            Pen p = null;
            try
            {
                p = new Pen(c, w);
                switch (bs)
                {
                    case BorderStyleEnum.Dashed:
                        p.DashStyle = DashStyle.Dash;
                        break;
                    case BorderStyleEnum.Dotted:
                        p.DashStyle = DashStyle.Dot;
                        break;
                    case BorderStyleEnum.Double:
                    case BorderStyleEnum.Groove:
                    case BorderStyleEnum.Inset:
                    case BorderStyleEnum.Solid:
                    case BorderStyleEnum.Outset:
                    case BorderStyleEnum.Ridge:
                    case BorderStyleEnum.WindowInset:
                    default:
                        p.DashStyle = DashStyle.Solid;
                        break;
                }
                PointF[] tmp = new PointF[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    tmp[i] = Measurement.PixelsFromPoints(points[i].X + cp.Left - cp.Scroll.X,
                                                        points[i].Y + cp.Top - cp.Scroll.Y,
                                                        cp.Dpi);
                }

                g.DrawCurve(p, tmp, Offset, tmp.Length - 1, Tension);
            }
            finally
            {
                if (p != null)
                    p.Dispose();
            }

        }

        private static void DrawEllipse(PageEllipse pe, Graphics g, RectangleF r)
        {
            StyleInfo si = pe.SI;
            if (!si.BackgroundColor.IsEmpty)
            {
                g.FillEllipse(new SolidBrush(si.BackgroundColor), r);
            }
            if (si.BStyleTop != BorderStyleEnum.None)
            {
                Pen p = new Pen(si.BColorTop, si.BWidthTop);
                switch (si.BStyleTop)
                {
                    case BorderStyleEnum.Dashed:
                        p.DashStyle = DashStyle.Dash;
                        break;
                    case BorderStyleEnum.Dotted:
                        p.DashStyle = DashStyle.Dot;
                        break;
                    case BorderStyleEnum.Double:
                    case BorderStyleEnum.Groove:
                    case BorderStyleEnum.Inset:
                    case BorderStyleEnum.Solid:
                    case BorderStyleEnum.Outset:
                    case BorderStyleEnum.Ridge:
                    case BorderStyleEnum.WindowInset:
                    default:
                        p.DashStyle = DashStyle.Solid;
                        break;
                }
                g.DrawEllipse(p, r);
            }
        }

        private static void FillPolygon(PagePolygon pp, Graphics g, RectangleF r, CanvasProperties cp)
        {

            StyleInfo si = pp.SI;
            PointF[] tmp = new PointF[pp.Points.Length];
            if (!si.BackgroundColor.IsEmpty)
            //RectangleF(PixelsX(pi.X + _left - _hScroll), PixelsY(pi.Y + _top - _vScroll), 
            //                                                                    PixelsX(pi.W), PixelsY(pi.H))           
            {
                for (int i = 0; i < pp.Points.Length; i++)
                {
                    tmp[i] = Measurement.PixelsFromPoints(pp.Points[i].X + cp.Left - cp.Scroll.X,
                                                        pp.Points[i].Y + cp.Top - cp.Scroll.Y,
                                                        cp.Dpi);
                }
                g.FillPolygon(new SolidBrush(si.BackgroundColor), tmp);
            }
        }

        private static void DrawPie(PagePie pp, Graphics g, RectangleF r)
        {
            StyleInfo si = pp.SI;
            if (!si.BackgroundColor.IsEmpty)
            {
                g.FillPie(new SolidBrush(si.BackgroundColor), (int)r.X, (int)r.Y, (int)r.Width, (int)r.Height, (float)pp.StartAngle, (float)pp.SweepAngle);
            }

            if (si.BStyleTop != BorderStyleEnum.None)
            {
                Pen p = new Pen(si.BColorTop, si.BWidthTop);
                switch (si.BStyleTop)
                {
                    case BorderStyleEnum.Dashed:
                        p.DashStyle = DashStyle.Dash;
                        break;
                    case BorderStyleEnum.Dotted:
                        p.DashStyle = DashStyle.Dot;
                        break;
                    case BorderStyleEnum.Double:
                    case BorderStyleEnum.Groove:
                    case BorderStyleEnum.Inset:
                    case BorderStyleEnum.Solid:
                    case BorderStyleEnum.Outset:
                    case BorderStyleEnum.Ridge:
                    case BorderStyleEnum.WindowInset:
                    default:
                        p.DashStyle = DashStyle.Solid;
                        break;
                }
                g.DrawPie(p, r, pp.StartAngle, pp.SweepAngle);
            }
        }

        private static void DrawString(PageText pt, Graphics g, RectangleF r, CanvasProperties cp, PageItemManager cpim)
        {
            StyleInfo si = pt.SI;
            string s = pt.Text;

            Font drawFont = null;
            StringFormat drawFormat = null;
            Brush drawBrush = null;
            try
            {
                // STYLE
                System.Drawing.FontStyle fs = 0;
                if (si.FontStyle == FontStyleEnum.Italic)
                    fs |= System.Drawing.FontStyle.Italic;

                switch (si.TextDecoration)
                {
                    case TextDecorationEnum.Underline:
                        fs |= System.Drawing.FontStyle.Underline;
                        break;
                    case TextDecorationEnum.LineThrough:
                        fs |= System.Drawing.FontStyle.Strikeout;
                        break;
                    case TextDecorationEnum.Overline:
                    case TextDecorationEnum.None:
                        break;
                }

                // WEIGHT
                switch (si.FontWeight)
                {
                    case FontWeightEnum.Bold:
                    case FontWeightEnum.Bolder:
                    case FontWeightEnum.W500:
                    case FontWeightEnum.W600:
                    case FontWeightEnum.W700:
                    case FontWeightEnum.W800:
                    case FontWeightEnum.W900:
                        fs |= System.Drawing.FontStyle.Bold;
                        break;
                    default:
                        break;
                }
                try
                {
                    drawFont = new Font(si.GetFontFamily(), si.FontSize, fs);	// si.FontSize already in points
                }
                catch (ArgumentException)
                {
                    drawFont = new Font("Arial", si.FontSize, fs);	// if this fails we'll let the error pass thru
                }
                // ALIGNMENT
                drawFormat = new StringFormat();
                switch (si.TextAlign)
                {
                    case TextAlignEnum.Right:
                        drawFormat.Alignment = StringAlignment.Far;
                        break;
                    case TextAlignEnum.Center:
                        drawFormat.Alignment = StringAlignment.Center;
                        break;
                    case TextAlignEnum.Left:
                    default:
                        drawFormat.Alignment = StringAlignment.Near;
                        break;
                }
                if (pt.SI.WritingMode == WritingModeEnum.tb_rl)
                {
                    drawFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                    drawFormat.FormatFlags |= StringFormatFlags.DirectionVertical;
                }
                switch (si.VerticalAlign)
                {
                    case VerticalAlignEnum.Bottom:
                        drawFormat.LineAlignment = StringAlignment.Far;
                        break;
                    case VerticalAlignEnum.Middle:
                        drawFormat.LineAlignment = StringAlignment.Center;
                        break;
                    case VerticalAlignEnum.Top:
                    default:
                        drawFormat.LineAlignment = StringAlignment.Near;
                        break;
                }
                // draw the background 
                DrawBackground(g, r, si);

                // adjust drawing rectangle based on padding
                //RectangleF r2 = new RectangleF(r.Left + si.PaddingLeft,
                //                               r.Top + si.PaddingTop,
                //                               r.Width - si.PaddingLeft - si.PaddingRight,
                //                               r.Height - si.PaddingTop - si.PaddingBottom);
                // http://www.fyireporting.com/forum/viewtopic.php?t=892
                //A.S.> convert pt to px if needed(when printing we need px, when draw preview - pt)
                RectangleF r2;
                if (g.PageUnit == GraphicsUnit.Pixel)
                {
                    r2 = new RectangleF(r.Left + (si.PaddingLeft * g.DpiX) / 72,
                        r.Top + (si.PaddingTop * g.DpiX) / 72,
                        r.Width - ((si.PaddingLeft + si.PaddingRight) * g.DpiX) / 72,
                        r.Height - ((si.PaddingTop + si.PaddingBottom) * g.DpiX) / 72);
                }
                else
                {
                    // adjust drawing rectangle based on padding
                    r2 = new RectangleF(r.Left + si.PaddingLeft,
                        r.Top + si.PaddingTop,
                        r.Width - si.PaddingLeft - si.PaddingRight,
                        r.Height - si.PaddingTop - si.PaddingBottom);
                }

                drawBrush = new SolidBrush(si.Color);
                if (si.TextAlign == TextAlignEnum.Justified) //Added from forum: Hugo http://www.fyireporting.com/forum/viewtopic.php?t=552
                    GraphicsExtended.DrawStringJustified(g, pt.Text, drawFont, drawBrush, r2, '-');
                else if (pt.NoClip)	// request not to clip text
                {
                    g.DrawString(pt.Text, drawFont, drawBrush, new PointF(r.Left, r.Top), drawFormat);
                    HighlightString(g, pt, new RectangleF(r.Left, r.Top, float.MaxValue, float.MaxValue),
                        drawFont, drawFormat, cp);
                }
                else
                {
                    g.DrawString(pt.Text, drawFont, drawBrush, r2, drawFormat);
                    HighlightString(g, pt, r2, drawFont, drawFormat, cp);
                }
                if (cpim.SelectToolEnabled)
                {
                    if (pt.AllowSelect && cpim.SelectedItemList.Contains(pt))
                        g.FillRectangle(new SolidBrush(Color.FromArgb(50, cp.SelectItemColor)), r2);
                }
            }
            finally
            {
                if (drawFont != null)
                    drawFont.Dispose();
                if (drawFormat != null)
                    drawFont.Dispose();
                if (drawBrush != null)
                    drawBrush.Dispose();
            }
        }

        private static void HighlightString(Graphics g, PageText dtext, RectangleF r, Font f, StringFormat sf, CanvasProperties cp)
        {
            if (cp.HighlightText == null || cp.HighlightText.Length == 0)
                return;         // nothing to highlight
            bool bhighlightItem = dtext == cp.HighlightItem ||
                    (cp.HighlightItem != null && dtext.HtmlParent == cp.HighlightItem);
            if (!(cp.HighlightAll || bhighlightItem))
                return;         // not highlighting all and not on current highlight item

            string hlt = cp.HighlightCaseSensitive ? cp.HighlightText : cp.HighlightText.ToLower();
            string text = cp.HighlightCaseSensitive ? dtext.Text : dtext.Text.ToLower();

            if (text.IndexOf(hlt) < 0)
                return;         // string not in text

            StringFormat sf2 = null;
            try
            {
                // Create a CharacterRange array with the highlight location and length
                // Handle multiple occurences of text
                List<CharacterRange> rangel = new List<CharacterRange>();
                int loc = text.IndexOf(hlt);
                int hlen = hlt.Length;
                int len = text.Length;
                while (loc >= 0)
                {
                    rangel.Add(new CharacterRange(loc, hlen));
                    if (loc + hlen < len)  // out of range of text
                        loc = text.IndexOf(hlt, loc + hlen);
                    else
                        loc = -1;
                }

                if (rangel.Count <= 0)      // we should have gotten one; but
                    return;

                CharacterRange[] ranges = rangel.ToArray();

                // Construct a new StringFormat object.
                sf2 = sf.Clone() as StringFormat;

                // Set the ranges on the StringFormat object.
                sf2.SetMeasurableCharacterRanges(ranges);

                // Get the Regions to highlight by calling the 
                // MeasureCharacterRanges method.
                if (r.Width <= 0 || r.Height <= 0)
                {
                    SizeF ts = g.MeasureString(dtext.Text, f);
                    r.Height = ts.Height;
                    r.Width = ts.Width;
                }
                Region[] charRegion = g.MeasureCharacterRanges(dtext.Text, f, r, sf2);

                // Fill in the region using a semi-transparent color to highlight
                foreach (Region rg in charRegion)
                {
                    Color hl = bhighlightItem ? cp.HighlightItemColor : cp.HighlightAllColor;
                    g.FillRegion(new SolidBrush(Color.FromArgb(50, hl)), rg);
                }
            }
            catch { }   // if highlighting fails we don't care; need to continue
            finally
            {
                if (sf2 != null)
                    sf2.Dispose();
            }
        }

        #endregion
        #endregion

        /// <summary>
        /// A compare method that sorts page items by their XY coordinates.
        /// </summary>
        /// <returns>An integer indicating the relative location of one page item to the other.</returns>
        public static int ComparePageItemByPageXY(PageItem pi1, PageItem pi2)
        {
            if (pi1.Page.Count != pi2.Page.Count)
                return pi1.Page.Count - pi2.Page.Count;

            if (pi1.Y != pi2.Y)
            {
                return Convert.ToInt32((pi1.Y - pi2.Y) * 1000);
            }
            return Convert.ToInt32((pi1.X - pi2.X) * 1000);
        }
    }
}
