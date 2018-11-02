using Autofac;
using Autofac.Extras.DynamicProxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XNode.Autofac;
using XNode.Client;
using XNode.Client.Configuration;
using XNode.Client.ServiceCallers;
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
            LoggerManager.ClientLoggerFactory.AddConsole(LogLevel.Error);

            string path = Path.Combine(Directory.GetCurrentDirectory(), "config_simple.json");
            var configRoot = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();

            var globalConfig = configRoot.GetGlobalConfig();
            GlobalSettings.Apply(globalConfig);

            IServiceProxyManager serviceProxyManager = StartClient(configRoot);
            IContainer container = GetAutofacContainer(serviceProxyManager);
            INodeServer nodeServer = StartServer(configRoot, container);

            System.Console.ReadLine();
            nodeServer.StopAsync().Wait();
            System.Console.WriteLine("Stop");
        }

        private static IServiceProxyManager StartClient(IConfigurationRoot configRoot)
        {
            var clientConfig = configRoot.GetClientConfig();

            var serviceProxyManager = new ServiceProxyManager();

            if (clientConfig.ServiceProxies == null || clientConfig.ServiceProxies.Count == 0)
            {
                return serviceProxyManager;
            }

            //var serializer = new MsgPackSerializer(LoggerManager.ClientLoggerFactory);
            var serializer = new ProtoBufSerializer(LoggerManager.ClientLoggerFactory);

            var serviceCaller = new ServiceCallerBuilder()
                .UseDefault()
                .Build();

            foreach (var config in clientConfig.ServiceProxies)
            {
                var serviceProxy = new ServiceProxy(
                config.ProxyName,
                config?.Services,
                serviceCaller)
                .AddServices(config.ProxyTypes)
                .AddClients(
                    new NodeClientBuilder()
                        .ConfigConnections(config.Connections)
                        .ConfigSerializer(serializer)
                        .ConfigLoginHandler(new DefaultLoginHandler(configRoot.GetDefaultLoginHandlerConfig(config.ProxyName), serializer))
                        .UseDotNetty()
                        .Build()
                );
                serviceProxyManager.Regist(serviceProxy);
            }

            serviceProxyManager.ConnectAsync().Wait();

            return serviceProxyManager;
        }

        private static INodeServer StartServer(IConfigurationRoot configRoot, IContainer container)
        {
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
                .UseAutofac(container)
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
            return nodeServer;
        }

        private static IContainer GetAutofacContainer(IServiceProxyManager serviceProxyManager)
        {
            var builder = new ContainerBuilder();

            builder.Register(c => new ServiceProxyInterceptor(serviceProxyManager));
            builder.RegisterType<SimpleService2>()
               .As<ISimpleService2>()
               .EnableInterfaceInterceptors()       //接口拦截
               .InterceptedBy(typeof(ServiceProxyInterceptor))
               .SingleInstance();

            builder.RegisterType<SimpleService>()
                .As<ISimpleService>()
                .SingleInstance();

            var container = builder.Build();
            return container;
        }
    }
}
