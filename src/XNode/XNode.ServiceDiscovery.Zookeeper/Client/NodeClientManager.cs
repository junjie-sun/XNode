// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using XNode.Client;
using XNode.Client.Configuration;
using XNode.Communication.DotNetty;
using XNode.Security;
using XNode.Serializer;

namespace XNode.ServiceDiscovery.Zookeeper
{
    /// <summary>
    /// NodeClient管理器
    /// </summary>
    public class NodeClientManager : INodeClientManager
    {
        private ILogger logger;

        private string localHost;

        private int? localPort;

        private Func<NodeClientArgs, IList<INodeClient>> nodeClientFactory;

        private IDictionary<string, NodeClientInfo> sharedNodeClientList = new Dictionary<string, NodeClientInfo>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="nodeClientFactory">NodeClient工厂</param>
        /// <param name="localHost">本地Host</param>
        /// <param name="localPort">本地端口</param>
        public NodeClientManager(ILoggerFactory loggerFactory,
            Func<NodeClientArgs, IList<INodeClient>> nodeClientFactory,
            string localHost = null,
            int? localPort = null)
        {
            logger = loggerFactory.CreateLogger<NodeClientManager>();
            this.nodeClientFactory = nodeClientFactory;
            this.localHost = null;
            this.localPort = localPort;
        }

        /// <summary>
        /// 创建NodeClient
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="connectionInfos">连接信息</param>
        /// <param name="serializerName">序列化器名称</param>
        /// <param name="useNewClient">是否强制创建新的NodeClient实例</param>
        /// <param name="isConnect">NodeClient实例创建后是否进行连接</param>
        /// <returns></returns>
        public IList<INodeClient> CreateNodeClientList(string serviceName, IList<ConnectionInfo> connectionInfos, string serializerName, bool useNewClient, bool isConnect = false)
        {
            SetConnectionInfos(connectionInfos);

            if (useNewClient)
            {
                return CreateNewNodeClientList(serviceName, connectionInfos, serializerName, isConnect);
            }
            else
            {
                return CreateSharedNodeClientList(serviceName, connectionInfos, serializerName, isConnect);
            }
        }

        /// <summary>
        /// 移除NodeClient
        /// </summary>
        /// <param name="hostName">Host名称</param>
        public void RemoveNodeClient(string hostName)
        {
            var nodeClientInfo = sharedNodeClientList[hostName];
            nodeClientInfo.RefCount--;
            if (nodeClientInfo.RefCount == 0)
            {
                sharedNodeClientList.Remove(hostName);
                try
                {
                    nodeClientInfo.NodeClient.CloseAsync().Wait();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Close NodeClient failed. HostName={hostName}");
                }
            }
        }

        /// <summary>
        /// 创建默认NodeClientFactory
        /// </summary>
        /// <param name="zookeeperClientConfig"></param>
        /// <param name="serializerList"></param>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        public static Func<NodeClientArgs, IList<INodeClient>> CreateDefaultNodeClientFactory(ZookeeperClientConfig zookeeperClientConfig,
            IList<ISerializer> serializerList,
            ILoggerFactory loggerFactory)
        {
            IList<INodeClient> nodeClientFactory(NodeClientArgs args)
            {
                var serializer = serializerList.Where(s => s.Name == args.SerializerName).Single();

                var loginHandlerConfig = zookeeperClientConfig.Security.Where(s => s.ServiceName == args.ServiceName).SingleOrDefault();
                if (loginHandlerConfig == null)
                {
                    loginHandlerConfig = zookeeperClientConfig.Security.Where(s => string.Compare(s.ServiceName, "Default", true) == 0).SingleOrDefault();
                    if (loginHandlerConfig == null)
                    {
                        throw new Exception("Not found default security configuration.");
                    }
                }

                var passiveClosedStrategyConfig = zookeeperClientConfig.PassiveClosedStrategy.Where(p => p.ServiceName == args.ServiceName).SingleOrDefault();
                if (passiveClosedStrategyConfig == null)
                {
                    passiveClosedStrategyConfig = zookeeperClientConfig.PassiveClosedStrategy.Where(p => string.Compare(p.ServiceName, "Default", true) == 0).SingleOrDefault();
                    if (passiveClosedStrategyConfig == null)
                    {
                        throw new Exception("Not found default passive closed strategy configuration.");
                    }
                }

                return new NodeClientBuilder()
                    .ConfigConnections(args.ConnectionInfos)
                    .ConfigSerializer(serializer)
                    .ConfigLoginHandler(new DefaultLoginHandler(loginHandlerConfig.Config, serializer))
                    .ConfigPassiveClosedStrategy(new DefaultPassiveClosedStrategy(passiveClosedStrategyConfig.Config, loggerFactory))
                    .UseDotNetty()
                    .Build();
            }
            return nodeClientFactory;
        }

        #region 私有方法

        private void SetConnectionInfos(IList<ConnectionInfo> connectionInfos)
        {
            if (!string.IsNullOrWhiteSpace(localHost))
            {
                foreach (var connInfo in connectionInfos)
                {
                    connInfo.LocalHost = localHost;
                    connInfo.LocalPort = localPort;
                }
            }
        }

        private IList<INodeClient> CreateNewNodeClientList(string serviceName, IList<ConnectionInfo> connectionInfos, string serializerName, bool isConnect)
        {
            var nodeClientList = nodeClientFactory(new NodeClientArgs()
            {
                ServiceName = serviceName,
                SerializerName = serializerName,
                ConnectionInfos = connectionInfos
            });
            if (isConnect)
            {
                var list = new List<INodeClient>();
                foreach (var nodeClient in nodeClientList)
                {
                    try
                    {
                        nodeClient.ConnectAsync().Wait();
                        list.Add(nodeClient);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"NodeClient connect failed. Host={nodeClient.Host}, Port={nodeClient.Port}");
                        continue;
                    }
                }
                nodeClientList = list;
            }
            return nodeClientList;
        }

        private IList<INodeClient> CreateSharedNodeClientList(string serviceName, IList<ConnectionInfo> connectionInfos, string serializerName, bool isConnect)
        {
            var nodeClientList = new List<INodeClient>();
            var newConnectionInfos = new List<ConnectionInfo>();
            foreach (var connectionInfo in connectionInfos)
            {
                var hostName = Utils.GetHostName(connectionInfo.Host, connectionInfo.Port);
                if (!sharedNodeClientList.ContainsKey(hostName))
                {
                    newConnectionInfos.Add(connectionInfo);
                }
                else
                {
                    var nodeClientInfo = sharedNodeClientList[hostName];
                    nodeClientInfo.RefCount++;
                    nodeClientList.Add(nodeClientInfo.NodeClient);
                }
            }

            var newNodeClientList = CreateNewNodeClientList(serviceName, newConnectionInfos, serializerName, isConnect);

            foreach (var nodeClient in newNodeClientList)
            {
                var hostName = Utils.GetHostName(nodeClient.Host, nodeClient.Port);
                sharedNodeClientList.Add(hostName, new NodeClientInfo()
                {
                    NodeClient = nodeClient,
                    RefCount = 1
                });
                nodeClientList.Add(nodeClient);
            }

            return nodeClientList;
        }

        #endregion
    }

    internal class NodeClientInfo
    {
        public INodeClient NodeClient { get; set; }

        public int RefCount { get; set; }
    }
}
