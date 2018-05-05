// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service;
using System;
using System.IO;
using System.Text;
using XNode.Communication.DotNetty;
using XNode.Logging;
using XNode.Serializer.ProtoBuf;
using XNode.Server;
using XNode.Server.Configuration;
using XNode.Zipkin;
using Zipkin;

namespace Server2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please input enter to start order server.");
            Console.ReadLine();

            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            //配置服务端日志工厂，为了能看到服务调用细节，此处将日志级别设置为Information
            LoggerManager.ServerLoggerFactory.AddConsole(LogLevel.Information);

            //加载配置文件
            string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
            var configRoot = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();

            var serverConfig = configRoot.GetServerConfig();

            //配置服务
            var nodeServer = new NodeServerBuilder()
                .ApplyConfig(serverConfig)
                .ConfigSerializer(new ProtoBufSerializer(LoggerManager.ServerLoggerFactory))
                .AddServiceProcessor(new ZipkinProcessor())     //添加ZipkinProcessor
                .ConfigServiceProvider(GetServiceProvider())
                .UseDotNetty(serverConfig.ServerInfo)
                .Build();

            //Zipkin配置
            new ZipkinBootstrapper("OrderServer")
                .ZipkinAt("192.168.108.131")
                .WithSampleRate(1.0)
                .Start();

            //启动服务
            nodeServer.StartAsync().Wait();

            Console.ReadLine();

            //关闭服务
            nodeServer.StopAsync();
        }

        private static XNode.Server.IServiceProvider GetServiceProvider()
        {
            //注册服务
            return new DefaultServiceProvider()
                .RegistService(typeof(IOrderService), typeof(OrderService));
        }
    }
}
