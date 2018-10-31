// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Autofac.Extras.DynamicProxy;
using Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service;
using System;
using System.IO;
using System.Text;
using XNode.Autofac;
using XNode.Client;
using XNode.Client.Configuration;
using XNode.Client.ServiceCallers;
using XNode.Communication;
using XNode.Communication.DotNetty;
using XNode.Logging;
using XNode.Serializer.ProtoBuf;
using XNode.Server;
using XNode.Server.Configuration;
using XNode.Zipkin;
using Zipkin;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please input enter to start customer server.");
            Console.ReadLine();

            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            //配置服务端日志工厂，为了能看到服务调用细节，此处将日志级别设置为Information
            LoggerManager.ServerLoggerFactory.AddConsole(LogLevel.Information);

            //配置客户端日志工厂，为了能看到服务调用细节，此处将日志级别设置为Information
            LoggerManager.ClientLoggerFactory.AddConsole(LogLevel.Information);

            //加载配置文件
            string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
            var configRoot = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();

            var serviceProxyManager = new ServiceProxyManager();
            var container = GetAutofacContainer(serviceProxyManager);

            #region 客户端配置

            var clientConfig = configRoot.GetClientConfig();

            var serializer = new ProtoBufSerializer(LoggerManager.ClientLoggerFactory);

            var serviceCaller = new ServiceCallerBuilder()
                .Append(new ZipkinCaller(serializer))       //添加ZipkinCaller
                .UseDefault()
                .Build();

            if (clientConfig.ServiceProxies != null)
            {
                foreach (var config in clientConfig.ServiceProxies)
                {
                    serviceProxyManager
                        .Regist(config, serviceCaller)
                        .AddClients(
                            new NodeClientBuilder()
                                .ConfigConnections(config.Connections)
                                .ConfigSerializer(serializer)
                                .UseDotNetty()
                                .Build()
                        );
                }
            }

            #endregion

            #region 服务端配置

            var serverConfig = configRoot.GetServerConfig();

            //配置服务
            var nodeServer = new NodeServerBuilder()
                .ApplyConfig(serverConfig)
                .ConfigSerializer(new ProtoBufSerializer(LoggerManager.ServerLoggerFactory))
                .AddServiceProcessor(new ZipkinProcessor())     //添加ZipkinProcessor
                .UseAutofac(container)
                .UseDotNetty(serverConfig.ServerInfo)
                .Build();

            #endregion

            //Zipkin配置
            new ZipkinBootstrapper("CustomerServer")
                .ZipkinAt("192.168.108.131")
                .WithSampleRate(1.0)
                .Start();

            try
            {
                //连接服务
                serviceProxyManager.ConnectAsync().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    if (e is NetworkException netEx)
                    {
                        Console.WriteLine($"Connect has net error. Host={netEx.Host}, Port={netEx.Port}, Message={netEx.Message}");
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            //启动服务
            nodeServer.StartAsync().Wait();

            Console.ReadLine();

            //关闭服务连接
            serviceProxyManager.CloseAsync();

            //关闭DotNetty事件循环
            BootstrapManager.Disable();

            //关闭服务
            nodeServer.StopAsync();
        }

        private static IContainer GetAutofacContainer(IServiceProxyManager serviceProxyManager)
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new ServiceProxyInterceptor(serviceProxyManager));
            builder.RegisterType<CustomerService>()
                .As<ICustomerService>()
                .SingleInstance();
            builder.RegisterType<OrderService>()
                .As<IOrderService>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(ServiceProxyInterceptor))
                .SingleInstance();

            var container = builder.Build();
            return container;
        }
    }
}
