// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XNode.Client.Console.Services;
using XNode.Client.NodeClientContainers;
using XNode.Client.ServiceCallers;
using XNode.Communication;
using XNode.Communication.DotNetty;
using XNode.Logging;
using XNode.Serializer.MsgPack;
using XNode.Serializer.ProtoBuf;

namespace XNode.Client.Console
{
    public static class XNodeServiceProxyManagerTest
    {
        private static IServiceProxy serviceProxy;

        public static void Test(string host, int port, string localHost, int? localPort)
        {
            System.Console.ReadLine();

            var proxyName = "XNodeDemoService";
            LoggerManager.ClientLoggerFactory.AddConsole(LogLevel.Debug);
            var serializer = new ProtoBufSerializer(LoggerManager.ClientLoggerFactory);
            var nodeClientParametersList = new List<NodeClientParameters>()
            {
                new NodeClientParameters()
                {
                    Host = host,
                    Port = port,
                    LocalHost = localHost,
                    LocalPort = localPort,
                    Serializer = serializer,
                    Communication = new DotNettyClient(host, port, localHost, localPort)
                }
            };

            var nodeClientContainer = new DefaultNodeClientContainer();
            foreach (var c in nodeClientParametersList)
            {
                nodeClientContainer.Add(new DefaultNodeClient(c));
            }

            var serviceList = new List<Type>() { typeof(ICustomerService), typeof(IOrderService) };

            var serviceProxyManager = new ServiceProxyManager();

            serviceProxy = new ServiceProxy(proxyName, null, null, nodeClientContainer)
                .AddService<ICustomerService>()
                .AddService<OrderService>();

            serviceProxyManager.Regist(serviceProxy);

            serviceProxyManager.ConnectAsync().Wait();

            CallGetServiceName().Wait();
            CallAddCustomer().Wait();
            CallGetCustomer().Wait();
            CallQueryCustomer().Wait();
            CallRemoveCustomer().Wait();
            CallQueryCustomer().Wait();
            CallRemoveAllCustomer().Wait();
            CallQueryCustomer().Wait();

            CallAddOrder().Wait();
            CallQueryOrder().Wait();

            System.Console.ReadLine();

            serviceProxyManager.CloseAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    System.Console.WriteLine(task.Exception.InnerException.Message);
                    return;
                }
                System.Console.WriteLine("Closed");
            });
        }

        #region  GetServiceName

        private async static Task CallGetServiceName()
        {
            System.Console.WriteLine("------------------------------GetServiceName----------------------------------");

            try
            {
                var result = await serviceProxy.CallRemoteServiceAsync(GetCustomerServiceActionType("GetServiceName"), null) as string;
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
                var customer1 = new Customer()
                {
                    Id = 1,
                    Name = "Michael",
                    Birthday = new DateTime(1999, 1, 1)
                };
                var task1 = serviceProxy.CallRemoteServiceAsync(GetCustomerServiceActionType("AddCustomer"), new object[] { customer1 });

                var customer2 = new Customer()
                {
                    Id = 2,
                    Name = "Jane",
                    Birthday = new DateTime(2001, 5, 10)
                };
                var task2 = serviceProxy.CallRemoteServiceAsync(GetCustomerServiceActionType("AddCustomer"), new object[] { customer2 });

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

        private async static Task CallGetCustomer()
        {
            System.Console.WriteLine("------------------------------GetCustomer----------------------------------");

            try
            {
                var customer = await serviceProxy.CallRemoteServiceAsync(GetCustomerServiceActionType("GetCustomer"), new object[] { 1 }) as Customer;
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
                var customers = await serviceProxy.CallRemoteServiceAsync(GetCustomerServiceActionType("QueryCustomer"), null) as List<Customer>;
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

        private async static Task CallRemoveCustomer()
        {
            System.Console.WriteLine("------------------------------RemoveCustomer----------------------------------");

            try
            {
                await serviceProxy.CallRemoteServiceAsync(GetCustomerServiceActionType("RemoveCustomer"), new object[] { 1 });

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

        private async static Task CallRemoveAllCustomer()
        {
            System.Console.WriteLine("------------------------------RemoveAllCustomer----------------------------------");

            try
            {
                await serviceProxy.CallRemoteServiceAsync(GetCustomerServiceActionType("RemoveAllCustomer"), null);

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
                var task1 = serviceProxy.CallRemoteServiceAsync(GetOrderServiceActionType("AddOrder"), new object[] { order1 });

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
                var task2 = serviceProxy.CallRemoteServiceAsync(GetOrderServiceActionType("AddOrder"), new object[] { order2 });

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
                var orders = await serviceProxy.CallRemoteServiceAsync(GetOrderServiceActionType("QueryOrder"), new object[] { 1, "Michael" }) as List<Order>;
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
