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
using System.Reflection;


namespace Reporting.Rdl
{
	///<summary>
	/// CodeModules definition and processing.
	///</summary>
	[Serializable]
	internal class CodeModules : ReportLink, IEnumerable
	{
        List<CodeModule> _Items;			// list of code module

		internal CodeModules(ReportDefn r, ReportLink p, XmlNode xNode) : base(r, p)
		{
            _Items = new List<CodeModule>();
			// Loop thru all the child nodes
			foreach(XmlNode xNodeLoop in xNode.ChildNodes)
			{
				if (xNodeLoop.NodeType != XmlNodeType.Element)
					continue;
				if (xNodeLoop.Name == "CodeModule")
				{
					CodeModule cm = new CodeModule(r, this, xNodeLoop);
					_Items.Add(cm);
				}
				else
				{
					// don't know this element - log it
					OwnerReport.rl.LogError(4, "Unknown CodeModules element '" + xNodeLoop.Name + "' ignored.");
				}
			}
			if (_Items.Count == 0)
				OwnerReport.rl.LogError(8, "For CodeModules at least one CodeModule is required.");
			else
                _Items.TrimExcess();
		}
		/// <summary>
		/// Return the Type given a class name.  Searches the CodeModules that are specified
		/// in the report.
		/// </summary>
		internal Type this[string s]
		{
			get 
			{
				Type tp=null;
                if (s == string.Empty)
                    return null;
				try
				{
					// loop thru all the codemodules looking for the assembly
					//  that contains this type
					foreach (CodeModule cm in _Items)
					{
						Assembly a = cm.LoadedAssembly();
						if (a != null)
						{
							tp = a.GetType(s,false,true);
							if (tp != null)
								break;
						}
					}
				}
				catch(Exception ex) 
				{
					OwnerReport.rl.LogError(4, string.Format("Exception finding type. {0}", ex.Message));
				}
				return tp;
			}
		}
		
		override internal void FinalPass()
		{
			foreach (CodeModule cm in _Items)
			{
				cm.FinalPass();
			}
			return;
		}

		internal void LoadModules()
		{
			foreach (CodeModule cm in _Items)
			{
				cm.LoadedAssembly();
			}
		}

        internal List<CodeModule> Items
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
    /// CodeModule definition and processing.
    ///</summary>
    [Serializable]
    internal class CodeModule : ReportLink
    {
        string _CodeModule;	// Name of the code module to load
        [NonSerialized]
        Assembly _LoadedAssembly = null;	// 
        [NonSerialized]
        bool bLoadFailed = false;

        internal CodeModule(ReportDefn r, ReportLink p, XmlNode xNode)
            : base(r, p)
        {
            _CodeModule = xNode.InnerText;
            //Added from Forums, User: Solidstore http://www.fyireporting.com/forum/viewtopic.php?t=905
            if (!_CodeModule.Contains(",")) // if not a full assembly reference
                if (!_CodeModule.ToLower().EndsWith(".dll")) // check .dll ending
                    _CodeModule += ".dll";
        }

        internal Assembly LoadedAssembly()
        {
            if (bLoadFailed)		// We only try to load once.
                return null;

            if (_LoadedAssembly == null)
            {
                try
                {
                    _LoadedAssembly = Utility.Assembly.AssemblyLoadFrom(_CodeModule);
                }
                catch (Exception e)
                {
                    OwnerReport.rl.LogError(4, String.Format("CodeModule {0} failed to load.  {1}",
                        _CodeModule, e.Message));
                    bLoadFailed = true;
                }
            }
            return _LoadedAssembly;
        }

        override internal void FinalPass()
        {
            return;
        }

        internal string CdModule
        {
            get { return _CodeModule; }
            set { _CodeModule = value; }
        }
    }
}
