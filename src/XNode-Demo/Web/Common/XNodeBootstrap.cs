// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XNode;
using XNode.Client;
using XNode.Client.Configuration;
using XNode.Client.ServiceCallers;
using XNode.Communication;
using XNode.Communication.DotNetty;
using XNode.Security;
using XNode.Serializer.ProtoBuf;

namespace Web.Common
{
    public class XNodeBootstrap
    {
        public void Run(ILoggerFactory loggerFactory, IServiceProxyManager serviceProxyManager)
        {
            var logger = loggerFactory.CreateLogger<XNodeBootstrap>();

            var dir = Directory.GetCurrentDirectory();
            var fileName = "xnode.json";
            string path = Path.Combine(dir, fileName);

            var configRoot = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();

            var name = configRoot.GetValue<string>("name");
            var globalConfig = configRoot.GetGlobalConfig();
            GlobalSettings.Apply(globalConfig);

            #region Client

            var clientConfig = configRoot.GetClientConfig();

            if (clientConfig.ServiceProxies == null || clientConfig.ServiceProxies.Count == 0)
            {
                return;
            }

            XNode.Logging.LoggerManager.ClientLoggerFactory = loggerFactory;

            var serializer = new ProtoBufSerializer(XNode.Logging.LoggerManager.ClientLoggerFactory);

            var serviceCaller = new ServiceCallerBuilder()
                .UseDefault()
                .Build();

            foreach (var config in clientConfig.ServiceProxies)
            {
                var serviceProxy = new ServiceProxy(
                    config.ProxyName,
                    config?.Services,
                    serviceCaller)
                    .AddServices(config.ProxyTypes)
                    .AddClients(
                        new NodeClientBuilder()
                            .ConfigConnections(config.Connections)
                            .ConfigSerializer(serializer)
                            .ConfigLoginHandler(new DefaultLoginHandler(configRoot.GetDefaultLoginHandlerConfig(config.ProxyName), serializer))
                            .UseDotNetty()
                            .Build()
                    );
                serviceProxyManager.Regist(serviceProxy);
            }

            serviceProxyManager.ConnectAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    if (task.Exception is AggregateException)
                    {
                        foreach (var e in task.Exception.InnerExceptions)
                        {
                            if (e is NetworkException netEx)
                            {
                                logger.LogError(netEx, $"XNode client connect has net error. Host={netEx.Host}, Port={netEx.Port}, Message={netEx.Message}");
                            }
                        }
                    }
                    else
                    {
                        logger.LogError(task.Exception, $"XNode client connect has error. Message={task.Exception.Message}");
                    }
                    throw task.Exception;
                }
            });

            #endregion
        }
    }
}
