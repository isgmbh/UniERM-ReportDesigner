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

namespace Reporting.Rdl
{
	///<summary>
	/// When Query is database SQL; QueryColumn represents actual database column.
	///</summary>
	[Serializable]
	internal class QueryColumn
	{
		internal int colNum;			// Column # in query select
		internal string colName;		// Column name in query
		internal TypeCode _colType;	// TypeCode in query

		internal QueryColumn(int colnum, string name, TypeCode c)
		{
			colNum = colnum;
            colName = name.TrimEnd('\0');
			_colType = c;
		}

		internal TypeCode colType
		{
			// Treat Char as String for queries: <sigh> drivers sometimes confuse char and string types
			//    telling me a type is char but actually returning a string (Mono work around)
			get {return _colType == TypeCode.Char? TypeCode.String: _colType; }
		}
	}
}
