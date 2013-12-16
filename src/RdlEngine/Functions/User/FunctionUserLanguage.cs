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
using System.Globalization;

using Reporting.Rdl;


namespace Reporting.Rdl
{
	/// <summary>
	/// The Language field in the User collection.
	/// </summary>
	[Serializable]
	internal class FunctionUserLanguage : IExpr
	{
		/// <summary>
		/// Client user language
		/// </summary>
		public FunctionUserLanguage() 
		{
		}

		public TypeCode GetTypeCode()
		{
			return TypeCode.String;
		}

		public bool IsConstant()
		{
			return false;
		}

		public IExpr ConstantOptimization()
		{	
			return this;
		}

		// Evaluate is for interpretation  
		public object Evaluate(Report rpt, Row row)
		{
			return EvaluateString(rpt, row);
		}
		
		public double EvaluateDouble(Report rpt, Row row)
		{	
			throw new Exception("Invalid conversion from Language to double.");
		}
		
		public decimal EvaluateDecimal(Report rpt, Row row)
		{
			throw new Exception("Invalid conversion from Language to Decimal.");
		}

        public int EvaluateInt32(Report rpt, Row row)
        {
            throw new Exception("Invalid conversion from Language to Int32.");
        }
		public string EvaluateString(Report rpt, Row row)
		{
			if (rpt == null || rpt.ClientLanguage == null)
				return CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
			else
				return rpt.ClientLanguage;
		}

		public DateTime EvaluateDateTime(Report rpt, Row row)
		{
			throw new Exception("Invalid conversion from Language to DateTime.");
		}

		public bool EvaluateBoolean(Report rpt, Row row)
		{
			throw new Exception("Invalid conversion from Language to boolean.");
		}
	}
}
