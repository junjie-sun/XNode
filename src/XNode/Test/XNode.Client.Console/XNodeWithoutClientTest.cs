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
using XNode.Serializer;
using XNode.Serializer.MsgPack;
using XNode.Serializer.ProtoBuf;
using XNode.ProtocolStack;
using XNode.Communication.DotNetty;

namespace XNode.Client.Console
{
    public static class XNodeWithoutClientTest
    {
        public static void Test(string host, int port, string localHost, int localPort)
        {
            System.Console.ReadLine();

            LoggerManager.ClientLoggerFactory.AddConsole(LogLevel.Debug);
            var client = new DotNettyClient(host, port, localHost, localPort);
            client.ConnectAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    System.Console.WriteLine(task.Exception.InnerException.Message);
                    return;
                }
                System.Console.WriteLine("Connected");
            }).Wait();

            var serializer = new ProtoBufSerializer(LoggerManager.ClientLoggerFactory);     //new MsgPackSerializer(LoggerManager.ClientLoggerFactory);

            CallGetServiceName(client, serializer).Wait();

            CallAddCustomer(client, serializer).Wait();

            CallGetCustomer(client, serializer).Wait();

            CallQueryCustomer(client, serializer).Wait();

            CallRemoveCustomer(client, serializer).Wait();

            CallQueryCustomer(client, serializer).Wait();

            CallRemoveAllCustomer(client, serializer).Wait();

            CallQueryCustomer(client, serializer).Wait();

            System.Console.ReadLine();

            client.CloseAsync().ContinueWith(task =>
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

        private static Task CallGetServiceName(DotNettyClient client, ISerializer serializer)
        {
            System.Console.WriteLine("------------------------------GetServiceName----------------------------------");

            var request = CreateGetServiceNameRequest();
            return client.SendAsync(serializer.SerializeAsync(request).Result, 1000 * 3)
                .ContinueWith(task =>
                {
                    try
                    {
                        GetServiceNameResponseHandler(serializer, task.Result.Data);
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerException.GetType() == typeof(RequestTimeoutExcption))
                        {
                            System.Console.WriteLine($"Timeout: RequestId={((RequestTimeoutExcption)ex.InnerException).Request.Id}");
                        }
                        else
                        {
                            System.Console.WriteLine($"Error: {ex.InnerException.Message}");
                        }
                    }
                });
        }

        private static ServiceRequest CreateGetServiceNameRequest()
        {
            var request = new ServiceRequest();
            request.ServiceId = 10001;
            request.ActionId = 1;
            request.ParamList = new List<byte[]>();

            return request;
        }

        private static void GetServiceNameResponseHandler(ISerializer serializer, byte[] data)
        {
            var response = (ServiceResponse)serializer.DeserializeAsync(typeof(ServiceResponse), data).Result;
            if (response.HasException)
            {
                System.Console.WriteLine($"Server has an error, Exception={response.ExceptionId}, ExceptionMessage={response.ExceptionMessage}");
            }
            else
            {
                var result = (string)serializer.DeserializeAsync(typeof(string), response.ReturnValue).Result;
                System.Console.WriteLine($"Result={result}");
            }
        }

        #endregion

        #region AddCustomer

        private static Task CallAddCustomer(DotNettyClient client, ISerializer serializer)
        {
            System.Console.WriteLine("------------------------------AddCustomerTest----------------------------------");

            var request1 = CreateAddCustomerRequest(serializer, new Customer()
            {
                Id = 1,
                Name = "Michael",
                Birthday = new DateTime(1999, 1, 1)
            });
            var task1 = client.SendAsync(serializer.SerializeAsync(request1).Result, 1000 * 3)
                .ContinueWith(task =>
                {
                    try
                    {
                        AddCustomerResponseHandler(serializer, task.Result.Data);
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerException.GetType() == typeof(RequestTimeoutExcption))
                        {
                            System.Console.WriteLine($"Timeout: RequestId={((RequestTimeoutExcption)ex.InnerException).Request.Id}");
                        }
                        else
                        {
                            System.Console.WriteLine($"Error: {ex.InnerException.Message}");
                        }
                    }
                });

            var request2 = CreateAddCustomerRequest(serializer, new Customer()
            {
                Id = 2,
                Name = "Jane",
                Birthday = new DateTime(2001, 5, 10)
            });
            var task2 = client.SendAsync(serializer.SerializeAsync(request2).Result, 1000 * 3)
                .ContinueWith(task =>
                {
                    try
                    {
                        AddCustomerResponseHandler(serializer, task.Result.Data);
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerException.GetType() == typeof(RequestTimeoutExcption))
                        {
                            System.Console.WriteLine($"Timeout: RequestId={((RequestTimeoutExcption)ex.InnerException).Request.Id}");
                        }
                        else
                        {
                            System.Console.WriteLine($"Error: {ex.InnerException.Message}");
                        }
                    }
                });

            return Task.WhenAll(task1, task2);
        }

        private static ServiceRequest CreateAddCustomerRequest(ISerializer serializer, Customer customer)
        {
            var request = new ServiceRequest();
            request.ServiceId = 10001;
            request.ActionId = 2;
            request.ParamList = new List<byte[]>();
            request.ParamList.Add(serializer.SerializeAsync(customer).Result);

            return request;
        }

        private static void AddCustomerResponseHandler(ISerializer serializer, byte[] data)
        {
            var response = (ServiceResponse)serializer.DeserializeAsync(typeof(ServiceResponse), data).Result;
            if (response.HasException)
            {
                System.Console.WriteLine($"Server has an error, Exception={response.ExceptionId}, ExceptionMessage={response.ExceptionMessage}");
            }
            else
            {
                System.Console.WriteLine($"Add customer success");
            }
        }

        #endregion

        #region GetCustomer

        private static Task CallGetCustomer(DotNettyClient client, ISerializer serializer)
        {
            System.Console.WriteLine("------------------------------GetCustomerTest----------------------------------");

            var request = CreateGetCustomerRequest(serializer);
            return client.SendAsync(serializer.SerializeAsync(request).Result, 1000 * 3)
                .ContinueWith(task =>
                {
                    try
                    {
                        GetCustomerResponseHandler(serializer, task.Result.Data);
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerException.GetType() == typeof(RequestTimeoutExcption))
                        {
                            System.Console.WriteLine($"Timeout: RequestId={((RequestTimeoutExcption)ex.InnerException).Request.Id}");
                        }
                        else
                        {
                            System.Console.WriteLine($"Error: {ex.InnerException.Message}");
                        }
                    }
                });
        }

        private static ServiceRequest CreateGetCustomerRequest(ISerializer serializer)
        {
            var request = new ServiceRequest();
            request.ServiceId = 10001;
            request.ActionId = 3;
            request.ParamList = new List<byte[]>();
            request.ParamList.Add(serializer.SerializeAsync(1).Result);

            return request;
        }

        private static void GetCustomerResponseHandler(ISerializer serializer, byte[] data)
        {
            var response = (ServiceResponse)serializer.DeserializeAsync(typeof(ServiceResponse), data).Result;
            if (response.HasException)
            {
                System.Console.WriteLine($"Server has an error, Exception={response.ExceptionId}, ExceptionMessage={response.ExceptionMessage}");
            }
            else
            {
                var result = (Customer)serializer.DeserializeAsync(typeof(Customer), response.ReturnValue).Result;
                System.Console.WriteLine($"Result=Id: {result.Id}, Name: {result.Name}, Birthday: {result.Birthday.ToString("yyyy-MM-dd")}");
            }
        }

        #endregion

        #region QueryCustomer

        private static Task CallQueryCustomer(DotNettyClient client, ISerializer serializer)
        {
            System.Console.WriteLine("------------------------------QueryCustomerTest----------------------------------");

            var request = CreateQueryCustomerRequest(serializer);
            return client.SendAsync(serializer.SerializeAsync(request).Result, 1000 * 3)
                .ContinueWith(task =>
                {
                    try
                    {
                        QueryCustomerResponseHandler(serializer, task.Result.Data);
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerException.GetType() == typeof(RequestTimeoutExcption))
                        {
                            System.Console.WriteLine($"Timeout: RequestId={((RequestTimeoutExcption)ex.InnerException).Request.Id}");
                        }
                        else
                        {
                            System.Console.WriteLine($"Error: {ex.InnerException.Message}");
                        }
                    }
                });
        }

        private static ServiceRequest CreateQueryCustomerRequest(ISerializer serializer)
        {
            var request = new ServiceRequest();
            request.ServiceId = 10001;
            request.ActionId = 4;
            request.ParamList = new List<byte[]>();

            return request;
        }

        private static void QueryCustomerResponseHandler(ISerializer serializer, byte[] data)
        {
            var response = (ServiceResponse)serializer.DeserializeAsync(typeof(ServiceResponse), data).Result;
            if (response.HasException)
            {
                System.Console.WriteLine($"Server has an error, Exception={response.ExceptionId}, ExceptionMessage={response.ExceptionMessage}");
            }
            else
            {
                var result = (List<Customer>)serializer.DeserializeAsync(typeof(List<Customer>), response.ReturnValue).Result;
                foreach (var customer in result)
                {
                    System.Console.WriteLine($"Result=Id: {customer.Id}, Name: {customer.Name}, Birthday: {customer.Birthday.ToString("yyyy-MM-dd")}");
                }
            }
        }

        #endregion

        #region RemoveCustomer

        private static Task CallRemoveCustomer(DotNettyClient client, ISerializer serializer)
        {
            System.Console.WriteLine("------------------------------RemoveCustomerTest----------------------------------");

            var request = CreateRemoveCustomerRequest(serializer);
            return client.SendAsync(serializer.SerializeAsync(request).Result, 1000 * 3)
                .ContinueWith(task =>
                {
                    try
                    {
                        RemoveCustomerResponseHandler(serializer, task.Result.Data);
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerException.GetType() == typeof(RequestTimeoutExcption))
                        {
                            System.Console.WriteLine($"Timeout: RequestId={((RequestTimeoutExcption)ex.InnerException).Request.Id}");
                        }
                        else
                        {
                            System.Console.WriteLine($"Error: {ex.InnerException.Message}");
                        }
                    }
                });
        }

        private static ServiceRequest CreateRemoveCustomerRequest(ISerializer serializer)
        {
            var request = new ServiceRequest();
            request.ServiceId = 10001;
            request.ActionId = 5;
            request.ParamList = new List<byte[]>();
            request.ParamList.Add(serializer.SerializeAsync(1).Result);

            return request;
        }

        private static void RemoveCustomerResponseHandler(ISerializer serializer, byte[] data)
        {
            var response = (ServiceResponse)serializer.DeserializeAsync(typeof(ServiceResponse), data).Result;
            if (response.HasException)
            {
                System.Console.WriteLine($"Server has an error, Exception={response.ExceptionId}, ExceptionMessage={response.ExceptionMessage}");
            }
            else
            {
                System.Console.WriteLine($"Remove customer success");
            }
        }

        #endregion

        #region RemoveAllCustomer

        private static Task CallRemoveAllCustomer(DotNettyClient client, ISerializer serializer)
        {
            System.Console.WriteLine("------------------------------RemoveAllCustomerTest----------------------------------");

            var request = CreateRemoveAllCustomerRequest(serializer);
            return client.SendAsync(serializer.SerializeAsync(request).Result, 1000 * 3)
                .ContinueWith(task =>
                {
                    try
                    {
                        RemoveAllCustomerResponseHandler(serializer, task.Result.Data);
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerException.GetType() == typeof(RequestTimeoutExcption))
                        {
                            System.Console.WriteLine($"Timeout: RequestId={((RequestTimeoutExcption)ex.InnerException).Request.Id}");
                        }
                        else
                        {
                            System.Console.WriteLine($"Error: {ex.InnerException.Message}");
                        }
                    }
                });
        }

        private static ServiceRequest CreateRemoveAllCustomerRequest(ISerializer serializer)
        {
            var request = new ServiceRequest();
            request.ServiceId = 10001;
            request.ActionId = 6;
            request.ParamList = new List<byte[]>();

            return request;
        }

        private static void RemoveAllCustomerResponseHandler(ISerializer serializer, byte[] data)
        {
            var response = (ServiceResponse)serializer.DeserializeAsync(typeof(ServiceResponse), data).Result;
            if (response.HasException)
            {
                System.Console.WriteLine($"Server has an error, Exception={response.ExceptionId}, ExceptionMessage={response.ExceptionMessage}");
            }
            else
            {
                System.Console.WriteLine($"Remove all customer success");
            }
        }

        #endregion
    }
}
