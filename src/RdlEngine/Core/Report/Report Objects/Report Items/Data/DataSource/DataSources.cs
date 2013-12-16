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
using System.Collections.Specialized;
using System.Xml;

using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data.Odbc;
using System.IO;


namespace Reporting.Rdl
{
	///<summary>
	/// Contains list of DataSource about how to connect to sources of data used by the DataSets.
	///</summary>
	[Serializable]
    public class DataSources : IEnumerable
	{
		Report _rpt;				// Runtime report
		ListDictionary _Items;		// list of report items

		internal DataSources(Report rpt, DataSourcesDefn dsds)
		{
			_rpt = rpt;
			_Items = new ListDictionary();

			// Loop thru all the child nodes
			foreach(DataSourceDefn dsd in dsds.Items.Values)
			{
				DataSource ds = new DataSource(rpt, dsd);
				_Items.Add(dsd.Name.Nm,	ds);
			}
		}

		public DataSource this[string name]
		{
			get 
			{
				return _Items[name] as DataSource;
			}
		}
        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return _Items.Values.GetEnumerator();
        }

        #endregion
    }
    
    ///<summary>
	/// Information about the data source (e.g. a database connection string).
	///</summary>
	[Serializable]
	public class DataSource
	{
		Report _rpt;	// Runtime report
		DataSourceDefn _dsd;	// DataSource definition

		internal DataSource(Report rpt, DataSourceDefn dsd)
		{
			_rpt = rpt;
			_dsd = dsd;
		}

		/// <summary>
		/// Get/Set the database connection.  User must handle closing of connection.
		/// </summary>
		public IDbConnection UserConnection
		{
			get {return _dsd.IsUserConnection(_rpt)? _dsd.GetConnection(_rpt): null;}	// never reveal connection internally connected
			set
			{
				_dsd.CleanUp(_rpt);					// clean up prior connection if necessary

				_dsd.SetUserConnection(_rpt, value);
			}
		}
	}
}
