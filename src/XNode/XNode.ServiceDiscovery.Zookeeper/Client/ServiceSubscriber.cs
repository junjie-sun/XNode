// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using Rabbit.Zookeeper;
using Rabbit.Zookeeper.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using XNode.Client;
using XNode.Client.Configuration;
using static org.apache.zookeeper.Watcher.Event;
using System.Threading.Tasks;
using MessagePack;

namespace XNode.ServiceDiscovery.Zookeeper
{
    /// <summary>
    /// 服务订阅器
    /// </summary>
    public class ServiceSubscriber : IServiceSubscriber
    {
        private ILogger logger;

        private IZookeeperClient client;

        private IServiceProxyCreator serviceProxyCreator;

        private INodeClientManager nodeClientManager;

        private IDictionary<string, ServiceSubscriberInfo> serviceSubscriberList = new Dictionary<string, ServiceSubscriberInfo>();

        private object hostsChangedLockObj = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">Zookeeper连接字符串</param>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="serviceProxyCreator">服务代理构造器</param>
        /// <param name="nodeClientManager">NodeClient管理器</param>
        /// <param name="basePath">Zookeeper根路径</param>
        public ServiceSubscriber(string connectionString,
            ILoggerFactory loggerFactory,
            IServiceProxyCreator serviceProxyCreator,
            INodeClientManager nodeClientManager,
            string basePath = "/XNode")
        {
            ConnectionString = connectionString;
            BasePath = basePath;
            logger = loggerFactory.CreateLogger<ServiceSubscriber>();
            this.serviceProxyCreator = serviceProxyCreator;
            this.nodeClientManager = nodeClientManager;

            try
            {
                client = new ZookeeperClient(new ZookeeperClientOptions(connectionString));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Connect zookeeper failed. ConnectionString={connectionString}");
                throw ex;
            }

            logger.LogInformation($"Connect zookeeper success. ConnectionString={connectionString}");
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
        /// <param name="useNewClient">是否强制使用新的NodeClient，当为false时会多个服务代理共享一个NodeClient实例</param>
        /// <returns></returns>
        public ServiceSubscriber Subscribe<ServiceProxyType>(bool useNewClient = false)
        {
            var serviceProxyType = typeof(ServiceProxyType);

            var serviceProxyAttr = Utils.GetServiceProxyAttribute(serviceProxyType);

            var serviceName = serviceProxyAttr.Name;

            var servicePublishInfos = GetServicePublishInfos(serviceName);

            var serializerName = servicePublishInfos[0].SerializerName;

            var connectionInfos = GetConnectionInfos(servicePublishInfos);

            var nodeClientList = nodeClientManager.CreateNodeClientList(connectionInfos, serializerName, useNewClient);

            var serviceProxy = serviceProxyCreator.Create(serviceName, serviceProxyType, nodeClientList);

            serviceSubscriberList.Add(serviceName, new ServiceSubscriberInfo()
            {
                ServiceProxy = serviceProxy,
                ConnectionInfos = connectionInfos,
                UseNewClient = useNewClient
            });

            HostsChangedHandler(serviceName);

            return this;
        }

        /// <summary>
        /// 获取所有订阅服务的代理对象
        /// </summary>
        /// <returns></returns>
        public IList<IServiceProxy> GetServiceProxies()
        {
            return serviceSubscriberList.Values.Select(s => s.ServiceProxy).ToList();
        }

        /// <summary>
        /// 关闭订阅
        /// </summary>
        public void Dispose()
        {
            client.Dispose();
            logger.LogInformation("ServiceSubscriber dispose.");
        }

        #region 私有方法

        private IList<ServicePublishInfo> GetServicePublishInfos(string serviceName)
        {
            var path = GetServicePath(serviceName);

            if (!client.ExistsAsync(path).Result)
            {
                logger.LogInformation($"Service not found on zookeeper. ServiceName={serviceName}");
                throw new Exception($"Service not found on zookeeper. ServiceName={serviceName}");
            }

            var hosts = client.GetChildrenAsync(path).Result.ToList();

            if (hosts.Count == 0)
            {
                logger.LogInformation($"No host config on zookeeper. ServiceName={serviceName}");
                throw new Exception($"No host config on zookeeper. ServiceName={serviceName}");
            }

            var list = new List<ServicePublishInfo>();

            foreach (var hostName in hosts)
            {
                var info = GetServicePublishInfo($"{path}/{hostName}");
                list.Add(info);
            }

            return list;
        }

        private ServicePublishInfo GetServicePublishInfo(string path)
        {
            var data = client.GetDataAsync(path).Result.ToArray();
            return MessagePackSerializer.Deserialize<ServicePublishInfo>(data);
        }

        private IList<ConnectionInfo> GetConnectionInfos(IList<ServicePublishInfo> servicePublishInfos)
        {
            var list = new List<ConnectionInfo>();

            foreach (var publishInfo in servicePublishInfos)
            {
                list.Add(new ConnectionInfo()
                {
                    Host = publishInfo.Host,
                    Port = publishInfo.Port
                });
            }

            return list;
        }

        private string GetServicePath(string serviceName)
        {
            return $"{BasePath}/{serviceName}";
        }

        private void HostsChangedHandler(string serviceName)
        {
            var path = GetServicePath(serviceName);

            client.SubscribeChildrenChange(path, (ct, args) =>
            {
                if (args.Type == EventType.NodeChildrenChanged)
                {
                    logger.LogInformation($"Hosts changed on zookeeper. ServiceName={serviceName}");

                    var serviceSubscriberInfo = serviceSubscriberList[serviceName];

                    lock (hostsChangedLockObj)
                    {
                        var serviceProxy = serviceSubscriberInfo.ServiceProxy;
                        var currentHosts = serviceSubscriberInfo.ConnectionInfos.Select(c => Utils.GetHostName(c.Host, c.Port));

                        var deletedHosts = currentHosts.Except(args.CurrentChildrens);
                        var insertedHosts = args.CurrentChildrens.Except(currentHosts);

                        if (deletedHosts.Count() > 0)
                        {
                            HandleDeletedHosts(deletedHosts, serviceSubscriberInfo);
                        }

                        if (insertedHosts.Count() > 0)
                        {
                            HandleInsertedHosts(insertedHosts, serviceSubscriberInfo, path);
                        }
                    }
                }
                return Task.CompletedTask;
            });
        }

        private void HandleDeletedHosts(IEnumerable<string> deletedHosts, ServiceSubscriberInfo serviceSubscriberInfo)
        {
            foreach (var hostName in deletedHosts)
            {
                var connectionInfo = serviceSubscriberInfo.ConnectionInfos.Where(c => Utils.GetHostName(c.Host, c.Port) == hostName).Single();
                if (serviceSubscriberInfo.UseNewClient)
                {
                    serviceSubscriberInfo.ServiceProxy.RemoveClient(connectionInfo.Host, connectionInfo.Port);
                }
                else
                {
                    serviceSubscriberInfo.ServiceProxy.RemoveClient(connectionInfo.Host, connectionInfo.Port, false);
                    nodeClientManager.RemoveNodeClient(hostName);
                }
                serviceSubscriberInfo.ConnectionInfos.Remove(connectionInfo);
            }
        }

        private void HandleInsertedHosts(IEnumerable<string> insertedHosts, ServiceSubscriberInfo serviceSubscriberInfo, string path)
        {
            var connectionInfos = new List<ConnectionInfo>();
            string serializerName = null;

            foreach (var hostName in insertedHosts)
            {
                ServicePublishInfo servicePublishInfo;
                try
                {
                    servicePublishInfo = GetServicePublishInfo($"{path}/{hostName}");
                    
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Insert host failed, get connection infomation error. HostName={hostName}");
                    continue;
                }

                var connectionInfo = new ConnectionInfo()
                {
                    Host = servicePublishInfo.Host,
                    Port = servicePublishInfo.Port
                };

                serviceSubscriberInfo.ConnectionInfos.Add(connectionInfo);
                connectionInfos.Add(connectionInfo);

                if (string.IsNullOrWhiteSpace(serializerName))
                {
                    serializerName = servicePublishInfo.SerializerName;
                }
            }

            IList<INodeClient> nodeClientList;

            try
            {
                nodeClientList = nodeClientManager.CreateNodeClientList(connectionInfos, serializerName, serviceSubscriberInfo.UseNewClient, true);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Insert host failed, create NodeClient error.");
                return;
            }

            try
            {
                serviceSubscriberInfo.ServiceProxy.AddClients(nodeClientList);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Insert host failed, Append client to ServiceProxy error.");
            }
        }

        #endregion
    }

    internal class ServiceSubscriberInfo
    {
        public IServiceProxy ServiceProxy { get; set; }

        public IList<ConnectionInfo> ConnectionInfos { get; set; }

        public bool UseNewClient { get; set; }
    }
}
