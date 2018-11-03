// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Communication;
using XNode.Communication.DotNetty;
using XNode.Logging;

namespace XNode.Server.Console
{
    public static class CommunicationServerTest
    {
        public static void Test(string host, int port)
        {
            
            var server = new DotNettyServer(host, port);
            server.OnRecieveLoginRequest += Server_OnRecieveLoginRequest;
            server.OnRecieveServiceRequest += Server_OnRecieveRequest;
            server.StartAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    System.Console.WriteLine(task.Exception.InnerException.Message);
                    return;
                }
                System.Console.WriteLine($"Server listen port {port}");
            });

            System.Console.ReadLine();

            server.CloseAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    System.Console.WriteLine(task.Exception.InnerException.Message);
                    return;
                }
                System.Console.WriteLine("Server closed");
            });
        }

        private static Task<LoginResponseData> Server_OnRecieveLoginRequest(LoginAuthInfo loginAuthInfo)
        {
            return Task.FromResult(new LoginResponseData()
            {
                AuthIdentity = "guest",
                AuthStatusCode = 0,
                AuthResult = true
            });
        }

        private static Task<ResponseData> Server_OnRecieveRequest(byte[] message, IDictionary<string, byte[]> attachments, LoginState loginState)
        {
            var msgStr = Encoding.UTF8.GetString(message);
            System.Console.WriteLine($"Recieve message: {msgStr}");
            var attachmentStr = Encoding.UTF8.GetString(attachments["TestString"]);
            var attachmentInt = BitConverter.ToInt32(attachments["TestInt"], 0);
            System.Console.WriteLine($"Recieve attachment: TestString={attachmentStr}, TestInt={attachmentInt}");
            return Task.Run<ResponseData>(() =>
            {
                var result = "Completed";
                var resAttachments = new Dictionary<string, byte[]>();
                resAttachments.Add("Res1", Encoding.UTF8.GetBytes("ZZZ"));
                resAttachments.Add("Res2", BitConverter.GetBytes(999));
                return new ResponseData()
                {
                    Data = Encoding.UTF8.GetBytes(result),
                    Attachments = resAttachments
                };
            });
        }
    }
}
