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
using System.Xml;
using Reporting.Rdl.Utility;

namespace Reporting.Rdl
{
	///<summary>
	/// DataLabel definition and processing.
	///</summary>
	[Serializable]
	internal class DataLabel : ReportLink
	{
		Style _Style;	// Defines text, border and background style
						// properties for the labels
		Expression _Value;	//(Variant) Expression for the value labels. If omitted,
						// values of in the ValueAxis are used for labels.
		bool _Visible;	// Whether the data label is displayed on the
						// chart. Defaults to False.
		DataLabelPositionEnum _Position;	// Position of the label.  Default: auto
		int _Rotation;	// Angle of rotation of the label text		
	
		internal DataLabel(ReportDefn r, ReportLink p, XmlNode xNode) : base(r, p)
		{
			_Style=null;
			_Value=null;
			_Visible=false;
			_Position=DataLabelPositionEnum.Auto;
			_Rotation=0;

			// Loop thru all the child nodes
			foreach(XmlNode xNodeLoop in xNode.ChildNodes)
			{
				if (xNodeLoop.NodeType != XmlNodeType.Element)
					continue;
				switch (xNodeLoop.Name)
				{
					case "Style":
						_Style = new Style(r, this, xNodeLoop);
						break;
					case "Value":
						_Value = new Expression(r, this, xNodeLoop, ExpressionType.Variant);
						break;
					case "Visible":
                        _Visible = Conversion.ToBoolean(xNodeLoop.InnerText, OwnerReport.rl);
						break;
					case "Position":
						_Position = DataLabelPosition.GetStyle(xNodeLoop.InnerText, OwnerReport.rl);
						break;
					case "Rotation":
						_Rotation = Conversion.ToInteger(xNodeLoop.InnerText);
						break;
					default:
						break;
				}
			}
		

		}

		// Handle parsing of function in final pass
		override internal void FinalPass()
		{
			if (_Style != null) 
				_Style.FinalPass();
			if (_Value != null) 
				_Value.FinalPass();
			return;
		}


		internal Style Style
		{
			get { return  _Style; }
			set {  _Style = value; }
		}

		internal Expression Value
		{
			get { return  _Value; }
			set {  _Value = value; }
		}

		internal bool Visible
		{
			get { return  _Visible; }
			set {  _Visible = value; }
		}

		internal DataLabelPositionEnum Position
		{
			get { return  _Position; }
			set {  _Position = value; }
		}

		internal int Rotation
		{
			get { return  _Rotation; }
			set {  _Rotation = value; }
		}
	}
}
