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
	///<summary>
	/// Collection of sort bys.
	///</summary>
	[Serializable]
	internal class Sorting : ReportLink
	{
        List<SortBy> _Items;			// list of SortBy

		internal Sorting(ReportDefn r, ReportLink p, XmlNode xNode) : base(r, p)
		{
			SortBy s;
            _Items = new List<SortBy>();
			// Loop thru all the child nodes
			foreach(XmlNode xNodeLoop in xNode.ChildNodes)
			{
				if (xNodeLoop.NodeType != XmlNodeType.Element)
					continue;
				switch (xNodeLoop.Name)
				{
					case "SortBy":
						s = new SortBy(r, this, xNodeLoop);
						break;
					default:	
						s=null;		// don't know what this is
						// don't know this element - log it
						OwnerReport.rl.LogError(4, "Unknown Sorting element '" + xNodeLoop.Name + "' ignored.");
						break;
				}
				if (s != null)
					_Items.Add(s);
			}
			if (_Items.Count == 0)
				OwnerReport.rl.LogError(8, "Sorting requires at least one SortBy be defined.");
			else
                _Items.TrimExcess();
		}
		
		override internal void FinalPass()
		{
			foreach (SortBy s in _Items)
			{
				s.FinalPass();
			}
			return;
		}

        internal List<SortBy> Items
		{
			get { return  _Items; }
		}
	}

    ///<summary>
    /// A single sort expression and direction.
    ///</summary>
    [Serializable]
    internal class SortBy : ReportLink
    {
        Expression _SortExpression;	// (Variant) The expression to sort the groups by.
        // The functions RunningValue and RowNumber
        // are not allowed in SortExpression.
        // References to report items are not allowed.
        SortDirectionEnum _Direction;	// Indicates the direction of the sort
        // Ascending (Default) | Descending

        internal SortBy(ReportDefn r, ReportLink p, XmlNode xNode)
            : base(r, p)
        {
            _SortExpression = null;
            _Direction = SortDirectionEnum.Ascending;

            // Loop thru all the child nodes
            foreach (XmlNode xNodeLoop in xNode.ChildNodes)
            {
                if (xNodeLoop.NodeType != XmlNodeType.Element)
                    continue;
                switch (xNodeLoop.Name)
                {
                    case "SortExpression":
                        _SortExpression = new Expression(r, this, xNodeLoop, ExpressionType.Variant);
                        break;
                    case "Direction":
                        _Direction = SortDirection.GetStyle(xNodeLoop.InnerText, OwnerReport.rl);
                        break;
                    default:
                        // don't know this element - log it
                        OwnerReport.rl.LogError(4, "Unknown SortBy element '" + xNodeLoop.Name + "' ignored.");
                        break;
                }
            }
            if (_SortExpression == null)
                OwnerReport.rl.LogError(8, "SortBy requires the SortExpression element.");
        }

        // Handle parsing of function in final pass
        override internal void FinalPass()
        {
            if (_SortExpression != null)
                _SortExpression.FinalPass();
            return;
        }

        internal Expression SortExpression
        {
            get { return _SortExpression; }
            set { _SortExpression = value; }
        }

        internal SortDirectionEnum Direction
        {
            get { return _Direction; }
            set { _Direction = value; }
        }
    }
}
