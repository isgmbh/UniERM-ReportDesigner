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
	///  Collection of Filter values to compare against in a filter.  Cardinality depends 
	///  filter operater.
	///</summary>
	[Serializable]
	internal class FilterValues : ReportLink
	{
        List<FilterValue> _Items;			// list of FilterValue

		internal FilterValues(ReportDefn r, ReportLink p, XmlNode xNode) : base(r, p)
		{
			FilterValue f;
            _Items = new List<FilterValue>();
			// Loop thru all the child nodes
			foreach(XmlNode xNodeLoop in xNode.ChildNodes)
			{
				if (xNodeLoop.NodeType != XmlNodeType.Element)
					continue;
				switch (xNodeLoop.Name)
				{
					case "FilterValue":
						f = new FilterValue(r, this, xNodeLoop);
						break;
					default:	
						f=null;		// don't know what this is
						// don't know this element - log it
						OwnerReport.rl.LogError(4, "Unknown FilterValues element '" + xNodeLoop.Name + "' ignored.");
						break;
				}
				if (f != null)
					_Items.Add(f);
			}
			if (_Items.Count == 0)
				OwnerReport.rl.LogError(8, "For FilterValues at least one FilterValue is required.");
			else
                _Items.TrimExcess();
		}
		
		override internal void FinalPass()
		{
			foreach (FilterValue f in _Items)
			{
				f.FinalPass();
			}
			return;
		}

		internal List<FilterValue> Items
		{
			get { return  _Items; }
		}
	}

    ///<summary>
    /// A value used in a filter.
    ///</summary>
    [Serializable]
    internal class FilterValue : ReportLink
    {
        Expression _Expression;			// 

        internal FilterValue(ReportDefn r, ReportLink p, XmlNode xNode)
            : base(r, p)
        {
            _Expression = new Expression(r, this, xNode, ExpressionType.Variant);
        }

        // Handle parsing of function in final pass
        override internal void FinalPass()
        {
            if (_Expression != null)
                _Expression.FinalPass();
            return;
        }

        internal Expression Expression
        {
            get { return _Expression; }
            set { _Expression = value; }
        }
    }
}
