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
using System.Xml;

namespace Reporting.Rdl
{
    /// CategoryGroupings definition and processing.
    ///</summary>
    [Serializable]
    internal class CategoryGroupings : ReportLink
    {
        List<CategoryGrouping> _Items;			// list of category groupings

        internal CategoryGroupings(ReportDefn r, ReportLink p, XmlNode xNode)
            : base(r, p)
        {
            CategoryGrouping cg;
            _Items = new List<CategoryGrouping>();
            // Loop thru all the child nodes
            foreach (XmlNode xNodeLoop in xNode.ChildNodes)
            {
                if (xNodeLoop.NodeType != XmlNodeType.Element)
                    continue;
                switch (xNodeLoop.Name)
                {
                    case "CategoryGrouping":
                        cg = new CategoryGrouping(r, this, xNodeLoop);
                        break;
                    default:
                        cg = null;		// don't know what this is
                        break;
                }
                if (cg != null)
                    _Items.Add(cg);
            }
            if (_Items.Count == 0)
                OwnerReport.rl.LogError(8, "For CategoryGroupings at least one CategoryGrouping is required.");
            else
                _Items.TrimExcess();
        }

        override internal void FinalPass()
        {
            foreach (CategoryGrouping cg in _Items)
            {
                cg.FinalPass();
            }
            return;
        }

        internal List<CategoryGrouping> Items
        {
            get { return _Items; }
        }
    }

    ///<summary>
    /// CategoryGrouping definition and processing.
    ///</summary>
    [Serializable]
    internal class CategoryGrouping : ReportLink
    {
        // A CategoryGrouping must have either DynamicCategories or StaticCategories
        //  but can't have both.
        DynamicCategories _DynamicCategories;	// Dynamic Category headings for this grouping
        StaticCategories _StaticCategories;		// Category headings for this grouping		

        internal CategoryGrouping(ReportDefn r, ReportLink p, XmlNode xNode)
            : base(r, p)
        {
            _DynamicCategories = null;
            _StaticCategories = null;

            // Loop thru all the child nodes
            foreach (XmlNode xNodeLoop in xNode.ChildNodes)
            {
                if (xNodeLoop.NodeType != XmlNodeType.Element)
                    continue;
                switch (xNodeLoop.Name)
                {
                    case "DynamicCategories":
                        _DynamicCategories = new DynamicCategories(r, this, xNodeLoop);
                        break;
                    case "StaticCategories":
                        _StaticCategories = new StaticCategories(r, this, xNodeLoop);
                        break;
                    default:
                        // don't know this element - log it
                        OwnerReport.rl.LogError(4, "Unknown CategoryGrouping element '" + xNodeLoop.Name + "' ignored.");
                        break;
                }
            }
            if ((_DynamicCategories == null && _StaticCategories == null) ||
                (_DynamicCategories != null && _StaticCategories != null))
                OwnerReport.rl.LogError(8, "CategoryGrouping requires either DynamicCategories element or StaticCategories element, but not both.");
        }

        override internal void FinalPass()
        {
            if (_DynamicCategories != null)
                _DynamicCategories.FinalPass();
            if (_StaticCategories != null)
                _StaticCategories.FinalPass();
            return;
        }

        internal DynamicCategories DynamicCategories
        {
            get { return _DynamicCategories; }
            set { _DynamicCategories = value; }
        }

        internal StaticCategories StaticCategories
        {
            get { return _StaticCategories; }
            set { _StaticCategories = value; }
        }
    }

    ///<summary>
}
