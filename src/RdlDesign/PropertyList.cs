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
using System.Text;
using System.Drawing;
using System.ComponentModel;            // need this for the properties metadata
using System.Xml;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Reporting.Rdl;

namespace Reporting.RdlDesign
{
    /// <summary>
    /// PropertyList - The List Properties
    /// </summary>
    [TypeConverter(typeof(PropertyListConverter))]
    internal class PropertyList : PropertyDataRegion
    {
        internal PropertyList(DesignXmlDraw d, DesignCtl dc, List<XmlNode> ris)
            : base(d, dc, ris)
        {
        }
        [CategoryAttribute("List"),
            DescriptionAttribute("Grouping data allows each repeated list region to represent a summarization of the rows in the group.")]
        public PropertyGrouping Grouping
        {
            get
            {
                return new PropertyGrouping(this);
            }
        }
        [CategoryAttribute("List"),
            DescriptionAttribute("Sorting controls the order of the repeated list regions.")]
        public PropertySorting Sorting
        {
            get
            {
                return new PropertySorting(this);
            }
        }
        #region XML
        [CategoryAttribute("XML"),
   DescriptionAttribute("The name to use for the data element for each instance of this list when exporting to XML.")]
        public string DataInstanceName
        {
            get
            {
                return GetValue("DataInstanceName", "");
            }
            set
            {
                SetValue("DataInstanceName", value);
            }
        }
        [CategoryAttribute("XML"),
   DescriptionAttribute("Determines whether list instances appear in the XML.")]
        public DataInstanceElementOutputEnum DataInstanceElementOutput
        {
            get
            {
                string v = GetValue("DataInstanceElementOutput", "Output");
                return Reporting.Rdl.DataInstanceElementOutput.GetStyle(v);
            }
            set
            {
                SetValue("DataInstanceElementOutput", value.ToString());
            }
        }
        #endregion
    }
    internal class PropertyListConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is PropertyList)
            {
                PropertyList pe = value as PropertyList;
                return pe.Name;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

    }

}
