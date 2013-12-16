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
using Reporting.Rdl.Utility;
using System.IO;


namespace Reporting.Rdl
{
	///<summary>
	/// Contains list of DataSource about how to connect to sources of data used by the DataSets.
	///</summary>
	[Serializable]
	internal class DataSourcesDefn : ReportLink
	{
		ListDictionary _Items;			// list of report items

		internal DataSourcesDefn(ReportDefn r, ReportLink p, XmlNode xNode) : base(r, p)
		{
			// Run thru the attributes
//			foreach(XmlAttribute xAttr in xNode.Attributes)
//			{
//			}
			_Items = new ListDictionary();
			// Loop thru all the child nodes
			foreach(XmlNode xNodeLoop in xNode.ChildNodes)
			{
				if (xNodeLoop.NodeType != XmlNodeType.Element)
					continue;
				if (xNodeLoop.Name == "DataSource")
				{
					DataSourceDefn ds = new DataSourceDefn(r, this, xNodeLoop);
					if (ds.Name != null)
						_Items.Add(ds.Name.Nm, ds);
				}
			}
			if (_Items.Count == 0)
				OwnerReport.rl.LogError(8, "For DataSources at least one DataSource is required.");
		}

		public DataSourceDefn this[string name]
		{
			get 
			{
				return _Items[name] as DataSourceDefn;
			}
		}

		internal void CleanUp(Report rpt)		// closes any connections
		{
			foreach (DataSourceDefn ds in _Items.Values)
			{
				ds.CleanUp(rpt);
			}
		}
		
		override internal void FinalPass()
		{
			foreach (DataSourceDefn ds in _Items.Values)
			{
				ds.FinalPass();
			}
			return;
		}

		internal bool ConnectDataSources(Report rpt)
		{
			// Handle any parent connections if any	(ie we're in a subreport and want to use parent report connections
			if (rpt.ParentConnections != null && rpt.ParentConnections.Items != null)
			{	// we treat subreport merged transaction connections as set by the User 
				foreach (DataSourceDefn ds in _Items.Values)
				{
					foreach (DataSourceDefn dsp in rpt.ParentConnections.Items.Values)
					{
						if (ds.AreSameDataSource(dsp))
						{
							ds.SetUserConnection(rpt, dsp.GetConnection(rpt));
							break;
						}
					}
				}
			}

			foreach (DataSourceDefn ds in _Items.Values)
			{
				ds.ConnectDataSource(rpt);
			}
			return true;
		}


		internal ListDictionary Items
		{
			get { return  _Items; }
		}
	}

    ///<summary>
    /// Information about the data source (e.g. a database connection string).
    ///</summary>
    [Serializable]
    internal class DataSourceDefn : ReportLink
    {
        Name _Name;		// The name of the data source
        // Must be unique within the report
        bool _Transaction;	// Indicates the data sets that use this data
        // source should be executed in a single transaction.
        ConnectionProperties _ConnectionProperties;	//Information about how to connect to the data source
        string _DataSourceReference;	//The full path (e.g.
        // “/salesreports/salesdatabase”) or relative path
        // (e.g. “salesdatabase”) to a data source
        // reference. Relative paths start in the same
        // location as the report.		
        [NonSerialized]
        IDbConnection _ParseConnection;	// while parsing we sometimes need to connect

        internal DataSourceDefn(ReportDefn r, ReportLink p, XmlNode xNode)
            : base(r, p)
        {
            _Name = null;
            _Transaction = false;
            _ConnectionProperties = null;
            _DataSourceReference = null;
            // Run thru the attributes
            foreach (XmlAttribute xAttr in xNode.Attributes)
            {
                switch (xAttr.Name)
                {
                    case "Name":
                        _Name = new Name(xAttr.Value);
                        break;
                }
            }

            // Loop thru all the child nodes
            foreach (XmlNode xNodeLoop in xNode.ChildNodes)
            {
                if (xNodeLoop.NodeType != XmlNodeType.Element)
                    continue;
                switch (xNodeLoop.Name)
                {
                    case "Transaction":
                        _Transaction = Conversion.ToBoolean(xNodeLoop.InnerText, OwnerReport.rl);
                        break;
                    case "ConnectionProperties":
                        _ConnectionProperties = new ConnectionProperties(r, this, xNodeLoop);
                        break;
                    case "DataSourceReference":
                        _DataSourceReference = xNodeLoop.InnerText;
                        break;
                    default:
                        // don't know this element - log it
                        OwnerReport.rl.LogError(4, "Unknown DataSource element '" + xNodeLoop.Name + "' ignored.");
                        break;
                }
            }
            if (_Name == null)
                OwnerReport.rl.LogError(8, "DataSource Name is required but not specified.");
            else if (_ConnectionProperties == null && _DataSourceReference == null)
                OwnerReport.rl.LogError(8, string.Format("Either ConnectionProperties or DataSourceReference must be specified for DataSource {0}.", this._Name.Nm));
            else if (_ConnectionProperties != null && _DataSourceReference != null)
                OwnerReport.rl.LogError(8, string.Format("Either ConnectionProperties or DataSourceReference must be specified for DataSource {0} but not both.", this._Name.Nm));
        }

        override internal void FinalPass()
        {
            if (_ConnectionProperties != null)
                _ConnectionProperties.FinalPass();

            ConnectDataSource(null);
            return;
        }

        internal bool IsConnected(Report rpt)
        {
            return GetConnection(rpt) == null ? false : true;
        }

