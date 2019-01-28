// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using XNode.Client;
using XNode.Client.Configuration;

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
        /// <param name="connectionInfos">连接信息</param>
        /// <param name="serializerName">序列化器名称</param>
        /// <param name="useNewClient">是否强制创建新的NodeClient实例</param>
        /// <param name="isConnect">NodeClient实例创建后是否进行连接</param>
        /// <returns></returns>
        public IList<INodeClient> CreateNodeClientList(IList<ConnectionInfo> connectionInfos, string serializerName, bool useNewClient, bool isConnect = false)
        {
            SetConnectionInfos(connectionInfos);

            if (useNewClient)
            {
                return CreactNewNodeClientList(connectionInfos, serializerName, isConnect);
            }
            else
            {
                return CreateSharedNodeClientList(connectionInfos, serializerName, isConnect);
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

        private IList<INodeClient> CreactNewNodeClientList(IList<ConnectionInfo> connectionInfos, string serializerName, bool isConnect)
        {
            var nodeClientList = nodeClientFactory(new NodeClientArgs()
            {
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

        private IList<INodeClient> CreateSharedNodeClientList(IList<ConnectionInfo> connectionInfos, string serializerName, bool isConnect)
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

            var newNodeClientList = CreactNewNodeClientList(newConnectionInfos, serializerName, isConnect);

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
