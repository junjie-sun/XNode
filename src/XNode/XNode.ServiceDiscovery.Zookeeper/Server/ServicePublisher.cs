// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MessagePack;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using XNode.Server;
using static org.apache.zookeeper.KeeperException;

namespace XNode.ServiceDiscovery.Zookeeper
{
    /// <summary>
    /// 服务发布器
    /// </summary>
    public class ServicePublisher : IServicePublisher
    {
        private ILogger logger;

        private IZookeeperClient client;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">Zookeeper连接字符串</param>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="basePath">Zookeeper根路径</param>
        public ServicePublisher(string connectionString,
            ILoggerFactory loggerFactory,
            string basePath = "/XNode")
        {
            ConnectionString = connectionString;
            BasePath = basePath;
            logger = loggerFactory.CreateLogger<ServicePublisher>();

            ZooKeeper.LogLevel = System.Diagnostics.TraceLevel.Error;

            try
            {
                client = new ZookeeperClient(new ZookeeperClientOptions(connectionString));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Connect zookeeper failed. ConnectionString={connectionString}");
                throw ex;
            }

            logger.LogDebug($"Connect zookeeper success. ConnectionString={connectionString}");
        }

        /// <summary>
        /// Zookeeper连接字符串
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Zookeeper根路径
        /// </summary>
        public string BasePath { get; }

        /// <summary>
        /// 服务订阅
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="host">服务Host</param>
        /// <param name="port">服务端口</param>
        /// <param name="serializerName">序列化器名称</param>
        /// <returns></returns>
        public IServicePublisher Publish(Type serviceType, string host, int port, string serializerName)
        {
            logger.LogDebug($"Publish service begin. ServiceType={serviceType.FullName}, Host={host}, Port={port}, SerializerName={serializerName}");

            var serviceAttr = serviceType.GetServiceAttribute();

            if (serviceAttr == null)
            {
                throw new InvalidOperationException($"ServiceType has not set ServiceAttribute. Type={serviceType.FullName}");
            }

            var serviceName = serviceAttr.Name;

            CreateBaseNode();

            var servicePath = CreateServiceNode(serviceName);

            var servicePublishInfo = new ServicePublishInfo()
            {
                Host = host,
                Port = port,
                SerializerName = serializerName
            };

            CreateHostNode(servicePath, servicePublishInfo);

            logger.LogDebug($"Publish service finished. ServiceType={serviceType.FullName}, Host={host}, Port={port}, SerializerName={serializerName}");

            return this;
        }

        /// <summary>
        /// 关闭发布
        /// </summary>
        public void Dispose()
        {
            client.Dispose();
            logger.LogDebug("ServicePublisher disposed.");
        }

        #region 私有方法

        private string GetServicePath(string serviceName)
        {
            return $"{BasePath}/{serviceName}";
        }

        private string GetHostPath(string servicePath, string hostName)
        {
            return $"{servicePath}/{hostName}";
        }

        private void CreateBaseNode()
        {
            if (!client.ExistsAsync($"{BasePath}").Result)
            {
                try
                {
                    client.CreatePersistentAsync($"{BasePath}", null).Wait();
                }
                catch (AggregateException ex)
                {
                    if (!(ex.InnerException is NodeExistsException))
                    {
                        throw ex;
                    }
                }
            }
        }

        private string CreateServiceNode(string serviceName)
        {
            var servicePath = GetServicePath(serviceName);

            if (!client.ExistsAsync(servicePath).Result)
            {
                try
                {
                    client.CreatePersistentAsync(servicePath, null).Wait();
                }
                catch (AggregateException ex)
                {
                    if (!(ex.InnerException is NodeExistsException))
                    {
                        throw ex;
                    }
                }
            }

            return servicePath;
        }

        private void CreateHostNode(string servicePath, ServicePublishInfo servicePublishInfo)
        {
            var hostName = Utils.GetHostName(servicePublishInfo.Host, servicePublishInfo.Port);
            var hostPath = GetHostPath(servicePath, hostName);
            var data = MessagePackSerializer.Serialize(servicePublishInfo);

            if (!client.ExistsAsync(hostPath).Result)
            {
                try
                {
                    client.CreateEphemeralAsync(hostPath, data).Wait();
                }
                catch (AggregateException ex)
                {
                    if (!(ex.InnerException is NodeExistsException))
                    {
                        throw ex;
                    }
                }
            }
        }

        #endregion
    }
}
