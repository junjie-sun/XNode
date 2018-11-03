// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using XNode.Autofac;
using XNode.Logging;
using XNode.Serializer.MsgPack;
using XNode.Serializer.ProtoBuf;
using Microsoft.Extensions.Logging;
using System.IO;
using XNode.Server.Configuration;
using Microsoft.Extensions.Configuration;
using XNode.Security;
using XNode.Server.Processors;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Zipkin;
using XNode.Zipkin;
using XNode.Communication.DotNetty;

namespace XNode.Server.Console.XNodeServer
{
    public static class XNodeServerTest
    {
        public static void Test(string host, int port)
        {
            LoggerManager.ServerLoggerFactory.AddConsole(LogLevel.Information);

            string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
            var configRoot = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();

            var serviceAuthorizer = new DefaultServiceAuthorizer(configRoot.GetDefaultServiceAuthorizeConfig(), LoggerManager.ServerLoggerFactory);
            var loginValidator = new DefaultLoginValidator(configRoot.GetDefaultLoginValidatorConfig(), LoggerManager.ServerLoggerFactory);

            IFileProvider fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
            ChangeToken.OnChange(() => fileProvider.Watch("config.json"), () =>
            {
                configRoot.Reload();
                serviceAuthorizer.LoadConfig(configRoot.GetDefaultServiceAuthorizeConfig());
                loginValidator.LoadConfig(configRoot.GetDefaultLoginValidatorConfig());
            });

            new ZipkinBootstrapper("XNodeDemoServer", System.Net.IPAddress.Parse(host), (short)port)
                .ZipkinAt("192.168.87.131")
                .WithSampleRate(1.0)
                .Start();

            //var serviceProvider = CreateDefaultServiceProvider();
            //var serviceProvider = CreateAutofacServiceProvider();
            //serviceProvider.LoggerFactory = LoggerManager.ServerLoggerFactory;

            var serverConfig = configRoot.GetServerConfig();
            var nodeServer = new NodeServerBuilder()
                //.ConfigServerInfo(host, port)
                .ApplyConfig(serverConfig)
                //.ConfigServiceProvider(serviceProvider)
                //.ConfigSerializer(new MsgPackSerializer(LoggerManager.ServerLoggerFactory))
                .ConfigSerializer(new ProtoBufSerializer(LoggerManager.ServerLoggerFactory))
                .ConfigLoginValidator(loginValidator)
                .AddServiceProcessor(new ZipkinProcessor())
                .AddServiceProcessor(new ServiceAuthorizeProcessor(serviceAuthorizer))
                .UseDotNetty(serverConfig.ServerInfo)
                .UseAutofac(GetAutofacContainer())
                .Build();

            nodeServer.OnStarted += (arg) =>
            {
                System.Console.WriteLine("Server started:");
                System.Console.WriteLine($"Host={arg.Host}, Port={arg.Port}");
                var list = arg.Routes.Where(r => r.Enabled).Select(r => new { r.ServiceName }).Distinct();
                foreach (var item in list)
                {
                    System.Console.WriteLine(item.ServiceName);
                }

                //nodeServer.Disable(10002);
                //nodeServer.Enable(10002);
            };

            nodeServer.OnStopped += (arg) =>
            {
                System.Console.WriteLine("Server stopped:");
                System.Console.WriteLine($"Host={arg.Host}, Port={arg.Port}");
                var list = arg.Routes.Where(r => r.Enabled).Select(r => new { r.ServiceName }).Distinct();
                foreach (var item in list)
                {
                    System.Console.WriteLine(item.ServiceName);
                }
            };

            nodeServer.StartAsync().Wait();

            System.Console.ReadLine();
            nodeServer.StopAsync().Wait();
            System.Console.WriteLine("Stop");
        }

        private static IServiceProvider CreateDefaultServiceProvider()
        {
            return new DefaultServiceProvider()
                .RegistService(new CustomerService())
                .RegistService(typeof(IOrderService), typeof(OrderService));
        }

        private static IServiceProvider CreateAutofacServiceProvider()
        {
            IContainer container = GetAutofacContainer();

            return new AutofacServiceProvider(container);
        }

        private static IContainer GetAutofacContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<CustomerService>()
                .SingleInstance();
            builder.RegisterType<OrderService>()
                .As<IOrderService>()
                .SingleInstance();

            var container = builder.Build();
            return container;
        }
    }
}
