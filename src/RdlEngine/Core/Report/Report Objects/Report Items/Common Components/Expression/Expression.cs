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
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using Reporting.Rdl;

namespace Reporting.Rdl
{
	///<summary>
	/// A report expression: includes original source, parsed expression and type information.
	///</summary>
	[Serializable]
	internal class Expression : ReportLink, IExpr
	{
		string _Source;			// source of expression
		IExpr _Expr;			// expression after parse
		TypeCode _Type;			// type of expression; only available after parsed
		ExpressionType _ExpectedType;	// expected type of expression
		string _UniqueName;		// unique name of expression; not always created

        [System.Diagnostics.DebuggerStepThrough]
		internal Expression(ReportDefn r, ReportLink p, XmlNode xNode, ExpressionType et) : this(r, p, xNode.InnerText, et){}

        [System.Diagnostics.DebuggerStepThrough]
        internal Expression(ReportDefn r, ReportLink p, String xNode, ExpressionType et) : base(r, p)
        {
            _Source = xNode;
            _Type = TypeCode.Empty;
            _ExpectedType = et;
            _Expr = null;
        }

		// UniqueName of expression
        
		internal string UniqueName
		{
            [System.Diagnostics.DebuggerStepThrough]
			get {return _UniqueName;}
		}

		override internal void FinalPass()
		{
			// optimization: avoid expression overhead if this isn't really an expression
			if (_Source == null)
			{
				_Expr = new Constant<string>(""); //Originally Plain Constant
				return;
			}
			else if (_Source == "" ||			// empty expression
				_Source[0] != '=')	// if 1st char not '='
			{
                //Originally Plain Constant
				_Expr = new Constant<string>(_Source);	//   this is a constant value
				return;
			}

			Parser p = new Parser(OwnerReport.DataCache);

			// find the fields that are part of the DataRegion (if there is one)
			IDictionary fields=null;
			ReportLink dr = Parent;
			Grouping grp= null;		// remember if in a table group or detail group or list group
			Matrix m=null;
			ReportLink phpf=null;
			while (dr != null) // Loops through Report Items and their 
            {                  // parents until the group that contains it is obtained.
				if (dr is Grouping)
					p.NoAggregateFunctions = true;
				else if (dr is TableGroup)
					grp = ((TableGroup) dr).Grouping;
				else if (dr is Matrix)
				{
					m = (Matrix) dr;		// if matrix we need to pass special
					break;
				}
				else if (dr is Details)
				{
					grp = ((Details) dr).Grouping;
				}
				else if (dr is List)
				{
					grp = ((List) dr).Grouping;
					break;
				}
				else if (dr is PageHeader || dr is PageFooter)
				{
					phpf = dr;
				}
				else if (dr is DataRegion || dr is DataSetDefn)
					break;
				dr = dr.Parent;
			}
			if (dr != null)
			{
				if (dr is DataSetDefn)
				{
					DataSetDefn d = (DataSetDefn) dr;
					if (d.Fields != null)
						fields = d.Fields.Items;
				}
				else	// must be a DataRegion
				{
					DataRegion d = (DataRegion) dr;
					if (d.DataSetDefn != null &&
						d.DataSetDefn.Fields != null)
						fields = d.DataSetDefn.Fields.Items;
				}
			}

			NameLookup lu = new NameLookup(fields, OwnerReport.LUReportParameters,
				OwnerReport.LUReportItems,OwnerReport.LUGlobals,
				OwnerReport.LUUser, OwnerReport.LUAggrScope,
				grp, m, OwnerReport.CodeModules, OwnerReport.Classes, OwnerReport.DataSetsDefn,
				OwnerReport.CodeType);

			if (phpf != null)
			{
				// Non-null when expression is in PageHeader or PageFooter; 
				//   Expression name needed for dynamic lookup of ReportItems on a page.
				lu.PageFooterHeader = phpf;
				lu.ExpressionName = _UniqueName = "xn_" + Interlocked.Increment(ref Parser.Counter).ToString();
			}

			try 
			{
				_Expr = p.Parse(lu, _Source);
			}
			catch (Exception e)
			{
				_Expr = new Constant<string>(e.Message); //Originally ConstantError
				// Invalid expression
				OwnerReport.rl.LogError(8, ErrorText(e.Message));
			}

			// Optimize removing any expression that always result in a constant
			try
			{
				_Expr = _Expr.ConstantOptimization();
			}
			catch(Exception ex)
			{
				OwnerReport.rl.LogError(4, "Expression:" + _Source + "\r\nConstant Optimization exception:\r\n" + ex.Message + "\r\nStack trace:\r\n" + ex.StackTrace );
			}
			_Type = _Expr.GetTypeCode();

			return;
		}

        private string ErrorText(string msg)
        {
            ReportLink rl = this.Parent;
            while (rl != null)
            {
                if (rl is ReportItem)
                    break;
                rl = rl.Parent;
            }

            string prefix="Expression";
            if (rl != null)
            {          
                ReportItem ri = rl as ReportItem;
                if (ri.Name != null)
                    prefix = ri.Name.Nm + " expression";
            }
            return prefix + " '" + _Source + "' failed to parse: " + msg;
        }

