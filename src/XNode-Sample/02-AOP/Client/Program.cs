// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Autofac.Extras.DynamicProxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using XNode.Autofac;
using XNode.Client;
using XNode.Client.Configuration;
using XNode.Communication;
using XNode.Communication.DotNetty;
using XNode.Logging;
using XNode.Serializer.ProtoBuf;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please input enter to begin.");
            Console.ReadLine();

            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            //配置客户端日志工厂
            LoggerManager.ClientLoggerFactory.AddConsole(LogLevel.Error);

            //加载配置文件
            string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
            var configRoot = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();

            var clientConfig = configRoot.GetClientConfig();

            var serializer = new ProtoBufSerializer(LoggerManager.ClientLoggerFactory);

            var serviceProxyManager = new ServiceProxyManager();

            //创建Autofac容器并注册服务类型
            var container = GetAutofacContainer(serviceProxyManager);

            if (clientConfig.ServiceProxies != null)
            {
                //注册服务代理
                foreach (var config in clientConfig.ServiceProxies)
                {
                    serviceProxyManager
                        .Regist(config)
                        .AddClients(
                            new NodeClientBuilder()
                                .ConfigConnections(config.Connections)
                                .ConfigSerializer(serializer)
                                .UseDotNetty()
                                .Build()
                        );
                }
            }

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

            try
            {
                //调用服务
                var sampleService = container.Resolve<ISampleService>();
                var result = sampleService.Welcome("XNode");
                Console.WriteLine(result);
            }
            catch (RequestTimeoutExcption ex)
            {
                Console.WriteLine($"Timeout: RequestId={ex.Request.Id}");
            }
            catch (ServiceCallException ex)
            {
                Console.WriteLine($"Service call exception: ExceptionId={ex.ExceptionId}, ExceptionMessage={ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.ReadLine();

            //关闭服务连接
            serviceProxyManager.CloseAsync();
        }

        private static IContainer GetAutofacContainer(IServiceProxyManager serviceProxyManager)
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new ServiceProxyInterceptor(serviceProxyManager));
            builder.RegisterType<SampleService>()
                .As<ISampleService>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(ServiceProxyInterceptor))
                .SingleInstance();

            var container = builder.Build();
            return container;
        }
    }
}
