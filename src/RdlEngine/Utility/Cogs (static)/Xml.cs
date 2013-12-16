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
using System.Xml.Xsl;
using System.Text;
using System.IO;
using System.Drawing;			// for Color class
using System.Reflection;

namespace Reporting.Rdl
{
	///<summary>
	/// Some utility classes consisting entirely of static routines.
	///</summary>
	public sealed class Xml
	{
		static internal Color ColorFromHtml(string colorString, Color defaultColor)
		{
			return ColorFromHtml(colorString, defaultColor, null);
		}

		static internal Color ColorFromHtml(string colorString, Color defaultColor, ReportLog rl)
		{
			Color c;
			try 
			{
				c = ColorTranslator.FromHtml(colorString);
			}
			catch 
			{
				c = defaultColor;
				if (rl != null)
					rl.LogError(4, string.Format("'{0}' is an invalid HTML color.", colorString));
			}
			return c;
		}

		/// <summary>
		/// Takes an arbritrary string and returns a string that can 
        /// be embedded in an
		/// XML element.  For example, '&lt;' is changed to '&amp;lt;'
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		static public string ToXmlAnsi(string s)
		{
			StringBuilder rs = new StringBuilder(s.Length);

			foreach (char c in s)
			{
				if (c == '<')
					rs.Append("&lt;");
				else if (c == '&')
					rs.Append("&amp;");
				else if ((int) c <= 127)	// in ANSI range
					rs.Append(c);
				else
					rs.Append("&#" + ((int) c).ToString() + ";");
			}

			return rs.ToString();
		}
        /// <summary>
        /// Takes an arbritrary string and returns a string 
        /// that can be handles unicode
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static public string ToHtmlAnsi(string s)
        {
            StringBuilder rs = new StringBuilder(s.Length);

            foreach (char c in s)
            {
                if ((int)c <= 127)	// in ANSI range
                    rs.Append(c);
                else
                    rs.Append("&#" + ((int)c).ToString() + ";");
            }

            return rs.ToString();
        }

		static internal void XslTransform(string xslFile, string inXml, Stream outResult)
		{
			XmlDocument xDoc = new XmlDocument();
			xDoc.LoadXml(inXml);

            XslCompiledTransform xslt = new XslCompiledTransform();

			//Load the stylesheet.
			xslt.Load(xslFile);

			xslt.Transform(xDoc,null,outResult);
           
			return;
		}

		static internal string EscapeXmlAttribute(string s)
		{
			string result;

			result = s.Replace("'", "&#39;");

			return result;
		}
		

    }
}