        [System.Diagnostics.DebuggerStepThrough]
		private void ReportError(Report rpt, int severity, string err)
		{
			if (rpt == null)
				OwnerReport.rl.LogError(severity, err);
			else
				rpt.rl.LogError(severity, err);
		}

		internal string Source
		{
            [System.Diagnostics.DebuggerStepThrough]
			get { return  _Source; }
		}
		internal IExpr Expr
		{
            [System.Diagnostics.DebuggerStepThrough]
			get { return  _Expr; }
		}
		internal TypeCode Type
		{
            [System.Diagnostics.DebuggerStepThrough]
			get { return  _Type; }
		}
		internal ExpressionType ExpectedType
		{
            [System.Diagnostics.DebuggerStepThrough]
			get { return  _ExpectedType; }
		}
		#region IExpr Members

        [System.Diagnostics.DebuggerStepThrough]
		public System.TypeCode GetTypeCode()
		{
            if (_Expr == null)
                return System.TypeCode.Object;      // we really don't know the type
			return _Expr.GetTypeCode();
		}

        [System.Diagnostics.DebuggerStepThrough]
		public bool IsConstant()
		{
            if (_Expr == null)
            {
                this.FinalPass();           // expression hasn't been parsed yet -- let try to parse it.
                if (_Expr == null)
                    return false;           // no luck; then don't treat as constant
            }
            return _Expr.IsConstant();
		}

        [System.Diagnostics.DebuggerStepThrough]
		public IExpr ConstantOptimization()
		{
			return this;
		}

		public object Evaluate(Report rpt, Row row)
		{
			try 
			{
				// Check to see if we're evaluating an expression in a page header or footer;
				//   If that is the case the rows are cached by page.
				if (row == null && this.UniqueName != null)
				{
					Rows rows = rpt.GetPageExpressionRows(UniqueName);
					if (rows != null && rows.Data != null && rows.Data.Count > 0)
						row = rows.Data[0];
				}

				return _Expr.Evaluate(rpt, row);
			}
			catch (Exception e)
			{
				string err;
				if (e.InnerException != null)
					err = String.Format("Exception evaluating {0}.  {1}.  {2}", _Source, e.Message, e.InnerException.Message);
				else
					err = String.Format("Exception evaluating {0}.  {1}", _Source, e.Message);

				ReportError(rpt, 4, err);
				return null;
			}
		}

        [System.Diagnostics.DebuggerStepThrough]
		public string EvaluateString(Report rpt, Row row)
		{
			try 
			{
				return _Expr.EvaluateString(rpt, row);
			}
			catch (Exception e)
			{	
				string err = String.Format("Exception evaluating {0}.  {1}", _Source, e.Message);
				ReportError(rpt, 4, err);
				return null;
			}
		}

        [System.Diagnostics.DebuggerStepThrough]
		public double EvaluateDouble(Report rpt, Row row)
		{
			try 
			{
				return _Expr.EvaluateDouble(rpt, row);
			}
			catch (Exception e)
			{	
				string err = String.Format("Exception evaluating {0}.  {1}", _Source, e.Message);
				ReportError(rpt, 4, err);
				return double.NaN;
			}
		}

        [System.Diagnostics.DebuggerStepThrough]
        public decimal EvaluateDecimal(Report rpt, Row row)
		{
			try 
			{
				return _Expr.EvaluateDecimal(rpt, row);
			}
			catch (Exception e)
			{	
				string err = String.Format("Exception evaluating {0}.  {1}", _Source, e.Message);
				ReportError(rpt, 4, err);
				return decimal.MinValue;
			}
		}

        [System.Diagnostics.DebuggerStepThrough]
        public int EvaluateInt32(Report rpt, Row row)
        {
            try
            {
                return _Expr.EvaluateInt32(rpt, row);
            }
            catch (Exception e)
            {
                string err = String.Format("Exception evaluating {0}.  {1}", _Source, e.Message);
                ReportError(rpt, 4, err);
                return int.MinValue;
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
		public DateTime EvaluateDateTime(Report rpt, Row row)
		{
			try 
			{
				return _Expr.EvaluateDateTime(rpt, row);
			}
			catch (Exception e)
			{	
				string err = String.Format("Exception evaluating {0}.  {1}", _Source, e.Message);
				ReportError(rpt, 4, err);
				return DateTime.MinValue;
			}
		}

        [System.Diagnostics.DebuggerStepThrough]
		public bool EvaluateBoolean(Report rpt, Row row)
		{
			try 
			{
				return _Expr.EvaluateBoolean(rpt, row);
			}
			catch (Exception e)
			{	
				string err = String.Format("Exception evaluating {0}.  {1}", _Source, e.Message);
				ReportError(rpt, 4, err);
				return false;
			}
		}

		#endregion
	}
}
