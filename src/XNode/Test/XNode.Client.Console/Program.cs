// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using XNode.Communication;

namespace XNode.Client.Console
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
            string localHost = "10.246.84.159";
            //string localHost = "192.168.2.105";
            int localPort = 9011;
            if (args != null && args.Length > 0)
            {
                try
                {
                    host = args[0];
                    port = int.Parse(args[1]);
                    localHost = args[2];
                    localPort = int.Parse(args[3]);
                }
                catch { }
            }

            //CommunicationClientTest.Test(host, port, localHost, localPort);
            //XNodeWithoutClientTest.Test(host, port, localHost, localPort);
            //XNodeClientTest.Test(host, port, localHost, localPort);
            //XNodeServiceProxyManagerTest.Test(host, port, localHost, localPort);
            XNodeAutofacTest.Test(host, port, localHost, localPort);
            //SimpleClientTest.Test();

            System.Console.ReadLine();
        }
    }
}