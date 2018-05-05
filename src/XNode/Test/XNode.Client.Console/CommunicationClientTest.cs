// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using XNode.Communication;
using XNode.Communication.DotNetty;
using XNode.Logging;

namespace XNode.Client.Console
{
    public static class CommunicationClientTest
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
            });

            System.Console.WriteLine("Input messge and send to server. Please input $quit to exit.");
            string message;
            do
            {
                message = System.Console.ReadLine();
                if (message.Equals("$quit", StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }
                for (long i = 0; i < 100; i++)
                {
                    var attachments = new Dictionary<string, byte[]>();
                    attachments.Add("TestString", Encoding.UTF8.GetBytes("ABC"));
                    attachments.Add("TestInt", BitConverter.GetBytes(100));

                    client.SendAsync(Encoding.UTF8.GetBytes(message), 1000 * 3, attachments)
                    .ContinueWith(task =>
                    {
                        try
                        {
                            var result = task.Result;
                            var str = Encoding.UTF8.GetString(result.Data);
                            System.Console.WriteLine($"Result={str}");
                            System.Console.WriteLine($"Res1={Encoding.UTF8.GetString(result.Attachments["Res1"])}");
                            System.Console.WriteLine($"Res2={BitConverter.ToInt32(result.Attachments["Res2"], 0)}");
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
                System.Console.WriteLine("Done");
            } while (true);

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
    }
}
