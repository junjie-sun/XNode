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
using System.Reflection;
using System.Text;
using XNode.Autofac;
using XNode.Client;
using XNode.Client.Configuration;
using XNode.Client.ServiceCallers;
using XNode.Communication;
using XNode.Communication.DotNetty;
using XNode.Logging;
using XNode.Serializer.ProtoBuf;
using XNode.Zipkin;
using Zipkin;

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

            //Zipkin配置
            new ZipkinBootstrapper("Client")
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

            try
            {
                //调用服务
                var customerService = container.Resolve<ICustomerService>();
                var customer = customerService.GetCustomers(1).Result;
                Console.WriteLine($"Id = {customer.Id}, Name = {customer.Name}");
                Console.WriteLine("Orders:");
                foreach (var order in customer.Orders)
                {
                    Console.WriteLine($"OrderId = {order.Id}");
                    Console.WriteLine($"Detail:");
                    foreach (var detail in order.Detail)
                    {
                        Console.WriteLine($"GoodsId = {detail.GoodsId}, GoodsName = {detail.GoodsName}, Price = {detail.Price}, Amount = {detail.Amount}");
                    }
                    Console.WriteLine("-----------------------------------");
                }
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
            builder.RegisterType<CustomerService>()
                .As<ICustomerService>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(ServiceProxyInterceptor))
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
