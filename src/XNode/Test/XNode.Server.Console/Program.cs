// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Threading.Tasks;
using XNode.Server.Console.XNodeServer;

namespace XNode.Server.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.InputEncoding = Encoding.UTF8;
            System.Console.OutputEncoding = Encoding.UTF8;

            string host = "10.246.84.159";
            //string host = "192.168.2.105";
            int port = 9001;
            if (args != null && args.Length > 0)
            {
                try
                {
                    host = args[0];
                    port = int.Parse(args[1]);
                }
                catch { }
            }

            //CommunicationServerTest.Test(host, port);
            //XNodeServerTest.Test(host, port);
            SimpleServerTest.Test();

            System.Console.ReadLine();
        }
    }
}