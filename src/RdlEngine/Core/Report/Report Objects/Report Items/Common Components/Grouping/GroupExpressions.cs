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
	/// Collection of group expressions.
	///</summary>
	[Serializable]
	internal class GroupExpressions : ReportLink
	{
        List<GroupExpression> _Items;			// list of GroupExpression

		internal GroupExpressions(ReportDefn r, ReportLink p, XmlNode xNode) : base(r, p)
		{
			GroupExpression g;
            _Items = new List<GroupExpression>();
			// Loop thru all the child nodes
			foreach(XmlNode xNodeLoop in xNode.ChildNodes)
			{
				if (xNodeLoop.NodeType != XmlNodeType.Element)
					continue;
				switch (xNodeLoop.Name)
				{
					case "GroupExpression":
						g = new GroupExpression(r, this, xNodeLoop);
						break;
					default:	
						g=null;		// don't know what this is
						// don't know this element - log it
						OwnerReport.rl.LogError(4, "Unknown GroupExpressions element '" + xNodeLoop.Name + "' ignored.");
						break;
				}
				if (g != null)
					_Items.Add(g);
			}
			if (_Items.Count == 0)
				OwnerReport.rl.LogError(8, "GroupExpressions require at least one GroupExpression be defined.");
			else
                _Items.TrimExcess();
		}
		
		override internal void FinalPass()
		{
			foreach (GroupExpression g in _Items)
			{
				g.FinalPass();
			}
			return;
		}

        internal List<GroupExpression> Items
		{
			get { return  _Items; }
		}
	}

    ///<summary>
    /// Definition of an expression within a group.
    ///</summary>
    [Serializable]
    internal class GroupExpression : ReportLink
    {
        Expression _Expression;			// 

        internal GroupExpression(ReportDefn r, ReportLink p, XmlNode xNode)
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
