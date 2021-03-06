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
	/// DataInstanceElement definition and processing.
	///</summary>
	public enum DataInstanceElementOutputEnum
	{
		Output,			// Indicates the list instances should appear in the output
		NoOutput		// Indicates the list instances should not appear in the output		
	}

	public class DataInstanceElementOutput
	{
        static public DataInstanceElementOutputEnum GetStyle(string s)
        {
            return GetStyle(s, null);
        }
		static internal DataInstanceElementOutputEnum GetStyle(string s, ReportLog rl)
		{
			DataInstanceElementOutputEnum rs;

			switch (s)
			{		
				case "Output":
					rs = DataInstanceElementOutputEnum.Output;
					break;
				case "NoOutput":
					rs = DataInstanceElementOutputEnum.NoOutput;
					break;
				default:		
                    if (rl != null)
					    rl.LogError(4, "Unknown DataInstanceElementOutput '" + s + "'.  Output assumed.");
					rs = DataInstanceElementOutputEnum.Output;
					break;
			}
			return rs;
		}
	}

}
