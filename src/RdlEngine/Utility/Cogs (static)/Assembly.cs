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
using System.Collections.Generic;
using System.Text;
using System.IO;
using Reflection = System.Reflection;
using Reporting.Rdl;

namespace Reporting.Rdl.Utility
{
    public sealed class Assembly
    {
        /// <summary>
        /// Loads assembly from file; tries up to 3 time; load with name, load from BaseDirectory, 
        /// and load from BaseDirectory concatenated with Relative directory.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static internal Reflection.Assembly AssemblyLoadFrom(string s)
        {
            Reflection.Assembly ra = null;
            try
            {	// try 1) loading just from name
                ra = Reflection.Assembly.LoadFrom(s);
            }
            catch
            {	// try 2) loading from the various directories available
                string d0 = RdlEngineConfig.DirectoryLoadedFrom;
                string d1 = AppDomain.CurrentDomain.BaseDirectory;
                string d2 = AppDomain.CurrentDomain.RelativeSearchPath;
                if (d2 == null || d2 == string.Empty)
                    ra = AssemblyLoadFromPvt(Path.GetFileName(s), d0, d1);
                else
                    ra = AssemblyLoadFromPvt(Path.GetFileName(s), d0, d1, d2);
            }

            return ra;
        }

        static Reflection.Assembly AssemblyLoadFromPvt(string file, params string[] dir)
        {
            Reflection.Assembly ra = null;
            for (int i = 0; i < dir.Length; i++)
            {
                if (dir[i] == null)
                    continue;
                //Josh: 6:23:10 Changed to System.IO.Path.Combine()
                string f = System.IO.Path.Combine(dir[i], file);
                try
                {
                    ra = Reflection.Assembly.LoadFrom(f);
                    if (ra != null)             // don't really need this as call will throw exception when it fails
                        break;
                }
                catch
                {
                    if (i + 1 == dir.Length)
                    {  // on last try just plain load of the file
                        ra = Reflection.Assembly.Load(file);
                    }
                }
            }
            return ra;
        }

        static internal Reflection.MethodInfo GetMethod(Type t, string method, Type[] argTypes)
        {
            if (t == null || method == null)
                return null;

            Reflection.MethodInfo mInfo = t.GetMethod(method,
               Reflection.BindingFlags.IgnoreCase |
               Reflection.BindingFlags.Public | 
               Reflection.BindingFlags.Static, null,       // TODO: use Laxbinder class
               argTypes, null);
            if (mInfo == null)
                mInfo = t.GetMethod(method, argTypes);  // be less specific and try again (Code VB functions don't always get caught?)
            if (mInfo == null)
            {
                // Try to find method in base classes --- fix thanks to jonh
                Type b = t.BaseType;
                while (b != null)
                {
                    //                    mInfo = b.GetMethod(method, argTypes);
                    mInfo = b.GetMethod(method,
                        Reflection.BindingFlags.IgnoreCase |
                        Reflection.BindingFlags.Public |
                        Reflection.BindingFlags.Static |
                        Reflection.BindingFlags.DeclaredOnly,
                        null, argTypes, null);
                    if (mInfo != null)
                        break;
                    b = b.BaseType;
                }
            }
            return mInfo;
        }

        static internal Type GetTypeFromTypeCode(TypeCode tc)
        {
            Type t = null;
            switch (tc)
            {
                case TypeCode.Boolean:
                    t = Type.GetType("System.Boolean");
                    break;
                case TypeCode.Byte:
                    t = Type.GetType("System.Byte");
                    break;
                case TypeCode.Char:
                    t = Type.GetType("System.Char");
                    break;
                case TypeCode.DateTime:
                    t = Type.GetType("System.DateTime");
                    break;
                case TypeCode.Decimal:
                    t = Type.GetType("System.Decimal");
                    break;
                case TypeCode.Double:
                    t = Type.GetType("System.Double");
                    break;
                case TypeCode.Int16:
                    t = Type.GetType("System.Int16");
                    break;
                case TypeCode.Int32:
                    t = Type.GetType("System.Int32");
                    break;
                case TypeCode.Int64:
                    t = Type.GetType("System.Int64");
                    break;
                case TypeCode.Object:
                    t = Type.GetType("System.Object");
                    break;
                case TypeCode.SByte:
                    t = Type.GetType("System.SByte");
                    break;
                case TypeCode.Single:
                    t = Type.GetType("System.Single");
                    break;
                case TypeCode.String:
                    t = Type.GetType("System.String");
                    break;
                case TypeCode.UInt16:
                    t = Type.GetType("System.UInt16");
                    break;
                case TypeCode.UInt32:
                    t = Type.GetType("System.UInt32");
                    break;
                case TypeCode.UInt64:
                    t = Type.GetType("System.UInt64");
                    break;
                default:
                    t = Type.GetType("Object");
                    break;
            }
            return t;
        }

        static internal object GetConstFromTypeCode(TypeCode tc)
        {
            object t = null;
            switch (tc)
            {
                case TypeCode.Boolean:
                    t = (object)true;
                    break;
                case TypeCode.Byte:
                    t = (object)Byte.MinValue;
                    break;
                case TypeCode.Char:
                    t = (object)Char.MinValue;
                    break;
                case TypeCode.DateTime:
                    t = (object)DateTime.MinValue;
                    break;
                case TypeCode.Decimal:
                    t = (object)Decimal.MinValue;
                    break;
                case TypeCode.Double:
                    t = (object)Double.MinValue;
                    break;
                case TypeCode.Int16:
                    t = (object)Int16.MinValue;
                    break;
                case TypeCode.Int32:
                    t = (object)Int32.MinValue;
                    break;
                case TypeCode.Int64:
                    t = (object)Int64.MinValue;
                    break;
                case TypeCode.Object:
                    t = (object)"";
                    break;
                case TypeCode.SByte:
                    t = (object)SByte.MinValue;
                    break;
                case TypeCode.Single:
                    t = (object)Single.MinValue;
                    break;
                case TypeCode.String:
                    t = (object)"";
                    break;
                case TypeCode.UInt16:
                    t = (object)UInt16.MinValue;
                    break;
                case TypeCode.UInt32:
                    t = (object)UInt32.MinValue;
                    break;
                case TypeCode.UInt64:
                    t = (object)UInt64.MinValue;
                    break;
                default:
                    t = (object)"";
                    break;
            }
            return t;
        }
    }
}
