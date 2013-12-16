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

namespace Reporting.Viewer.Enums
{
    /// <summary>
    /// The method of scrolling to be used when viewing a report.
    /// </summary>
    public enum ScrollMode
    {
        /// <summary>
        /// Single page scrolling
        /// </summary>
        SinglePage,
        /// <summary>
        /// Continuous, single page scrolling
        /// </summary>
        Continuous,
        /// <summary>
        /// Two page (facing) scrolling
        /// </summary>
        Facing,
        /// <summary>
        /// Continuous, two page (facing) scrolling
        /// </summary>
        ContinuousFacing
    }

    /// <summary>
    /// The filter to use when searching for items.
    /// </summary>
    public enum ViewerSearchOptions
    {
        /// <summary>
        /// No additional search filters
        /// </summary>
        None = 0,
        /// <summary>
        /// A case sensitive search filter
        /// </summary>
        MatchCase = 1,
        /// <summary>
        /// Search in reverse order
        /// </summary>
        Backward = 2
    }

    /// <summary>
    /// The direction to search in.
    /// </summary>
    public enum ViewerSearchDirection
    {
        /// <summary>
        /// Search for the previous match
        /// </summary>
        Previous = 1,
        /// <summary>
        /// Search for the next match
        /// </summary>
        Next = 2,
    }

    /// <summary>
    /// The type of zoom to be used when viewing a report.
    /// </summary>
    public enum ZoomMode
    {
        /// <summary>
        /// Use the zoom amount when zooming
        /// </summary>
        UseZoom,
        /// <summary>
        /// Fit to the page height
        /// </summary>
        FitPage,
        /// <summary>
        /// Fit to the page width
        /// </summary>
        FitWidth
    }
}