// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using XNode.Communication.DotNetty;
using XNode.Logging;
using XNode.Security;
using XNode.Serializer.ProtoBuf;
using XNode.Server;
using XNode.Server.Configuration;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            //配置服务端日志工厂
            LoggerManager.ServerLoggerFactory.AddConsole(LogLevel.Error);

            //加载配置文件
            string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
            var configRoot = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();

            var serverConfig = configRoot.GetServerConfig();

            var loginValidator = new DefaultLoginValidator(configRoot.GetDefaultLoginValidatorConfig(), LoggerManager.ServerLoggerFactory);     //创建LoginValidator实例

            //配置服务
            var nodeServer = new NodeServerBuilder()
                .ApplyConfig(serverConfig)
                .ConfigSerializer(new ProtoBufSerializer(LoggerManager.ServerLoggerFactory))
                .ConfigLoginValidator(loginValidator)       //配置LoginValidator
                .ConfigServiceProvider(GetServiceProvider())
                .UseDotNetty(serverConfig.ServerInfo)
                .Build();

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
                .RegistService(typeof(ISampleService), typeof(SampleService));
        }
    }
}
