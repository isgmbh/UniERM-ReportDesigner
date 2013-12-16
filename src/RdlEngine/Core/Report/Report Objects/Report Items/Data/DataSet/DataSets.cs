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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using System.Data;


namespace Reporting.Rdl
{
	///<summary>
	/// The sets of data (defined by DataSet) that are retrieved as part of the Report.
	///</summary>
	[Serializable]
    public class DataSets : IEnumerable
	{
		Report _rpt;				// runtime report
		IDictionary _Items;			// list of report items

		internal DataSets(Report rpt, DataSetsDefn dsn)
		{
			_rpt = rpt;

			if (dsn.Items.Count < 10)
				_Items = new ListDictionary();	// Hashtable is overkill for small lists
			else
				_Items = new Hashtable(dsn.Items.Count);

			// Loop thru all the child nodes
			foreach(DataSetDefn dsd in dsn.Items.Values)
			{
				DataSet ds = new DataSet(rpt, dsd);
				_Items.Add(dsd.Name.Nm, ds);
			}
		}
		
		public DataSet this[string name]
		{
			get 
			{
				return _Items[name] as DataSet;
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
	/// Runtime Information about a set of data; public interface to the definition
	///</summary>
	[Serializable]
	public class DataSet
	{
		Report _rpt;		//	the runtime report
		DataSetDefn _dsd;	//  the true definition of the DataSet
	
		internal DataSet(Report rpt, DataSetDefn dsd)
		{
			_rpt = rpt;
			_dsd = dsd;
		}

		public void SetData(IDataReader dr)
		{
			_dsd.Query.SetData(_rpt, dr, _dsd.Fields, _dsd.Filters);		// get the data (and apply the filters
		}

		public void SetData(DataTable dt)
		{
			_dsd.Query.SetData(_rpt, dt, _dsd.Fields, _dsd.Filters);
		}

		public void SetData(XmlDocument xmlDoc)
		{
			_dsd.Query.SetData(_rpt, xmlDoc, _dsd.Fields, _dsd.Filters);
		}

		public void SetData(IEnumerable ie)
		{
			_dsd.Query.SetData(_rpt, ie, _dsd.Fields, _dsd.Filters);
		}

	}
}
