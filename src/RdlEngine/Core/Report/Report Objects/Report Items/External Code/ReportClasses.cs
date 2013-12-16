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
using System.Xml;


namespace Reporting.Rdl
{
	///<summary>
	/// Contains information about which classes to instantiate during report initialization.
	/// These instances can then be used in expressions throughout the report.
	///</summary>
	[Serializable]
	internal class Classes : ReportLink, IEnumerable
	{
        List<ReportClass> _Items;			// list of report class

		internal Classes(ReportDefn r, ReportLink p, XmlNode xNode) : base(r, p)
		{
            _Items = new List<ReportClass>();
			// Loop thru all the child nodes
			foreach(XmlNode xNodeLoop in xNode.ChildNodes)
			{
				if (xNodeLoop.NodeType != XmlNodeType.Element)
					continue;
				if (xNodeLoop.Name == "Class")
				{
					ReportClass rc = new ReportClass(r, this, xNodeLoop);
					_Items.Add(rc);
				}
			}
			if (_Items.Count == 0)
				OwnerReport.rl.LogError(8, "For Classes at least one Class is required.");
			else
                _Items.TrimExcess();
		}
		
		internal ReportClass this[string s]
		{
			get 
			{
				foreach (ReportClass rc in _Items)
				{
					if (rc.InstanceName.Nm == s)
						return rc;
				}
				return null;
			}
		}

		override internal void FinalPass()
		{
			foreach (ReportClass rc in _Items)
			{
				rc.FinalPass();
			}
			return;
		}

		internal void Load(Report rpt)
		{
			foreach (ReportClass rc in _Items)
			{
				rc.Load(rpt);
			}
			return;
		}

        internal List<ReportClass> Items
		{
			get { return  _Items; }
		}
		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return _Items.GetEnumerator();
		}

		#endregion
	}

    ///<summary>
    /// ReportClass represents the Class report element. 
    ///</summary>
    [Serializable]
    internal class ReportClass : ReportLink
    {
        string _ClassName;		// The name of the class
        Name _InstanceName;		// The name of the variable to assign the class to.
        // This variable can be used in expressions
        // throughout the report.

        internal ReportClass(ReportDefn r, ReportLink p, XmlNode xNode)
            : base(r, p)
        {
            _ClassName = null;
            _InstanceName = null;

            // Loop thru all the child nodes
            foreach (XmlNode xNodeLoop in xNode.ChildNodes)
            {
                if (xNodeLoop.NodeType != XmlNodeType.Element)
                    continue;
                switch (xNodeLoop.Name)
                {
                    case "ClassName":
                        _ClassName = xNodeLoop.InnerText;
                        break;
                    case "InstanceName":
                        _InstanceName = new Name(xNodeLoop.InnerText);
                        break;
                    default:
                        break;
                }
            }
            if (_ClassName == null)
                OwnerReport.rl.LogError(8, "Class ClassName is required but not specified.");

            if (_InstanceName == null)
                OwnerReport.rl.LogError(8, "Class InstanceName is required but not specified or invalid for " + _ClassName == null ? "<unknown name>" : _ClassName);
        }

        override internal void FinalPass()
        {
            return;
        }

        internal object Load(Report rpt)
        {
            WorkClass wc = GetWC(rpt);
            if (wc.bCreateFailed)		// We only try to create once.
                return wc.Instance;

            if (wc.Instance != null)	// Already loaded
                return wc.Instance;

            if (OwnerReport.CodeModules == null)	// nothing to load against
                return null;

            // Load an instance of the object
            string err = "";
            try
            {
                Type tp = OwnerReport.CodeModules[_ClassName];
                if (tp != null)
                {
                    System.Reflection.Assembly asm = tp.Assembly;
                    wc.Instance = asm.CreateInstance(_ClassName, false);
                }
                else
                    err = "Class not found.";
            }
            catch (Exception e)
            {
                wc.Instance = null;
                err = e.Message;
            }

            if (wc.Instance == null)
            {
                string e = String.Format("Unable to create instance of class {0}.  {1}",
                    _ClassName, err);
                if (rpt == null)
                    OwnerReport.rl.LogError(4, e);
                else
                    rpt.rl.LogError(4, e);
                wc.bCreateFailed = true;
            }
            return wc.Instance;
        }

        internal string ClassName
        {
            get { return _ClassName; }
        }

        internal Name InstanceName
        {
            get { return _InstanceName; }
        }

        internal object Instance(Report rpt)
        {
            return Load(rpt);			// load if necessary
        }
        private WorkClass GetWC(Report rpt)
        {
            if (rpt == null)
                return new WorkClass();

            WorkClass wc = rpt.Cache.Get(this, "wc") as WorkClass;
            if (wc == null)
            {
                wc = new WorkClass();
                rpt.Cache.Add(this, "wc", wc);
            }
            return wc;
        }

        private void RemoveWC(Report rpt)
        {
            rpt.Cache.Remove(this, "wc");
        }

        class WorkClass
        {
            internal object Instance;
            internal bool bCreateFailed;
            internal WorkClass()
            {
                Instance = null;	// 
                bCreateFailed = false;
            }
        }
    }
}
