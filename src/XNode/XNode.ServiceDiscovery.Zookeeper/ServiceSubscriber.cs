// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using Rabbit.Zookeeper;
using Rabbit.Zookeeper.Implementation;
using System;
using System.Collections.Generic;
using System.Reflection;
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

        private IList<ServiceInfo> serviceConfigs;

        private string localHost;

        private int? localPort;

        private IDictionary<string, ServiceSubscriberInfo> serviceSubscriberList = new Dictionary<string, ServiceSubscriberInfo>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">Zookeeper连接字符串</param>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="serviceConfigs">服务配置</param>
        /// <param name="localHost">本地Host</param>
        /// <param name="localPort">本地端口</param>
        /// <param name="basePath">Zookeeper根路径</param>
        public ServiceSubscriber(string connectionString,
            ILoggerFactory loggerFactory,
            IList<ServiceInfo> serviceConfigs = null,
            string localHost = null,
            int? localPort = null,
            string basePath = "/XNode")
        {
            ConnectionString = connectionString;
            BasePath = basePath;
            logger = loggerFactory.CreateLogger<ServiceSubscriber>();
            this.serviceConfigs = serviceConfigs;
            this.localHost = localHost;
            this.localPort = localPort;
            try
            {
                client = new ZookeeperClient(new ZookeeperClientOptions(connectionString));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Connect zookeeper failed. ConnectionString={connectionString}");
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
        /// <param name="serviceProxyFactory">ServiceProxy工厂</param>
        /// <param name="nodeClientFactory">NodeClient工厂</param>
        /// <returns></returns>
        public ServiceSubscriber Subscribe<ServiceProxyType>(
            Func<ServiceProxyArgs, IServiceProxy> serviceProxyFactory,
            Func<NodeClientArgs, IList<INodeClient>> nodeClientFactory)
        {
            var serviceProxyType = typeof(ServiceProxyType);

            var serviceProxyAttr = GetServiceProxyAttribute(serviceProxyType);

            ServiceInfo config = null;
            if (serviceConfigs != null && serviceConfigs.Count > 0)
            {
                config = serviceConfigs.Where(c => c.ServiceId == serviceProxyAttr.ServiceId).FirstOrDefault();
            }

            var serviceName = config != null ? config.Name : serviceProxyAttr.Name;

            var servicePublishInfos = GetServicePublishInfos(serviceName);

            var serviceProxy = serviceProxyFactory(new ServiceProxyArgs()
            {
                Name = serviceName,
                ServiceInfo = config,
                ServiceType = serviceProxyType
            }).AddService(serviceProxyType);

            var connectionInfos = GetConnectionInfos(servicePublishInfos, localHost, localPort);

            var nodeClientList = nodeClientFactory(new NodeClientArgs()
            {
                SerializerName = servicePublishInfos[0].SerializerName,
                ConnectionInfos = connectionInfos
            });

            serviceProxy.AddClients(nodeClientList);

            serviceSubscriberList.Add(serviceName, new ServiceSubscriberInfo()
            {
                ServiceProxy = serviceProxy,
                ConnectionInfos = connectionInfos,
                NodeClientFactory = nodeClientFactory
            });

            HostsChangedHandler(serviceName);

            return this;
        }

        /// <summary>
        /// 关闭订阅
        /// </summary>
        public void Dispose()
        {
            client.Dispose();
            logger.LogInformation("ServiceSubscriber dispose.");
        }

        /// <summary>
        /// 获取所有订阅服务的代理对象
        /// </summary>
        /// <returns></returns>
        public IList<IServiceProxy> GetServiceProxies()
        {
            return null;
        }

        private ServiceProxyAttribute GetServiceProxyAttribute(Type serviceProxyType)
        {
            var typeInfo = serviceProxyType.GetTypeInfo();
            var serviceProxyAttr = typeInfo.GetCustomAttribute<ServiceProxyAttribute>();

            if (serviceProxyAttr == null)
            {
                logger.LogInformation($"ServiceProxyType has not set ServiceProxyAttribute. Type={serviceProxyType.FullName}");
                throw new InvalidOperationException($"ServiceProxyType has not set ServiceProxyAttribute. Type={serviceProxyType.FullName}");
            }

            return serviceProxyAttr;
        }

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

        private IList<ConnectionInfo> GetConnectionInfos(IList<ServicePublishInfo> servicePublishInfos, string localHost, int? localPort)
        {
            var list = new List<ConnectionInfo>();

            foreach (var publishInfo in servicePublishInfos)
            {
                list.Add(new ConnectionInfo()
                {
                    Host = publishInfo.Host,
                    Port = publishInfo.Port,
                    LocalHost = localHost,
                    LocalPort = localPort
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

                    lock (serviceSubscriberInfo)
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
                try
                {
                    var connectionInfo = serviceSubscriberInfo.ConnectionInfos.Where(c => Utils.GetHostName(c.Host, c.Port) == hostName).Single();
                    serviceSubscriberInfo.ServiceProxy.RemoveClient(connectionInfo.Host, connectionInfo.Port);
                    serviceSubscriberInfo.ConnectionInfos.Remove(connectionInfo);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Delete host failed. HostName={hostName}");
                }
            }
        }

        private void HandleInsertedHosts(IEnumerable<string> insertedHosts, ServiceSubscriberInfo serviceSubscriberInfo, string path)
        {
            var connectionInfos = new List<ConnectionInfo>();
            string serializerName = null;

            foreach (var hostName in insertedHosts)
            {
                try
                {
                    var servicePublishInfo = GetServicePublishInfo($"{path}/{hostName}");
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
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Insert host failed, get connection infomation error. HostName={hostName}");
                }
            }

            IList<INodeClient> nodeClientList;

            try
            {
                nodeClientList = serviceSubscriberInfo.NodeClientFactory(new NodeClientArgs()
                {
                    ConnectionInfos = connectionInfos,
                    SerializerName = serializerName
                });
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Insert host failed, create NodeClient error.");
                return;
            }

            foreach (var nodeClient in nodeClientList)
            {
                try
                {
                    nodeClient.ConnectAsync().Wait();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Insert host failed, NodeClient connect error. Host={nodeClient.Host}, Port={nodeClient.Port}");
                }
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
    }

    internal class ServiceSubscriberInfo
    {
        public IServiceProxy ServiceProxy { get; set; }

        public IList<ConnectionInfo> ConnectionInfos { get; set; }

        public Func<NodeClientArgs, IList<INodeClient>> NodeClientFactory { get; set; }
    }
}
