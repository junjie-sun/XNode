// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Autofac.Extras.DynamicProxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XNode.Autofac;
using XNode.Client;
using XNode.Client.Configuration;
using XNode.Communication;
using XNode.Logging;
using XNode.Mix.Console.Services;
using XNode.Security;
using XNode.Serializer.ProtoBuf;
using XNode.Server;
using XNode.Server.Configuration;
using XNode.Client.ServiceCallers;
using XNode.Server.Processors;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using XNode.Zipkin;
using Zipkin;
using System.Threading;
using XNode.Communication.DotNetty;

namespace XNode.Mix.Console
{
    class Program
    {
        private static IContainer container;

        static void Main(string[] args)
        {
            System.Console.InputEncoding = Encoding.UTF8;
            System.Console.OutputEncoding = Encoding.UTF8;

            var dir = Directory.GetCurrentDirectory();
            var fileName = "config.json";
            string path = Path.Combine(dir, fileName);

            var configRoot = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();

            var name = configRoot.GetValue<string>("name");
            var globalConfig = configRoot.GetGlobalConfig();
            GlobalSettings.Apply(globalConfig);

            var serviceProxyManager = new ServiceProxyManager();
            container = GetAutofacContainer(serviceProxyManager);

            #region Client配置

            var clientConfig = configRoot.GetClientConfig();

            LoggerManager.ClientLoggerFactory.AddConsole(LogLevel.Information);

            var clientSerializer = new ProtoBufSerializer(LoggerManager.ClientLoggerFactory);

            var serviceCaller = new ServiceCallerBuilder()
                //.Append(new ZipkinCaller(clientSerializer))
                .UseRetry()
                .UseDefault()
                .Build();

            if (clientConfig.ServiceProxies != null)
            {
                foreach (var config in clientConfig.ServiceProxies)
                {
                    var loggerFactory = LoggerManager.ClientLoggerFactory;
                    serviceProxyManager
                        .Regist(config, serviceCaller)
                        .AddClients(
                            new NodeClientBuilder()
                                .ConfigConnections(config.Connections)
                                .ConfigSerializer(clientSerializer)
                                .ConfigLoginHandler(new DefaultLoginHandler(configRoot.GetDefaultLoginHandlerConfig(config.ProxyName), clientSerializer))
                                .UseDotNetty()
                                .Build()
                        );
                }
            }

            #endregion

            #region Server配置

            LoggerManager.ServerLoggerFactory.AddConsole(LogLevel.Information);

            var loginValidator = new DefaultLoginValidator(configRoot.GetDefaultLoginValidatorConfig(), LoggerManager.ServerLoggerFactory);
            var serviceAuthorizer = new DefaultServiceAuthorizer(configRoot.GetDefaultServiceAuthorizeConfig(), LoggerManager.ServerLoggerFactory);

            var serverConfig = configRoot.GetServerConfig();
            var nodeServer = new NodeServerBuilder()
                .ApplyConfig(serverConfig)
                .ConfigSerializer(new ProtoBufSerializer(LoggerManager.ServerLoggerFactory))
                .ConfigLoginValidator(loginValidator)
                //.AddServiceProcessor(new ZipkinProcessor())
                .AddServiceProcessor(new ServiceAuthorizeProcessor(serviceAuthorizer))
                .UseDotNetty(serverConfig.ServerInfo)
                .UseAutofac(container)
                .Build();

            #endregion

            #region 配置监视

            IFileProvider fileProvider = new PhysicalFileProvider(dir);
            ChangeToken.OnChange(() => fileProvider.Watch(fileName), () =>
            {
                configRoot.Reload();
                serviceAuthorizer.LoadConfig(configRoot.GetDefaultServiceAuthorizeConfig());
                loginValidator.LoadConfig(configRoot.GetDefaultLoginValidatorConfig());
            });

            #endregion

            #region 启动

            System.Console.WriteLine("Please enter: 1-Start services and test, 2-Only start services.");
            var isTest = System.Console.ReadLine();

            nodeServer.StartAsync().Wait();

            try
            {
                serviceProxyManager.ConnectAsync().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    if (e is NetworkException netEx)
                    {
                        System.Console.WriteLine($"Connect has net error. Host={netEx.Host}, Port={netEx.Port}, Message={netEx.Message}");
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            #endregion

            #region Test

            if (isTest == "1")
            {
                //new ZipkinBootstrapper(name)
                //    .ZipkinAt("192.168.87.131")
                //    .WithSampleRate(1.0)
                //    .Start();

                CallAddCustomer().Wait();
                CallGetCustomer();
                CallQueryCustomer().Wait();

                CallAddOrder().Wait();
                CallQueryOrder().Wait();

                CallGetOrders().Wait();

                CallRemoveCustomer();
                CallQueryCustomer().Wait();
                CallRemoveAllCustomer();
                CallQueryCustomer().Wait();

                CallSaveCustomerPhoto().Wait();
                CallGetCustomerPhoto().Wait();

                //Test();
            }
            else
            {
                //new ZipkinBootstrapper(name, System.Net.IPAddress.Parse(serverConfig.ServerInfo.Host), (short)serverConfig.ServerInfo.Port)
                //    .ZipkinAt("192.168.87.131")
                //    .WithSampleRate(1.0)
                //    .Start();
            }

            #endregion

            #region 关闭

            System.Console.ReadLine();

            nodeServer.StopAsync();

            serviceProxyManager.CloseAsync();

            #endregion

            System.Console.ReadLine();
        }

        private static void Test()
        {
            var tasks = new List<Task>();
            for (var i = 0; i < 10000; i++)
            {
                tasks.Add(CallGetOrders());
            }

            Task.WaitAll(tasks.ToArray());

            System.Console.WriteLine("Done.");
            System.Console.ReadLine();
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

        #region  AddCustomer

        private async static Task CallAddCustomer()
        {
            System.Console.WriteLine("------------------------------AddCustomer----------------------------------");

            try
            {
                var customerService = container.Resolve<ICustomerService>();

                var customer1 = new Customer()
                {
                    Id = 1,
                    Name = "Michael",
                    Birthday = new DateTime(1999, 1, 1)
                };
                var task1 = customerService.AddCustomer(customer1);

                var customer2 = new Customer()
                {
                    Id = 2,
                    Name = "Jane",
                    Birthday = new DateTime(2001, 5, 10)
                };
                var task2 = customerService.AddCustomer(customer2);

                await Task.WhenAll(task1, task2);

                await task1;

                System.Console.WriteLine($"Add customer success");
            }
            catch (RequestTimeoutExcption ex)
            {
                System.Console.WriteLine($"Timeout: RequestId={ex.Request.Id}");
            }
            catch (ServiceCallException ex)
            {
                System.Console.WriteLine($"Service call exception: ExceptionId={ex.ExceptionId}, ExceptionMessage={ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
        }

        #endregion

        #region  GetCustomer

        private static void CallGetCustomer()
        {
            System.Console.WriteLine("------------------------------GetCustomer----------------------------------");

            try
            {
                var customerService = container.Resolve<ICustomerService>();
                var customer = customerService.GetCustomer(1);
                System.Console.WriteLine($"Result=Id: {customer.Id}, Name: {customer.Name}, Birthday: {customer.Birthday.ToString("yyyy-MM-dd")}");
            }
            catch (RequestTimeoutExcption ex)
            {
                System.Console.WriteLine($"Timeout: RequestId={ex.Request.Id}");
            }
            catch (ServiceCallException ex)
            {
                System.Console.WriteLine($"Service call exception: ExceptionId={ex.ExceptionId}, ExceptionMessage={ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
        }

        #endregion

        #region  QueryCustomer

        private async static Task CallQueryCustomer()
        {
            System.Console.WriteLine("------------------------------QueryCustomer----------------------------------");

            try
            {
                var customerService = container.Resolve<ICustomerService>();
                var customers = await customerService.QueryCustomer();

                if (customers == null)
                {
                    return;
                }

                foreach (var customer in customers)
                {
                    System.Console.WriteLine($"Result=Id: {customer.Id}, Name: {customer.Name}, Birthday: {customer.Birthday.ToString("yyyy-MM-dd")}");
                }
            }
            catch (RequestTimeoutExcption ex)
            {
                System.Console.WriteLine($"Timeout: RequestId={ex.Request.Id}");
            }
            catch (ServiceCallException ex)
            {
                System.Console.WriteLine($"Service call exception: ExceptionId={ex.ExceptionId}, ExceptionMessage={ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
        }

        #endregion

        #region  RemoveCustomer

        private static void CallRemoveCustomer()
        {
            System.Console.WriteLine("------------------------------RemoveCustomer----------------------------------");

            try
            {
                var customerService = container.Resolve<ICustomerService>();
                customerService.RemoveCustomer(1);

                System.Console.WriteLine($"Remove customer success");
            }
            catch (RequestTimeoutExcption ex)
            {
                System.Console.WriteLine($"Timeout: RequestId={ex.Request.Id}");
            }
            catch (ServiceCallException ex)
            {
                System.Console.WriteLine($"Service call exception: ExceptionId={ex.ExceptionId}, ExceptionMessage={ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
        }

        #endregion

        #region  RemoveAllCustomer

        private static void CallRemoveAllCustomer()
        {
            System.Console.WriteLine("------------------------------RemoveAllCustomer----------------------------------");

            try
            {
                var customerService = container.Resolve<ICustomerService>();
                customerService.RemoveAllCustomer();

                System.Console.WriteLine($"Remove all customer success");
            }
            catch (RequestTimeoutExcption ex)
            {
                System.Console.WriteLine($"Timeout: RequestId={ex.Request.Id}");
            }
            catch (ServiceCallException ex)
            {
                System.Console.WriteLine($"Service call exception: ExceptionId={ex.ExceptionId}, ExceptionMessage={ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
        }

        #endregion

        #region  AddOrder

        private async static Task CallAddOrder()
        {
            System.Console.WriteLine("------------------------------AddOrder----------------------------------");

            try
            {
                var orderService = container.Resolve<IOrderService>();

                var order1 = new Order()
                {
                    Id = 1,
                    CustomerId = 1,
                    CustomerName = "Michael",
                    Detail = new List<OrderDetail>()
                    {
                        new OrderDetail()
                        {
                            OrderId = 1,
                            GoodsId = 1001,
                            GoodsName = "TestGoods1"
                        },
                        new OrderDetail()
                        {
                            OrderId = 1,
                            GoodsId = 1002,
                            GoodsName = "TestGoods2"
                        }
                    }
                };
                var task1 = orderService.AddOrder(order1);

                var order2 = new Order()
                {
                    Id = 2,
                    CustomerId = 1,
                    CustomerName = "Michael",
                    Detail = new List<OrderDetail>()
                    {
                        new OrderDetail()
                        {
                            OrderId = 2,
                            GoodsId = 1010,
                            GoodsName = "TestGoods10"
                        }
                    }
                };
                var task2 = orderService.AddOrder(order2);

                await Task.WhenAll(task1, task2);
                System.Console.WriteLine($"Add order success");
            }
            catch (RequestTimeoutExcption ex)
            {
                System.Console.WriteLine($"Timeout: RequestId={ex.Request.Id}");
            }
            catch (ServiceCallException ex)
            {
                System.Console.WriteLine($"Service call exception: ExceptionId={ex.ExceptionId}, ExceptionMessage={ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
        }

        #endregion

        #region  QueryOrder

        private async static Task CallQueryOrder()
        {
            System.Console.WriteLine("------------------------------QueryOrder----------------------------------");

            try
            {
                var orderService = container.Resolve<IOrderService>();
                var orders = await orderService.QueryOrder(1, "Michael");
                if (orders == null)
                {
                    return;
                }
                foreach (var order in orders)
                {
                    System.Console.WriteLine($"Order: Id: {order.Id}, CustomerId: {order.CustomerId}, CustomerName: {order.CustomerName}");
                    System.Console.WriteLine("Order Detail:");
                    foreach (var detail in order.Detail)
                    {
                        System.Console.WriteLine($"OrderId={detail.OrderId}, GoodsId={detail.GoodsId}, GoodsName={detail.GoodsName}");
                    }
                    System.Console.WriteLine();
                }
            }
            catch (RequestTimeoutExcption ex)
            {
                System.Console.WriteLine($"Timeout: RequestId={ex.Request.Id}");
            }
            catch (ServiceCallException ex)
            {
                System.Console.WriteLine($"Service call exception: ExceptionId={ex.ExceptionId}, ExceptionMessage={ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
        }

        #endregion

        #region GetOrders

        private async static Task CallGetOrders()
        {
            System.Console.WriteLine("------------------------------GetOrders----------------------------------");

            try
            {
                var customerService = container.Resolve<ICustomerService>();
                var orders = await customerService.GetOrders(1);
                if (orders == null)
                {
                    return;
                }
                foreach (var order in orders)
                {
                    System.Console.WriteLine($"Order: Id: {order.Id}, CustomerId: {order.CustomerId}, CustomerName: {order.CustomerName}");
                    System.Console.WriteLine("Order Detail:");
                    foreach (var detail in order.Detail)
                    {
                        System.Console.WriteLine($"OrderId={detail.OrderId}, GoodsId={detail.GoodsId}, GoodsName={detail.GoodsName}");
                    }
                    System.Console.WriteLine();
                }
            }
            catch (RequestTimeoutExcption ex)
            {
                System.Console.WriteLine($"Timeout: RequestId={ex.Request.Id}");
            }
            catch (ServiceCallException ex)
            {
                System.Console.WriteLine($"Service call exception: ExceptionId={ex.ExceptionId}, ExceptionMessage={ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
        }

        #endregion

        #region  SaveCustomerPhoto

        private async static Task CallSaveCustomerPhoto()
        {
            System.Console.WriteLine("------------------------------SaveCustomerPhoto----------------------------------");

            try
            {
                var customerService = container.Resolve<ICustomerService>();

                var result = await customerService.SaveCustomerPhoto(new byte[] { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 });

                System.Console.WriteLine($"Save customer photo result: {result}");
            }
            catch (RequestTimeoutExcption ex)
            {
                System.Console.WriteLine($"Timeout: RequestId={ex.Request.Id}");
            }
            catch (ServiceCallException ex)
            {
                System.Console.WriteLine($"Service call exception: ExceptionId={ex.ExceptionId}, ExceptionMessage={ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
        }

        #endregion

        #region  GetCustomerPhoto

        private async static Task CallGetCustomerPhoto()
        {
            System.Console.WriteLine("------------------------------GetCustomerPhoto----------------------------------");

            try
            {
                var customerService = container.Resolve<ICustomerService>();

                var data = await customerService.GetCustomerPhoto();

                System.Console.WriteLine("Customer photo data:");

                foreach (var d in data)
                {
                    System.Console.Write(d);
                }

                System.Console.WriteLine();
            }
            catch (RequestTimeoutExcption ex)
            {
                System.Console.WriteLine($"Timeout: RequestId={ex.Request.Id}");
            }
            catch (ServiceCallException ex)
            {
                System.Console.WriteLine($"Service call exception: ExceptionId={ex.ExceptionId}, ExceptionMessage={ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
        }

        #endregion
    }
}
