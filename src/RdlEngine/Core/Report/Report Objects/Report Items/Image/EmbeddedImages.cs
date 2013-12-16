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
	/// The collection of embedded images in the Report.
	///</summary>
	[Serializable]
	internal class EmbeddedImages : ReportLink
	{
        List<EmbeddedImage> _Items;			// list of EmbeddedImage

		internal EmbeddedImages(ReportDefn r, ReportLink p, XmlNode xNode) : base(r, p)
		{
            _Items = new List<EmbeddedImage>();
			// Loop thru all the child nodes
			foreach(XmlNode xNodeLoop in xNode.ChildNodes)
			{
				if (xNodeLoop.NodeType != XmlNodeType.Element)
					continue;
				if (xNodeLoop.Name == "EmbeddedImage")
				{
					EmbeddedImage ei = new EmbeddedImage(r, this, xNodeLoop);
					_Items.Add(ei);
				}
				else
					this.OwnerReport.rl.LogError(4, "Unknown Report element '" + xNodeLoop.Name + "' ignored.");
			}
			if (_Items.Count == 0)
				OwnerReport.rl.LogError(8, "For EmbeddedImages at least one EmbeddedImage is required.");
			else
                _Items.TrimExcess();
		}
		
		override internal void FinalPass()
		{
			foreach (EmbeddedImage ei in _Items)
			{
				ei.FinalPass();
			}
			return;
		}

        internal List<EmbeddedImage> Items
		{
			get { return  _Items; }
		}
	}

    ///<summary>
    /// The defintion of an embedded images, including the actual image data and type.
    ///</summary>
    [Serializable]
    internal class EmbeddedImage : ReportLink
    {
        Name _Name;			// Name of the image.
        string _MIMEType;	// The MIMEType for the image. Valid values are:
        // image/bmp, image/jpeg, image/gif, image/png, image/xpng.
        string _ImageData;	// Base-64 encoded image data.		

        internal EmbeddedImage(ReportDefn r, ReportLink p, XmlNode xNode)
            : base(r, p)
        {
            _Name = null;
            _MIMEType = null;
            _ImageData = null;
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
                    case "MIMEType":
                        _MIMEType = xNodeLoop.InnerText;
                        break;
                    case "ImageData":
                        _ImageData = xNodeLoop.InnerText;
                        break;
                    default:
                        this.OwnerReport.rl.LogError(4, "Unknown Report element '" + xNodeLoop.Name + "' ignored.");
                        break;
                }
            }

            if (this.Name == null)
            {
                OwnerReport.rl.LogError(8, "EmbeddedImage Name is required but not specified.");
            }
            else
            {
                try
                {
                    OwnerReport.LUEmbeddedImages.Add(this.Name.Nm, this);		// add to referenceable embedded images
                }
                catch		// Duplicate name
                {
                    OwnerReport.rl.LogError(4, "Duplicate EmbeddedImage  name '" + this.Name.Nm + "' ignored.");
                }
            }
            if (_MIMEType == null)
                OwnerReport.rl.LogError(8, "EmbeddedImage MIMEType is required but not specified for " + (this.Name == null ? "'name not specified'" : this.Name.Nm));

            if (_ImageData == null)
                OwnerReport.rl.LogError(8, "EmbeddedImage ImageData is required but not specified for " + (this.Name == null ? "'name not specified'" : this.Name.Nm));
        }

        override internal void FinalPass()
        {
            return;
        }

        internal Name Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        internal string MIMEType
        {
            get { return _MIMEType; }
            set { _MIMEType = value; }
        }

        internal string ImageData
        {
            get { return _ImageData; }
            set { _ImageData = value; }
        }
    }
}
