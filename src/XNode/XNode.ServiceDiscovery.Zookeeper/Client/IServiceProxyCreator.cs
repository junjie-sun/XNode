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
    /// 服务代理构造器接口
    /// </summary>
    public interface IServiceProxyCreator
    {
        /// <summary>
        /// 创建服务代理实例
        /// </summary>
        /// <param name="proxyName">代理名称</param>
        /// <param name="serviceProxyType">服务代理类型</param>
        /// <param name="nodeClientList">NodeClient实例</param>
        /// <returns></returns>
        IServiceProxy Create(string proxyName,
            Type serviceProxyType,
            IList<INodeClient> nodeClientList);
    }
}
