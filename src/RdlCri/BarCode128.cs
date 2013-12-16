/*
 * This file is part of UniERM ReportDesigner, based on
 * the work of Kim Sheffield and the fyiReporting project. 
 * 
 * © 2011 Inpro-Soft GmbH (http://www.unierm.de)
 * 
 * Prior Copyrights:
 ====================================================================
   Copyright (C) 2004-2008  fyiReporting Software, LLC

   This file is part of the fyiReporting RDL project.
	
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.


   For additional information, email info@fyireporting.com or visit
   the website www.fyiReporting.com.
*/
/*------This File was edited: Nearly everything in this File is new, parts are based on the BarcodeEAN13-File -----*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;            // need this for the properties metadata
using System.Xml;
using System.Text.RegularExpressions;

using Reporting.Rdl;

namespace Reporting.Rdl
{
    /// <summary>
    /// BarCode128: basiert auf/based on:
    ///     http://www.jtbarton.com/Barcodes/Code128.aspx#FreeCode128
    /// </summary>
    public class BarCode128: ICustomReportItem
    {
        // Encoding arrays: digit encoding 

        static readonly string[] Barcode128definitionTable = 
            {"11011001100", "11001101100", "11001100110", "10010011000", "10010001100", //0-4
             "10001001100", "10011001000", "10011000100", "10001100100", "11001001000", //5-9
             "11001000100", "11000100100", "10110011100", "10011011100", "10011001110", //10-14
             "10111001100", "10011101100", "10011100110", "11001110010", "11001011100", //15-19
             "11001001110", "11011100100", "11001110100", "11101101110", "11101001100", //20-24
             "11100101100", "11100100110", "11101100100", "11100110100", "11100110010", //25-29
             "11011011000", "11011000110", "11000110110", "10100011000", "10001011000", //30-34
             "10001000110", "10110001000", "10001101000", "10001100010", "11010001000", //35-39
             "11000101000", "11000100010", "10110111000", "10110001110", "10001101110", //40-44
             "10111011000", "10111000110", "10001110110", "11101110110", "11010001110", //45-49
             "11000101110", "11011101000", "11011100010", "11011101110", "11101011000", //50-54
             "11101000110", "11100010110", "11101101000", "11101100010", "11100011010", //55-59
             "11101111010", "11001000010", "11110001010", "10100110000", "10100001100", //60-64
             "10010110000", "10010000110", "10000101100", "10000100110", "10110010000", //65-69
             "10110000100", "10011010000", "10011000010", "10000110100", "10000110010", //70-74
             "11000010010", "11001010000", "11110111010", "11000010100", "10001111010", //75-79
             "10100111100", "10010111100", "10010011110", "10111100100", "10011110100", //80-84
             "10011110010", "11110100100", "11110010100", "11110010010", "11011011110", //85-89
             "11011110110", "11110110110", "10101111000", "10100011110", "10001011110", //90-94
             "10111101000", "10111100010", "11110101000", "11110100010", "10111011110", //95-99
             "10111101110", "11101011110", "11110101110", "11010000100", "11010010000", //100-104
             "11010011100", "1100011101011"}; //105-106

        string _Barcode;    

        static public readonly float OptimalHeight = 15f;          // Optimal height at magnification 1    
        static public readonly float OptimalWidth = 37f;            // Optimal width at mag 1
        static readonly float AspectRatio = OptimalHeight / OptimalWidth;   // h / w: dimension at magnification factor 1
        static readonly float ModuleWidth = 0.50f;             // module width in mm at mag factor 1
        static readonly float FontHeight = 8;                  // Font height at mag factor 1
        static readonly int LeftQuietZoneModules = 11;          // # of modules in left quiet zone  
        static readonly int RightQuietZoneModules = 7;          // # of modules in left quiet zone  
        static readonly int GuardModules=3;                     // # of modules in left and right guard
        static readonly int ManufacturingModules=7*6;           // # of modules in manufacturing
        static readonly int CenterBarModules=5;                 // # of modules in center bar
        static readonly int ProductModules=7*6;                 // # of modules in product + checksum
        static readonly int ModulesToManufacturingStart =
            LeftQuietZoneModules + GuardModules;
        static readonly int ModulesToManufacturingEnd =
            ModulesToManufacturingStart + ManufacturingModules;
        static readonly int ModulesToProductStart =
            ModulesToManufacturingEnd + CenterBarModules;
        static readonly int ModulesToProductEnd =
            ModulesToProductStart + ProductModules;
        static readonly int TotalModules = ModulesToProductEnd + GuardModules + RightQuietZoneModules;

        public BarCode128()        // Need to be able to create an instance
        {
        }

        #region ICustomReportItem Members
        /// <summary>
        /// Runtime: Draw the BarCode
        /// </summary>
        /// <param name="bm">Bitmap to draw the barcode in.</param>
        public void DrawImage(System.Drawing.Bitmap bm)
        {
            string upcode = _Barcode;

            DrawImage(bm, upcode);
        }

        /// <summary>
        /// Design time: Draw a hard coded BarCode for design time;  Parameters can't be
        /// relied on since they aren't available.
        /// </summary>
        /// <param name="bm"></param>
        public void DrawDesignerImage(System.Drawing.Bitmap bm)
        {
            DrawImage(bm, "0123456789"); //"0123456789");
        }
        
        /// <summary>
        /// DrawImage given a Bitmap and a upcode does all the drawing work.
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="upcode"></param>
        internal void DrawImage(System.Drawing.Bitmap bm, string upcode)
        {
            string barPattern = this.GetEncoding(upcode);
            Graphics g = null;
            g = Graphics.FromImage(bm);
            float mag = GetMagnification(g, bm.Width, bm.Height);

            bool NummerAnzeigen = false;

            float barWidth = ModuleWidth * mag;
            float barHeight = OptimalHeight * mag;
            float fontHeight = FontHeight * mag;
            float fontHeightMM = fontHeight / 72.27f * 25.4f;
            Font f = null;
            try
            {
                g.PageUnit = System.Drawing.GraphicsUnit.Millimeter;

                // Fill in the background with white
                g.FillRectangle(Brushes.White, 0, 0, bm.Width, bm.Height);

                // Draw the human readable portion of the barcode
                f = new Font("Arial", fontHeight);

                // Draw the left guard text (i.e. 2nd digit of the NumberSystem)
                string wc = upcode;

                // Draw the bars
                int barCount = LeftQuietZoneModules;
                foreach (char bar in barPattern)
                {
                    if (bar == '1')
                    {
                        //float bh = ((barCount > ModulesToManufacturingStart && barCount < ModulesToManufacturingEnd) ||
                        //            (barCount > ModulesToProductStart && barCount < ModulesToProductEnd)) ?
                        //            barHeight - fontHeightMM : barHeight;
                        float bh = barHeight;
                        if (((barWidth * barCount) <= ((((barWidth * ((LeftQuietZoneModules+barPattern.ToCharArray().Length) - LeftQuietZoneModules)) / 2) + (barWidth * LeftQuietZoneModules)) - (g.MeasureString(wc, f).Width / 2))
                            || (barWidth * barCount) >= ((((barWidth * ((LeftQuietZoneModules + barPattern.ToCharArray().Length) - LeftQuietZoneModules)) / 2) + (barWidth * LeftQuietZoneModules)) + (g.MeasureString(wc, f).Width / 2)))
                            && NummerAnzeigen == true)
                        {
                            bh = barHeight;
                        }
                        else if (NummerAnzeigen == true)
                        {
                            bh = barHeight - fontHeightMM;
                        }

                        g.FillRectangle(Brushes.Black, barWidth * barCount, 0, barWidth, bh);
                    } 
                    barCount++;
                }

                if (NummerAnzeigen == true)
                {
                g.DrawString(wc, f, Brushes.Black,
                    new PointF((((barWidth * (barCount - LeftQuietZoneModules)) / 2) + (barWidth * LeftQuietZoneModules)) - (g.MeasureString(wc, f).Width / 2), barHeight - fontHeightMM));
                }                                
            }
            finally
            {
                if (f != null)
                    f.Dispose();
                if (g != null)
                    g.Dispose();
            }
        }

        /// <summary>
        /// BarCode isn't a DataRegion
        /// </summary>
        /// <returns></returns>
        public bool IsDataRegion()
        {
            return false;
        }

        /// <summary>
        /// Set the properties;  No validation is done at this time.
        /// </summary>
        /// <param name="props"></param>
        public void SetProperties(IDictionary<string, object> props)
        {
            object pv;
            try
            {
                pv = props["Barcode"];
                _Barcode = pv.ToString();
            }
            catch (KeyNotFoundException)
            {
                throw new Exception("Barcode property must be specified");
            }

            return;
        }
        /// <summary>
        /// Design time call: return string with <CustomReportItem> ... </CustomReportItem> syntax for 
        /// the insert.  The string contains a variable {0} which will be substituted with the
        /// configuration name.  This allows the name to be completely controlled by
        /// the configuration file.
        /// </summary>
        /// <returns></returns>
        public string GetCustomReportItemXml()              
        {
            return "<CustomReportItem><Type>{0}</Type>" +
                string.Format("<Height>{0}mm</Height><Width>{1}mm</Width>", OptimalHeight, OptimalWidth) + 
                "<CustomProperties>" +
                "<CustomProperty>" +
                "<Name>Barcode</Name>" +
                "<Value></Value>" +
                "</CustomProperty>" +
                "</CustomProperties>" +
                "</CustomReportItem>";
        }

        /// <summary>
        /// Return an instance of the class representing the properties.
        /// This method is called at design time;
        /// </summary>
        /// <returns></returns>
        public object GetPropertiesInstance(XmlNode iNode)
        {
            BarCodeProperties bcp = new BarCodeProperties(this, iNode);
            foreach (XmlNode n in iNode.ChildNodes)
            {
                if (n.Name != "CustomProperty") 
                    continue;
                string pname = this.GetNamedElementValue(n, "Name", "");
                switch (pname)
                {
                    case "Barcode":
                        bcp.SetValue(GetNamedElementValue(n, "Value", ""));
                        break;
                    default:
                        break;
                }
            }

            return bcp;
        }
    
        /// <summary>
        /// Set the custom properties given the properties object
        /// </summary>
        /// <param name="node"></param>
        /// <param name="inst"></param>
        public void SetPropertiesInstance(XmlNode node, object inst)
        {
            node.RemoveAll();       // Get rid of all properties

            BarCodeProperties bcp = inst as BarCodeProperties;
            if (bcp == null)
                return;

            // Value
            CreateChild(node, "Barcode", bcp.Value);
        }

        void CreateChild(XmlNode node, string nm, string val)
        {
            XmlDocument xd = node.OwnerDocument;
            XmlNode cp = xd.CreateElement("CustomProperty");
            node.AppendChild(cp);

            XmlNode name = xd.CreateElement("Name");
            name.InnerText = nm;
            cp.AppendChild(name);

            XmlNode v = xd.CreateElement("Value");
            v.InnerText = val;
            cp.AppendChild(v);
        }

        public void Dispose()
        {
            return;
        }

        #endregion

        float GetMagnification(Graphics g, int width, int height)
        {
            float r = (float)height / (float)width;
            if (r <= BarCode128.AspectRatio)
            {   // height is the limiting value
                r = BarCode128.MmYFromPixel(g, height) / BarCode128.OptimalHeight;
            }
            else
            {   // width is the limiting value
                r = BarCode128.MmXFromPixel(g, width) / BarCode128.OptimalWidth;
            }
            // Set the magnification limits
            //    Specification says 80% to 200% magnification allowed
            if (r < .8f)
                r = .8f;
            else if (r > 2f)
                r = 2;

            return r;
        }

        /// <summary>
        /// GetEncoding returns a string representing the on/off bars.  It should be passed
        /// a string of 12 characters: Country code 2 chars + Manufacturers code 5 chars +
        /// Product code 5 chars.   
        /// </summary>
        /// <param name="upccode"></param>
        /// <returns></returns>
        string GetEncoding(string upccode)
        {
            if (upccode == null)
                throw new ArgumentNullException("upccode");
            
            StringBuilder sb = new StringBuilder();
            string barcode = StringToBarcode(upccode);

            int digit;
            foreach (char bar in barcode)
            {
                digit = (int)bar;
                if (digit == 207) { digit = 32; }
                if (digit >= 32 && digit <= 126) { digit = digit - 32; }
                if (digit >= 195) { digit = digit - 100; }
                if (digit >= 0 && digit <= 106)
                {
                    sb.Append(BarCode128.Barcode128definitionTable[digit]);
                }
            }


            return sb.ToString();
        }
        
        public static string StringToBarcode(string value)
        {
            // Parameters : a string
            // Return     : a string which give the bar code when it is dispayed with CODE128.TTF font
            // 			 : an empty string if the supplied parameter is no good
            int charPos, minCharPos;
            int currentChar, checksum;
            bool isTableB = true, isValid = true;
            string returnValue = string.Empty;

            if (value.Length > 0)
            {

                // Check for valid characters
                for (int charCount = 0; charCount < value.Length; charCount++)
                {
                    //currentChar = char.GetNumericValue(value, charPos);
                    currentChar = (int)char.Parse(value.Substring(charCount, 1));
                    if (!(currentChar >= 32 && currentChar <= 126))
                    {
                        isValid = false;
                        break;
                    }
                }

                // Barcode is full of ascii characters, we can now process it
                if (isValid)
                {
                    charPos = 0;
                    while (charPos < value.Length)
                    {
                        if (isTableB)
                        {
                            // See if interesting to switch to table C
                            // yes for 4 digits at start or end, else if 6 digits
                            if (charPos == 0 || charPos + 4 == value.Length)
                                minCharPos = 4;
                            else
                                minCharPos = 6;


                            minCharPos = IsNumber(value, charPos, minCharPos);

                            if (minCharPos < 0)
                            {
                                // Choice table C
                                if (charPos == 0)
                                {
                                    // Starting with table C
                                    returnValue = ((char)205).ToString(); // char.ConvertFromUtf32(205);
                                }
                                else
                                {
                                    // Switch to table C
                                    returnValue = returnValue + ((char)199).ToString();
                                }
                                isTableB = false;
                            }
                            else
                            {
                                if (charPos == 0)
                                {
                                    // Starting with table B
                                    returnValue = ((char)204).ToString(); // char.ConvertFromUtf32(204);
                                }

                            }
                        }

                        if (!isTableB)
                        {
                            // We are on table C, try to process 2 digits
                            minCharPos = 2;
                            minCharPos = IsNumber(value, charPos, minCharPos);
                            if (minCharPos < 0) // OK for 2 digits, process it
                            {
                                currentChar = int.Parse(value.Substring(charPos, 2));
                                currentChar = currentChar < 95 ? currentChar + 32 : currentChar + 100;
                                returnValue = returnValue + ((char)currentChar).ToString();
                                charPos += 2;
                            }
                            else
                            {
                                // We haven't 2 digits, switch to table B
                                returnValue = returnValue + ((char)200).ToString();
                                isTableB = true;
                            }
                        }
                        if (isTableB)
                        {
                            // Process 1 digit with table B
                            returnValue = returnValue + value.Substring(charPos, 1);
                            charPos++;
                        }
                    }

                    // Calculation of the checksum
                    checksum = 0;
                    for (int loop = 0; loop < returnValue.Length; loop++)
                    {
                        currentChar = (int)char.Parse(returnValue.Substring(loop, 1));
                        currentChar = currentChar < 127 ? currentChar - 32 : currentChar - 100;
                        if (loop == 0)
                            checksum = currentChar;
                        else
                            checksum = (checksum + (loop * currentChar)) % 103;
                    }

                    // Calculation of the checksum ASCII code
                    checksum = checksum < 95 ? checksum + 32 : checksum + 100;
                    // Add the checksum and the STOP
                    returnValue = returnValue +
                        ((char)checksum).ToString() +
                        ((char)206).ToString();
                }
            }

            return returnValue;
        }


        private static int IsNumber(string InputValue, int CharPos, int MinCharPos)
        {
            // if the MinCharPos characters from CharPos are numeric, then MinCharPos = -1
            MinCharPos--;
            if (CharPos + MinCharPos < InputValue.Length)
            {
                while (MinCharPos >= 0)
                {
                    if ((int)char.Parse(InputValue.Substring(CharPos + MinCharPos, 1)) < 48
                        || (int)char.Parse(InputValue.Substring(CharPos + MinCharPos, 1)) > 57)
                    {
                        break;
                    }
                    MinCharPos--;
                }
            }
            return MinCharPos;
        }


        static internal int MmXFromPixel(Graphics g, float x)
        {
            int result = (int)(x / g.DpiX * 25.4f);	// convert to pixels

            return result;
        }

        static internal int MmYFromPixel(Graphics g, float y)
        {
            int result = (int)(y / g.DpiY * 25.4f);	// convert to pixels

            return result;
        }


        /// <summary>
        /// Calculate the checksum: (sum odd digits * 3 + sum even digits ) 
        ///   Checksum is the number that must be added to sum to make it 
        ///   evenly divisible by 10
        /// </summary>
        /// <param name="upccode"></param>
        /// <returns></returns>
        int CheckSum(string upccode)
        {
            int sum = 0;
            bool bOdd=false;
            foreach (char c in upccode)
            {
                int digit = (int) Char.GetNumericValue(c);
                sum += (bOdd ? digit * 3 : digit);
                bOdd = !bOdd;                       // switch every other character
            }
            int cs = 10 - (sum % 10);

            return cs == 10? 0: cs;
        }
        
        /// <summary>
        /// Get the child element with the specified name.  Return the InnerText
        /// value if found otherwise return the passed default.
        /// </summary>
        /// <param name="xNode">Parent node</param>
        /// <param name="name">Name of child node to look for</param>
        /// <param name="def">Default value to use if not found</param>
        /// <returns>Value the named child node</returns>
        string GetNamedElementValue(XmlNode xNode, string name, string def)
        {
            if (xNode == null)
                return def;

            foreach (XmlNode cNode in xNode.ChildNodes)
            {
                if (cNode.NodeType == XmlNodeType.Element &&
                    cNode.Name == name)
                    return cNode.InnerText;
            }
            return def;
        }

        /// <summary>
        /// BarCodeProperties- All properties are type string to allow for definition of
        /// a runtime expression.
        /// </summary>
        public class BarCodeProperties
        {
            string _Barcode;
            BarCode128 _bc;
            XmlNode _node;

            internal BarCodeProperties(BarCode128 bc, XmlNode node)
            {
                _bc = bc;
                _node = node;
            }

            internal void SetValue(string ns)
            {
                _Barcode = ns;
            }
            [CategoryAttribute("BarCode"),
               DescriptionAttribute("Nur Wert checksumme usw. werden berechnet.")]
            public string Value
            {
                get { return _Barcode; }
                set { _Barcode = value; _bc.SetPropertiesInstance(_node, this); }
            }
        }

       

    }
}
