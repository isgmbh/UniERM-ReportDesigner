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
using System.IO;
using System.Reflection;


using Reporting.Rdl;


namespace Reporting.Rdl
{
	/// <summary>
	/// <p>Expression definition</p>
	/// </summary>
	[Serializable]
	internal class Constant<T> : IExpr where T : IConvertible
	{
		T _Value;		// value of the constant

		/// <summary>
		/// Constant - as opposed to an expression
		/// </summary>
		public Constant(T value)
		{
			_Value = value;
		}

        public Constant(string value)
        {
            _Value = (T)Convert.ChangeType(value, Type.GetTypeCode(typeof(T)));
        }

		public TypeCode GetTypeCode()
		{
            return Type.GetTypeCode(typeof(T));
		}

		public bool IsConstant()
		{
			return true;
		}

		public IExpr ConstantOptimization()
		{	// already constant expression
			return this;
		}

		public object Evaluate(Report rpt, Row row)
		{
			return _Value;
		}

        public override string ToString()
        {
            return (string)Convert.ChangeType(_Value, TypeCode.String);
        }
        public string EvaluateString(Report rpt, Row row)
        {
            return this.ToString();
        }

        public double ToDouble()
        {
            return (double)Convert.ChangeType(_Value, TypeCode.Double);
        }
		public double EvaluateDouble(Report rpt, Row row)
		{
            return this.ToDouble();
		}

        public decimal ToDecimal()
        {
            return (decimal)Convert.ChangeType(_Value, TypeCode.Decimal);
        }
		public decimal EvaluateDecimal(Report rpt, Row row)
		{
            return this.ToDecimal();
		}

        public int ToInt32()
        {
            return (int)Convert.ChangeType(_Value, TypeCode.Int32);
        }
        public int EvaluateInt32(Report rpt, Row row)
        {
            return this.ToInt32();
        }

        public DateTime ToDateTime()
        {
            return (DateTime)Convert.ChangeType(_Value, TypeCode.DateTime);
        }
		public DateTime EvaluateDateTime(Report rpt, Row row)
		{
            return this.ToDateTime();
		}

        public bool ToBoolean()
        {
            return (bool)Convert.ChangeType(_Value, TypeCode.Boolean);
        }
		public bool EvaluateBoolean(Report rpt, Row row)
		{
            return this.ToBoolean();
		}
    }
}
