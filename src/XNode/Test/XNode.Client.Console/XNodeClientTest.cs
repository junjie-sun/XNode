// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Client.Console.Services;
using XNode.Communication;
using XNode.Logging;
using XNode.Serializer.MsgPack;
using XNode.Serializer.ProtoBuf;
using XNode.ProtocolStack;
using XNode.Communication.DotNetty;

namespace XNode.Client.Console
{
    public static class XNodeClientTest
    {
        public static void Test(string host, int port, string localHost, int? localPort)
        {
            System.Console.ReadLine();

            //var proxyName = "XNodeDemoService";
            LoggerManager.ClientLoggerFactory.AddConsole(LogLevel.Debug);
            var serializer = new ProtoBufSerializer(LoggerManager.ClientLoggerFactory);     //new MsgPackSerializer(LoggerManager.ClientLoggerFactory)
            var nodeClient = new DefaultNodeClient(new NodeClientParameters()
            {
                Host = host,
                Port = port,
                LocalHost = localHost,
                LocalPort = localPort,
                ProtocolStackFactory = new DefaultProtocolStackFactory(),
                Serializer = serializer,
                Communication = new DotNettyClient(host, port, localHost, localPort)
            });

            nodeClient.ConnectAsync().Wait();

            CallGetServiceName(nodeClient).Wait();
            CallAddCustomer(nodeClient).Wait();
            CallGetCustomer(nodeClient).Wait();
            CallQueryCustomer(nodeClient).Wait();
            CallRemoveCustomer(nodeClient).Wait();
            CallQueryCustomer(nodeClient).Wait();
            CallRemoveAllCustomer(nodeClient).Wait();
            CallQueryCustomer(nodeClient).Wait();

            System.Console.ReadLine();

            nodeClient.CloseAsync().ContinueWith(task =>
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

        private async static Task CallGetServiceName(INodeClient nodeClient)
        {
            System.Console.WriteLine("------------------------------GetServiceName----------------------------------");

            try
            {
                var result = await nodeClient.CallServiceAsync(10001, 1, null, typeof(string), 1000 * 3, null);
                System.Console.WriteLine($"Result={result.ReturnVal}");
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

        private async static Task CallAddCustomer(INodeClient nodeClient)
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
                var task1 = nodeClient.CallServiceAsync(10001, 2, new object[] { customer1 }, null, 1000 * 3, null);

                var customer2 = new Customer()
                {
                    Id = 2,
                    Name = "Jane",
                    Birthday = new DateTime(2001, 5, 10)
                };
                var task2 = nodeClient.CallServiceAsync(10001, 2, new object[] { customer2 }, null, 1000 * 3, null);

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

        private async static Task CallGetCustomer(INodeClient nodeClient)
        {
            System.Console.WriteLine("------------------------------GetCustomer----------------------------------");

            try
            {
                var result = await nodeClient.CallServiceAsync(10001, 3, new object[] { 1 }, typeof(Customer), 1000 * 3, null);
                var customer = (Customer)result.ReturnVal;
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

        private async static Task CallQueryCustomer(INodeClient nodeClient)
        {
            System.Console.WriteLine("------------------------------QueryCustomer----------------------------------");

            try
            {
                var result = await nodeClient.CallServiceAsync(10001, 4, null, typeof(List<Customer>), 1000 * 3, null);
                var customers = (List<Customer>)result.ReturnVal;
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

        private async static Task CallRemoveCustomer(INodeClient nodeClient)
        {
            System.Console.WriteLine("------------------------------RemoveCustomer----------------------------------");

            try
            {
                await nodeClient.CallServiceAsync(10001, 5, new object[] { 1 }, null, 1000 * 3, null);

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

        private async static Task CallRemoveAllCustomer(INodeClient nodeClient)
        {
            System.Console.WriteLine("------------------------------RemoveAllCustomer----------------------------------");

            try
            {
                await nodeClient.CallServiceAsync(10001, 6, null, null, 1000 * 3, null);

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
    }
}
