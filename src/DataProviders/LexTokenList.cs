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

namespace Reporting.Data
{
	/// <summary>
	/// Represents a list of the tokens.
	/// </summary>
	internal class LexTokenList : IEnumerable
	{
		private ArrayList tokens = null;

		internal LexTokenList()
		{
			tokens = new ArrayList();
		}

		internal void Add(LexToken token)
		{
			tokens.Add(token);
		}

		internal void Push(LexToken token)
		{
			tokens.Insert(0, token);
		}

		internal LexToken Peek()
		{
			return (LexToken)tokens[0];
		}

		internal LexToken Extract()
		{
			LexToken token = (LexToken)tokens[0];
			tokens.RemoveAt(0);
			return token;
		}

		internal int Count
		{
			get
			{
				return tokens.Count;
			}
		}

		public IEnumerator GetEnumerator()
		{
			return tokens.GetEnumerator();
		}
	}
}
