using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XNode.Autofac;
using XNode.Communication.DotNetty;
using XNode.Logging;
using XNode.Security;
using XNode.Serializer.ProtoBuf;
using XNode.Server.Configuration;
using XNode.Server.Processors;

namespace XNode.Server.Console.XNodeServer
{
    public static class SimpleServerTest
    {
        public static void Test()
        {
            LoggerManager.ServerLoggerFactory.AddConsole(LogLevel.Information);

            string path = Path.Combine(Directory.GetCurrentDirectory(), "config_simple.json");
            var configRoot = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();

            var serviceAuthorizer = new DefaultServiceAuthorizer(configRoot.GetDefaultServiceAuthorizeConfig(), LoggerManager.ServerLoggerFactory);
            var loginValidator = new DefaultLoginValidator(configRoot.GetDefaultLoginValidatorConfig(), LoggerManager.ServerLoggerFactory);

            var serverConfig = configRoot.GetServerConfig();
            var nodeServer = new NodeServerBuilder()
                .ApplyConfig(serverConfig)
                //.ConfigSerializer(new MsgPackSerializer(LoggerManager.ServerLoggerFactory))
                .ConfigSerializer(new ProtoBufSerializer(LoggerManager.ServerLoggerFactory))
                .ConfigLoginValidator(loginValidator)
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

        private static IServiceProvider CreateAutofacServiceProvider()
        {
            IContainer container = GetAutofacContainer();

            return new AutofacServiceProvider(container);
        }

        private static IContainer GetAutofacContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<SimpleService>()
                .As<ISimpleService>()
                .SingleInstance();

            var container = builder.Build();
            return container;
        }
    }
}
