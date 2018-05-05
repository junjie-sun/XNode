// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using XNode;
using XNode.Client;

namespace Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            var serviceProxyManager = new ServiceProxyManager();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new AutofacModule(serviceProxyManager));
            var container = containerBuilder.Build();

            new XNodeBootstrap().Run(new LoggerFactory(), serviceProxyManager, container);

            Console.ReadLine();
        }
    }
}
