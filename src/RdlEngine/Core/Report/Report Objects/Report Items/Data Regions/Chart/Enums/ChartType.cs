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
	/// Chart type and marker definition and processing.
	///</summary>
	internal enum ChartTypeEnum
	{
		Column,
		Bar,
		Line,
		Pie,
		Scatter,
		Bubble,
		Area,
		Doughnut,
		Stock,
        Map,
		Unknown
	}

	internal enum ChartMarkerEnum			// used for line point markers
	{									// the order reflects the usage order as well (see GetSeriesMarkers in ChartBase)
		Circle=0,
		Square=1,
		Triangle=2,
		Plus=3,
		X=4,
		Diamond=5,						// TODO: diamond doesn't draw well in small size
		Count=6,						// must equal the number of shapes
		None=Count+1,					// above Count
		Bubble=None+1,					// above None
        Line=Bubble+1                    // For scatter lines GJL
	}

	internal class ChartType
	{
		static internal ChartTypeEnum GetStyle(string s)
		{
			ChartTypeEnum ct;

			switch (s)
			{		
				case "Column":
					ct = ChartTypeEnum.Column;
					break;
				case "Bar":
					ct = ChartTypeEnum.Bar;
					break;
				case "Line":
					ct = ChartTypeEnum.Line;
					break;
				case "Pie":
					ct = ChartTypeEnum.Pie;
					break;
				case "Scatter":
					ct = ChartTypeEnum.Scatter;
					break;
				case "Bubble":
					ct = ChartTypeEnum.Bubble;
					break;
				case "Area":
					ct = ChartTypeEnum.Area;
					break;
				case "Doughnut":
					ct = ChartTypeEnum.Doughnut;
					break;
				case "Stock":
					ct = ChartTypeEnum.Stock;
					break;
                case "Map":
                    ct = ChartTypeEnum.Map;
                    break;
				default:		// unknown type
					ct = ChartTypeEnum.Unknown;
					break;
			}
			return ct;
		}
	}

}
