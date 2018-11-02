// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Autofac.Extras.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using XNode.Autofac;
using XNode.Client.Console.Services;
using XNode.Communication;
using XNode.Logging;
using XNode.Serializer.MsgPack;
using XNode.Serializer.ProtoBuf;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.Configuration;
using XNode.Client.Configuration;
using XNode.Security;
using XNode.Client.ServiceCallers;
using XNode.Client.NodeClientContainers;
using XNode.Zipkin;
using Zipkin;
using XNode.Communication.DotNetty;

namespace XNode.Client.Console
{
    public static class XNodeAutofacTest
    {
        private static IContainer container;

        public static void Test(string host, int port, string localHost, int? localPort)
        {
            System.Console.ReadLine();

            //var serviceProxyManager = Init(host, port, localHost, localPort);
            var serviceProxyManager = InitWithConfig(host, port, localHost, localPort);

            System.Console.ReadLine();

            var beginTime = DateTime.Now;

            for (var i = 0; i < 1; i++)
            {
                CallGetServiceName();
                CallAddCustomer().Wait();
                CallGetCustomer();
                CallQueryCustomer().Wait();
                CallRemoveCustomer();
                CallQueryCustomer().Wait();
                CallRemoveAllCustomer();
                CallQueryCustomer().Wait();
            }

            CallAddOrder().Wait();
            CallQueryOrder().Wait();
            
            var endTime = DateTime.Now;
            System.Console.WriteLine($"Start time: {beginTime.ToString("yyyy-MM-dd HH:mm:ss")}");
            System.Console.WriteLine($"End time: {endTime.ToString("yyyy-MM-dd HH:mm:ss")} Used time: {endTime - beginTime}");

            System.Console.ReadLine();

            serviceProxyManager.CloseAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    System.Console.WriteLine(task.Exception.InnerException.Message);
                    return;
                }
                System.Console.WriteLine("Closed");
            }).ContinueWith(task => BootstrapManager.Disable());
        }

        #region  GetServiceName

        private static void CallGetServiceName()
        {
            System.Console.WriteLine("------------------------------GetServiceName----------------------------------");

            try
            {
                var customerService = container.Resolve<ICustomerService>();
                var result = customerService.GetServiceName();
                System.Console.WriteLine($"Result={result}");
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
                var customers = await customerService.QueryCustomer(null, null);

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
                var orderService = container.Resolve<OrderService>();
                
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
                var orderService = container.Resolve<OrderService>();
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

        #region 私有方法

        private static IServiceProxyManager Init(string host, int port, string localHost, int? localPort)
        {
            var proxyName = "XNodeDemoService";
            LoggerManager.ClientLoggerFactory.AddConsole(LogLevel.Information);
            var serializer = new ProtoBufSerializer(LoggerManager.ClientLoggerFactory);      //new MsgPackSerializer(LoggerManager.ClientLoggerFactory);
            var nodeClientParametersList = new List<NodeClientParameters>()
            {
                new NodeClientParameters()
                {
                    Host = host,
                    Port = port,
                    LocalHost = localHost,
                    LocalPort = localPort,
                    Serializer = serializer,
                    Communication = new DotNettyClient(host, port),
                    LoginHandler = new DefaultLoginHandler(new DefaultLoginHandlerConfig()
                    {
                        AccountName = "Test01",
                        AccountKey = "123456"
                    }, serializer)
                }
            };

            var nodeClientContainer = new DefaultNodeClientContainer();
            foreach (var c in nodeClientParametersList)
            {
                nodeClientContainer.Add(new DefaultNodeClient(c));
            }

            var serviceProxy = new ServiceProxy(proxyName, null, null, nodeClientContainer)
                .AddService<ICustomerService>()
                .AddService<OrderService>();

            var serviceProxyManager = new ServiceProxyManager();
            serviceProxyManager.Regist(serviceProxy);

            serviceProxyManager.ConnectAsync().Wait();

            var builder = new ContainerBuilder();
            builder.Register(c => new ServiceProxyInterceptor(serviceProxyManager));
            builder.RegisterType<CustomerService>()
               .As<ICustomerService>()
               .EnableInterfaceInterceptors()       //接口拦截
               .InterceptedBy(typeof(ServiceProxyInterceptor))
               .SingleInstance();
            builder.RegisterType<OrderService>()
               //.As<IOrderService>()
               .EnableClassInterceptors()           //类拦截
               .InterceptedBy(typeof(ServiceProxyInterceptor))
               .SingleInstance();

            container = builder.Build();
            return serviceProxyManager;
        }

        private static IServiceProxyManager InitWithConfig(string host, int port, string localHost, int? localPort)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");

            var configRoot = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();

            var clientConfig = configRoot.GetClientConfig();

            LoggerManager.ClientLoggerFactory.AddConsole(LogLevel.Information);

            var serviceProxyTypeList = new List<Type>() { typeof(ICustomerService), typeof(OrderService) };

            new ZipkinBootstrapper("XNodeDemoClient", System.Net.IPAddress.Parse(localHost), (short)(localPort != null ? localPort.Value : 0))
                .ZipkinAt("192.168.87.131")
                .WithSampleRate(1.0)
                .Start();

            //var serializer = new MsgPackSerializer(LoggerManager.ClientLoggerFactory);
            var serializer = new ProtoBufSerializer(LoggerManager.ClientLoggerFactory);

            var serviceCaller = new ServiceCallerBuilder()
                .Append(new ZipkinCaller(serializer))
                .UseRetry()
                .UseDefault()
                .Build();

            var serviceProxyManager = new ServiceProxyManager();

            foreach (var serviceProxyConfig in clientConfig.ServiceProxies)
            {
                var serviceProxy = new ServiceProxy(
                serviceProxyConfig.ProxyName,
                serviceProxyConfig?.Services,
                serviceCaller)
                .AddServices(serviceProxyConfig.ProxyTypes)
                .AddClients(
                    new NodeClientBuilder()
                        .ConfigConnections(serviceProxyConfig.Connections)
                        .ConfigSerializer(serializer)
                        .ConfigLoginHandler(new DefaultLoginHandler(configRoot.GetDefaultLoginHandlerConfig(serviceProxyConfig.ProxyName), serializer))
                        .UseDotNetty()
                        .Build()
                );
                serviceProxyManager.Regist(serviceProxy);
            }

            serviceProxyManager.ConnectAsync().Wait();

            var builder = new ContainerBuilder();
            builder.Register(c => new ServiceProxyInterceptor(serviceProxyManager));
            builder.RegisterType<CustomerService>()
               .As<ICustomerService>()
               .EnableInterfaceInterceptors()       //接口拦截
               .InterceptedBy(typeof(ServiceProxyInterceptor))
               .SingleInstance();
            builder.RegisterType<OrderService>()
               //.As<IOrderService>()
               .EnableClassInterceptors()           //类拦截
               .InterceptedBy(typeof(ServiceProxyInterceptor))
               .SingleInstance();

            container = builder.Build();
            return serviceProxyManager;
        }

        private static MethodInfo GetCustomerServiceActionType(string methodName)
        {
            var serviceType = typeof(ICustomerService);
            return serviceType.GetMethod(methodName);
        }

        private static MethodInfo GetOrderServiceActionType(string methodName)
        {
            var serviceType = typeof(IOrderService);
            return serviceType.GetMethod(methodName);
        }

        #endregion
    }
}
