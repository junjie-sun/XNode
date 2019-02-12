// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using XNode.Client.Configuration;

namespace XNode.ServiceDiscovery.Zookeeper
{
    /// <summary>
    /// 创建NodeClient相关参数
    /// </summary>
    public class NodeClientArgs
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 服务代理连接信息
        /// </summary>
        public IList<ConnectionInfo> ConnectionInfos { get; set; }

        /// <summary>
        /// 序列化器名称
        /// </summary>
        public string SerializerName { get; set; }
    }
}