        internal bool AreSameDataSource(DataSourceDefn dsd)
        {
            if (this.DataSourceReference != null &&
                this.DataSourceReference == dsd.DataSourceReference)
                return true;		// datasource references are the same

            if (this.ConnectionProperties == null ||
                dsd.ConnectionProperties == null)
                return false;

            ConnectionProperties cp1 = this.ConnectionProperties;
            ConnectionProperties cp2 = dsd.ConnectionProperties;
            return (cp1.DataProvider == cp2.DataProvider &&
                cp1.ConnectstringValue == cp2.ConnectstringValue &&
                cp1.IntegratedSecurity == cp2.IntegratedSecurity);
        }

        internal bool ConnectDataSource(Report rpt)
        {
            IDbConnection cn = GetConnection(rpt);
            if (cn != null)
                return true;

            if (_DataSourceReference != null)
                ConnectDataSourceReference(rpt);	// this will create a _ConnectionProperties

            if (_ConnectionProperties == null ||
                _ConnectionProperties.ConnectstringValue == null)
                return false;

            bool rc = false;
            try
            {
                cn = RdlEngineConfig.GetConnection(_ConnectionProperties.DataProvider,
                    _ConnectionProperties.Connectstring(rpt));
                if (cn != null)
                {
                    cn.Open();
                    rc = true;
                }
            }
            catch (Exception e)
            {
                string err = string.Format("DataSource '{0}'.\r\n{1}", _Name,
                    e.InnerException == null ? e.Message : e.InnerException.Message);
                if (rpt == null)
                    OwnerReport.rl.LogError(4, err);	// error occurred during parse phase
                else
                    rpt.rl.LogError(4, err);
                if (cn != null)
                {
                    cn.Close();
                    cn = null;
                }
            }

            if (cn != null)
                SetSysConnection(rpt, cn);
            else
            {
                string err = string.Format("Unable to connect to datasource '{0}'.", this._Name.Nm);
                if (rpt == null)
                    OwnerReport.rl.LogError(4, err);	// error occurred during parse phase
                else
                    rpt.rl.LogError(4, err);
            }
            return rc;
        }

        void ConnectDataSourceReference(Report rpt)
        {
            if (_ConnectionProperties != null)
                return;

            try
            {
                string file;
                string folder = rpt == null ? OwnerReport.ParseFolder : rpt.Folder;
                if (folder == null)
                {   // didn't specify folder; check to see if we have a fully formed name 
                    if (!_DataSourceReference.EndsWith(".dsr", StringComparison.InvariantCultureIgnoreCase))
                        file = _DataSourceReference + ".dsr";
                    else
                        file = _DataSourceReference;
                }
                else if (_DataSourceReference[0] != Path.DirectorySeparatorChar)
                    file = folder + Path.DirectorySeparatorChar + _DataSourceReference + ".dsr";
                else
                    file = folder + _DataSourceReference + ".dsr";

                string pswd = OwnerReport.GetDataSourceReferencePassword == null ?
                                    null : OwnerReport.GetDataSourceReferencePassword();
                if (pswd == null)
                    throw new Exception("No password provided for shared DataSource reference");

                string xml = Rdl.DataSourceReference.Retrieve(file, pswd);
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xml);
                XmlNode xNodeLoop = xDoc.FirstChild;

                _ConnectionProperties = new ConnectionProperties(OwnerReport, this, xNodeLoop);
                _ConnectionProperties.FinalPass();
            }
            catch (Exception e)
            {
                OwnerReport.rl.LogError(4, e.Message);
                _ConnectionProperties = null;
            }
            return;
        }

        internal bool IsUserConnection(Report rpt)
        {
            if (rpt == null)
                return false;

            object uc = rpt.Cache.Get(this, "UserConnection");
            return uc == null ? false : true;
        }

        internal void SetUserConnection(Report rpt, IDbConnection cn)
        {
            if (cn == null)
                rpt.Cache.Remove(this, "UserConnection");
            else
            {
                rpt.Cache.AddReplace(this, "UserConnection", cn);
            }
        }

        private void SetSysConnection(Report rpt, IDbConnection cn)
        {
            if (rpt == null)
                _ParseConnection = cn;
            else if (cn == null)
                rpt.Cache.Remove(this, "SysConnection");
            else
                rpt.Cache.Add(this, "SysConnection", cn);
        }

        internal IDbConnection GetConnection(Report rpt)
        {
            IDbConnection cn;

            if (rpt == null)
                return _ParseConnection;

            cn = rpt.Cache.Get(this, "UserConnection") as IDbConnection;
            if (cn == null)
            {
                cn = rpt.Cache.Get(this, "SysConnection") as IDbConnection;
            }
            return cn;
        }

        internal void CleanUp(Report rpt)
        {
            if (IsUserConnection(rpt))
                return;
            IDbConnection cn = GetConnection(rpt);
            if (cn == null)
                return;

            try
            {
                cn.Close();
                // cn.Dispose();		// not good for connection pooling
            }
            catch (Exception ex)
            {	// report the error but keep going
                if (rpt != null)
                    rpt.rl.LogError(4, string.Format("Error closing connection. {0}", ex.Message));
                else
                    this.OwnerReport.rl.LogError(4, string.Format("Error closing connection. {0}", ex.Message));
            }
            SetSysConnection(rpt, null);	// get rid of connection from cache
            return;
        }

        internal Name Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        internal bool Transaction
        {
            get { return _Transaction; }
            set { _Transaction = value; }
        }

        internal ConnectionProperties ConnectionProperties
        {
            get { return _ConnectionProperties; }
            set { _ConnectionProperties = value; }
        }

        internal IDbConnection SqlConnect(Report rpt)
        {
            return GetConnection(rpt);
        }

        internal string DataSourceReference
        {
            get { return _DataSourceReference; }
            set { _DataSourceReference = value; }
        }
    }
}
