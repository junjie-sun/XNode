// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using XNode.Client;
using XNode.Client.Configuration;

namespace XNode.ServiceDiscovery.Zookeeper
{
    /// <summary>
    /// NodeClient管理器接口
    /// </summary>
    public interface INodeClientManager
    {
        /// <summary>
        /// 创建NodeClient
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="connectionInfos">连接信息</param>
        /// <param name="serializerName">序列化器名称</param>
        /// <param name="useNewClient">是否强制创建新的NodeClient实例</param>
        /// <param name="isConnect">NodeClient实例创建后是否进行连接</param>
        /// <returns></returns>
        IList<INodeClient> CreateNodeClientList(string serviceName, IList<ConnectionInfo> connectionInfos, string serializerName, bool useNewClient, bool isConnect = false);

        /// <summary>
        /// 移除NodeClient
        /// </summary>
        /// <param name="hostName">Host名称</param>
        void RemoveNodeClient(string hostName);
    }
}
