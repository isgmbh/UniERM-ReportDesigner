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
using System.Data;

namespace Reporting.Data
{
	/// <summary>
	/// XmlConnection handles connections for XML  
	/// </summary>
	public class XmlConnection : IDbConnection
	{
		string _Connection;				// the connection string; of format directory=
		bool bOpen=false;
		public XmlConnection(string conn)
		{
			ConnectionString = conn;
		}

		internal bool IsOpen
		{
			get {return bOpen;}
		}

		#region IDbConnection Members

		public void ChangeDatabase(string databaseName)
		{
			throw new NotImplementedException("ChangeDatabase method not supported.");
		}

		public IDbTransaction BeginTransaction(System.Data.IsolationLevel il)
		{
			throw new NotImplementedException("BeginTransaction method not supported.");
		}

		IDbTransaction System.Data.IDbConnection.BeginTransaction()
		{
			throw new NotImplementedException("BeginTransaction method not supported.");
		}

		public System.Data.ConnectionState State
		{
			get
			{
				throw new NotImplementedException("State not implemented");
			}
		}

		public string ConnectionString
		{
			get
			{
				return _Connection;
			}
			set
			{
				string c = value;
				// Now parse the connection string;
				Array args = c.Split (',');
				string directory=null;
				foreach(string arg in args)
				{
					if (arg.Trim().ToLower().StartsWith("directory="))	// Only have one type of argument right now
						directory = arg.Trim().Split('=').GetValue(1) as string;
				}
				_Connection = value;
			}
		}

		public IDbCommand CreateCommand()
		{
			return new XmlCommand(this);
		}

		public void Open()
		{
			bOpen = true;
		}

		public void Close()
		{
			bOpen = false;
		}

		public string Database
		{
			get
			{
				return null;			// don't really have a database
			}
		}

		public int ConnectionTimeout
		{
			get
			{
				return 0;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			this.Close();
		}

		#endregion
	}
}