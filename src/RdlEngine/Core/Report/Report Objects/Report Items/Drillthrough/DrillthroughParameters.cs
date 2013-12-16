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
	/// The collection of Drillthrough parameters.
	///</summary>
	[Serializable]
	internal class DrillthroughParameters : ReportLink
	{
        List<DrillthroughParameter> _Items;			// list of report items

		internal DrillthroughParameters(ReportDefn r, ReportLink p, XmlNode xNode) : base(r, p)
		{
			DrillthroughParameter d;
            _Items = new List<DrillthroughParameter>();
			// Loop thru all the child nodes
			foreach(XmlNode xNodeLoop in xNode.ChildNodes)
			{
				if (xNodeLoop.NodeType != XmlNodeType.Element)
					continue;
				switch (xNodeLoop.Name)
				{
					case "Parameter":
						d = new DrillthroughParameter(r, this, xNodeLoop);
						break;
					default:	
						d=null;		// don't know what this is
						// don't know this element - log it
						OwnerReport.rl.LogError(4, "Unknown Parameters element '" + xNodeLoop.Name + "' ignored.");
						break;
				}
				if (d != null)
					_Items.Add(d);
			}
			if (_Items.Count > 0)
                _Items.TrimExcess();
		}
		
		override internal void FinalPass()
		{
			foreach (DrillthroughParameter r in _Items)
			{
				r.FinalPass();
			}
			return;
		}

        internal List<DrillthroughParameter> Items
		{
			get { return  _Items; }
		}
	}

    ///<summary>
    /// A drillthrough parameter.
    ///</summary>
    [Serializable]
    internal class DrillthroughParameter : ReportLink
    {
        Name _Name;			// Name of the parameter
        Expression _Value;	// (Variant) An expression that evaluates to the value to
        // hand in for the parameter to the Drillthough.
        Expression _Omit;	// (Boolean) Indicates the parameter should be skipped.

        internal DrillthroughParameter(ReportDefn r, ReportLink p, XmlNode xNode)
            : base(r, p)
        {
            _Name = null;
            _Value = null;
            _Omit = null;
            // Run thru the attributes
            foreach (XmlAttribute xAttr in xNode.Attributes)
            {
                switch (xAttr.Name)
                {
                    case "Name":
                        _Name = new Name(xAttr.Value);
                        break;
                }
            }

            if (_Name == null)
            {	// Name is required for parameters
                OwnerReport.rl.LogError(8, "Parameter Name attribute required.'");
            }

            // Loop thru all the child nodes
            foreach (XmlNode xNodeLoop in xNode.ChildNodes)
            {
                if (xNodeLoop.NodeType != XmlNodeType.Element)
                    continue;
                switch (xNodeLoop.Name)
                {
                    case "Value":
                        _Value = new Expression(r, this, xNodeLoop, ExpressionType.Variant);
                        break;
                    case "Omit":
                        _Omit = new Expression(r, this, xNodeLoop, ExpressionType.Boolean);
                        break;
                    default:
                        // don't know this element - log it
                        OwnerReport.rl.LogError(4, "Unknown Parameter element '" + xNodeLoop.Name + "' ignored.");
                        break;
                }
            }
        }

        // Handle parsing of function in final pass
        override internal void FinalPass()
        {
            if (_Value != null)
                _Value.FinalPass();
            if (_Omit != null)
                _Omit.FinalPass();
            return;
        }

        internal Name Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        internal Expression Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        internal string ValueValue(Report rpt, Row r)
        {
            if (_Value == null)
                return "";

            return _Value.EvaluateString(rpt, r);
        }

        internal Expression Omit
        {
            get { return _Omit; }
            set { _Omit = value; }
        }

        internal bool OmitValue(Report rpt, Row r)
        {
            if (_Omit == null)
                return false;

            return _Omit.EvaluateBoolean(rpt, r);
        }
    }
}
