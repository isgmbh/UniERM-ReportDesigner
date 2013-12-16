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
	/// The collection of parameters for a subreport.
	///</summary>
	[Serializable]
	internal class SubReportParameters : ReportLink
	{
        List<SubreportParameter> _Items;			// list of SubreportParameter

		internal SubReportParameters(ReportDefn r, ReportLink p, XmlNode xNode) : base(r, p)
		{
			SubreportParameter rp;
            _Items = new List<SubreportParameter>();
			// Loop thru all the child nodes
			foreach(XmlNode xNodeLoop in xNode.ChildNodes)
			{
				if (xNodeLoop.NodeType != XmlNodeType.Element)
					continue;
				switch (xNodeLoop.Name)
				{
					case "Parameter":
						rp = new SubreportParameter(r, this, xNodeLoop);
						break;
					default:	
						rp=null;		// don't know what this is
						// don't know this element - log it
						OwnerReport.rl.LogError(4, "Unknown SubreportParameters element '" + xNodeLoop.Name + "' ignored.");
						break;
				}
				if (rp != null)
					_Items.Add(rp);
			}
			if (_Items.Count > 0)
                _Items.TrimExcess();
		}
		
		override internal void FinalPass()
		{
			foreach (SubreportParameter rp in _Items)
			{
				rp.FinalPass();
			}
			return;
		}

        internal List<SubreportParameter> Items
		{
			get { return  _Items; }
		}
	}

    ///<summary>
    /// A parameter for a subreport.
    ///</summary>
    [Serializable]
    internal class SubreportParameter : ReportLink
    {
        Name _Name;		// Name of the parameter
        Expression _Value;	// (Variant) An expression that evaluates to the value to
        // hand in for the parameter to the Subreport.

        internal SubreportParameter(ReportDefn r, ReportLink p, XmlNode xNode)
            : base(r, p)
        {
            _Name = null;
            _Value = null;
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
                OwnerReport.rl.LogError(8, "Parameter Name attribute required.");
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
                    default:
                        // don't know this element - log it
                        OwnerReport.rl.LogError(4, "Unknown Subreport parameter element '" + xNodeLoop.Name + "' ignored.");
                        break;
                }
            }

            if (_Value == null)
            {	// Value is required for parameters
                OwnerReport.rl.LogError(8, "The Parameter Value element is required but was not specified.");
            }
        }

        // Handle parsing of function in final pass
        override internal void FinalPass()
        {
            if (_Value != null)
                _Value.FinalPass();
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
    }
}
