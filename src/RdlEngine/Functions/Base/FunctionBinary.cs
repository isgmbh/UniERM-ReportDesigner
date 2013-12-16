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
	/// Binary operator
	/// </summary>
	[Serializable]
	internal abstract class FunctionBinary
	{
		public IExpr _lhs;			// lhs 
		public IExpr _rhs;			// rhs

		/// <summary>
		/// Arbitrary binary operater; might be a
		/// </summary>
		public FunctionBinary() 
		{
			_lhs = null;
			_rhs = null;
		}

		public FunctionBinary(IExpr l, IExpr r) 
		{
			_lhs = l;
			_rhs = r;
		}

		public bool IsConstant()
		{
			if (_lhs.IsConstant())
				return _rhs.IsConstant();

			return false;
		}

//		virtual public bool EvaluateBoolean(Report rpt, Row row)
//		{
//			return false;
//		}

		public IExpr Lhs
		{
			get { return  _lhs; }
			set {  _lhs = value; }
		}

		public IExpr Rhs
		{
			get { return  _rhs; }
			set {  _rhs = value; }
		}
	}
}
