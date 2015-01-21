/*
 * This file is part of UniERM ReportDesigner, based on reportFU by Josh Wilson,
 * the work of Kim Sheffield and the fyiReporting project. 
 * 
 * © 2012-2013 Inpro-Soft GmbH (http://www.unierm.de)
 * 
 * Prior Copyrights:
 * _________________________________________________________
 * |Copyright (C) 2010 devFU Pty Ltd, Josh Wilson and Others|
 * | (http://reportfu.org)                                  |
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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;

namespace Reporting.RdlDesign
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]
        static void Main()
        {
            // Determine if an instance is already running?
            bool firstInstance;
            string mName = "Local\\RdlDesigner441";         // !!!! warning  !!!! string needs to be changed with when release version changes
            //   can't use Assembly in this context
            System.Threading.Mutex mutex = new System.Threading.Mutex(false, mName, out firstInstance);


            //Josh: Removed
            //Added from Forum, User: solidstore http://www.fyireporting.com/forum/viewtopic.php?t=905
            // Uncomment to enable remoting if configured.
            //RemotingConfiguration.Configure(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, true); 

            if (firstInstance)
            {   // just start up the designer when we're first in line
                Application.EnableVisualStyles();
                Application.DoEvents();
                Application.Run(new RdlDesignerForm());
                Settings.Default.Save();
                return;
            }


            // Process already running.   Notify other process that is might need to open another file
            string[] args = Environment.GetCommandLineArgs();
            
            IpcChannel clientChannel = new IpcChannel("RdlClientSend");
            ChannelServices.RegisterChannel(clientChannel, false);

            RdlIpcObject ipc =
               (RdlIpcObject)Activator.GetObject(
               typeof(RdlIpcObject),
               "ipc://RdlProject/IpcCommands");


            List<string> commands = new List<string>();


            commands.Add("/a");     // signal that application should activate itself
            
            // copy all the command line arguments process received
            for (int i = 1; i < args.Length; i++)
            {
                commands.Add(args[i]);
            }

            ipc.setCommands(commands);
            //}
        }
    }
}
