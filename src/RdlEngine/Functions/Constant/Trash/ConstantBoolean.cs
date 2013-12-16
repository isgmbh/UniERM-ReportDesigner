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
using System.IO;
using System.Reflection;


using Reporting.Rdl;


namespace Reporting.Rdl
{
    ///// <summary>
    ///// Boolean constant
    ///// </summary>
    //[Serializable]
    //internal class ConstantBoolean : Constant, IExpr
    //{
    //    bool _Value;		// value of the constant

    //    /// <summary>
    //    /// A boolean constant: i.e. true or false.
    //    /// </summary>
    //    public ConstantBoolean(string v) 
    //    {
    //        _Value = Convert.ToBoolean(v);
    //    }

    //    public ConstantBoolean(bool v) 
    //    {
    //        _Value = v;
    //    }

    //    public TypeCode GetTypeCode()
    //    {
    //        return TypeCode.Boolean;
    //    }

    //    public string EvaluateString(Report rpt, Row row)
    //    {
    //        return Convert.ToString(_Value);
    //    }
		
    //    public double EvaluateDouble(Report rpt, Row row)
    //    {
    //        return Convert.ToDouble(_Value);
    //    }
		
    //    public decimal EvaluateDecimal(Report rpt, Row row)
    //    {
    //        return Convert.ToDecimal(_Value);
    //    }

    //    public int EvaluateInt32(Report rpt, Row row)
    //    {
    //        return Convert.ToInt32(_Value);
    //    }

    //    public DateTime EvaluateDateTime(Report rpt, Row row)
    //    {
    //        return Convert.ToDateTime(_Value);
    //    }

    //    public bool EvaluateBoolean(Report rpt, Row row)
    //    {
    //        return _Value;
    //    }
    //}
}
