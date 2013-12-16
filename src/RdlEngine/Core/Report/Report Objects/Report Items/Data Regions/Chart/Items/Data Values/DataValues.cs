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
	/// In Charts, the collection of data values for a single data point.
	///</summary>
	[Serializable]
	internal class DataValues : ReportLink
	{
        List<DataValue> _Items;			// list of DataValue

		internal DataValues(ReportDefn r, ReportLink p, XmlNode xNode) : base(r, p)
		{
			DataValue dv;
            _Items = new List<DataValue>();
			// Loop thru all the child nodes
			foreach(XmlNode xNodeLoop in xNode.ChildNodes)
			{
				if (xNodeLoop.NodeType != XmlNodeType.Element)
					continue;
				switch (xNodeLoop.Name)
				{
					case "DataValue":
						dv = new DataValue(r, this, xNodeLoop);
						break;
					default:	
						dv=null;		// don't know what this is
						// don't know this element - log it
						OwnerReport.rl.LogError(4, "Unknown DataValues element '" + xNodeLoop.Name + "' ignored.");
						break;
				}
				if (dv != null)
					_Items.Add(dv);
			}
			if (_Items.Count == 0)
				OwnerReport.rl.LogError(8, "For DataValues at least one DataValue is required.");
			else
                _Items.TrimExcess();
		}
		
		override internal void FinalPass()
		{
			foreach (DataValue dv in _Items)
			{
				dv.FinalPass();
			}
			return;
		}

        internal List<DataValue> Items
		{
			get { return  _Items; }
		}
	}

    ///<summary>
    /// In charts, the DataValue defines a single value for the DataPoint.
    ///</summary>
    [Serializable]
    internal class DataValue : ReportLink
    {
        Expression _Value;	// (Variant) Value expression. Same restrictions as
        //  the expressions in a matrix cell		
        internal DataValue(ReportDefn r, ReportLink p, XmlNode xNode)
            : base(r, p)
        {
            _Value = null;

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
                        break;
                }
            }
            if (_Value == null)
                OwnerReport.rl.LogError(8, "DataValue requires the Value element.");
        }

        // Handle parsing of function in final pass
        override internal void FinalPass()
        {
            if (_Value != null)
                _Value.FinalPass();
            return;
        }


        internal Expression Value
        {
            get { return _Value; }
            set { _Value = value; }
        }
    }
}
