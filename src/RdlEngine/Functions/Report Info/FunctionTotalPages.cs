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
using System.IO;

using Reporting.Rdl;


namespace Reporting.Rdl
{
	/// <summary>
	/// Total Pages
	/// </summary>
	[Serializable]
	internal class FunctionTotalPages : IExpr
	{
		/// <summary>
		/// Total page count; relys on PageHeader, PageFooter to set Report.TotalPages
		/// </summary>
		public FunctionTotalPages() 
		{
		}

		public TypeCode GetTypeCode()
		{
			return TypeCode.Int32;
		}

		public bool IsConstant()
		{
			return false;
		}

		public IExpr ConstantOptimization()
		{	// not a constant expression
			return this;
		}

		// Evaluate is for interpretation  
		public object Evaluate(Report rpt, Row row)
		{
            return rpt == null ? (int) 1 : (int) rpt.TotalPages;
		}
		
		public double EvaluateDouble(Report rpt, Row row)
		{	
			return rpt == null? 1: rpt.TotalPages;
		}

        public int EvaluateInt32(Report rpt, Row row)
        {
            return rpt == null ? 1 : rpt.TotalPages;
        }
		
		public decimal EvaluateDecimal(Report rpt, Row row)
		{
			int result = EvaluateInt32(rpt, row);

			return Convert.ToDecimal(result);
		}

		public string EvaluateString(Report rpt, Row row)
		{
			int result = EvaluateInt32(rpt, row);
			return result.ToString();
		}

		public DateTime EvaluateDateTime(Report rpt, Row row)
		{
			int result = EvaluateInt32(rpt, row);
			return Convert.ToDateTime(result);
		}

		public bool EvaluateBoolean(Report rpt, Row row)
		{
			int result = EvaluateInt32(rpt, row);
			return Convert.ToBoolean(result);
		}
		
	}
}
