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

namespace Reporting.Rdl.Utility
{
    public sealed class FileSystem
    {
        public static Uri XmlFileExists(string file)
        {
            return FileExists(file, "xml");
        }

        private static Uri FileExists(string file, string fileType)
        {
            if (!file.EndsWith(fileType, StringComparison.InvariantCultureIgnoreCase))
                file += "." + fileType;

            string d0 = RdlEngineConfig.DirectoryLoadedFrom;
            string d1 = AppDomain.CurrentDomain.BaseDirectory;
            string d2 = AppDomain.CurrentDomain.RelativeSearchPath;
            return FileExistsFrom(file, d0, d1, d2);
        }

        public static Uri FileExistsFrom(string file, params string[] dirs)
        {
            foreach (string path in dirs)
            {
                if (path != string.Empty)
                {
                    Uri fullFile = new Uri(Path.Combine(path, file));
                    if (File.Exists(fullFile.LocalPath))
                        return fullFile;
                }
            }
            // ok check to see if we can load without any directory
            return File.Exists(file) ? new Uri(file) : null;

            //for (int i = 0; i < dir.Length; i++)
            //{
            //    if (dir[i] == null || dir[i] == string.Empty)
            //        continue;

            //    string f = dir[i] + file;
            //    if (File.Exists(f))
            //        return f;
            //}

        }
    }
}
