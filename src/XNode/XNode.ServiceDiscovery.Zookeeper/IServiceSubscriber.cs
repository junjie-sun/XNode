// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using XNode.Client;

namespace XNode.ServiceDiscovery.Zookeeper
{
    /// <summary>
    /// 服务订阅器接口
    /// </summary>
    public interface IServiceSubscriber : IDisposable
    {
        /// <summary>
        /// 服务订阅
        /// </summary>
        /// <param name="serviceProxyFactory">ServiceProxy工厂</param>
        /// <param name="nodeClientFactory">NodeClient工厂</param>
        /// <returns></returns>
        ServiceSubscriber Subscribe<ServiceProxyType>(
            Func<ServiceProxyArgs, IServiceProxy> serviceProxyFactory,
            Func<NodeClientArgs, IList<INodeClient>> nodeClientFactory);

        /// <summary>
        /// 获取所有订阅服务的代理对象
        /// </summary>
        /// <returns></returns>
        IList<IServiceProxy> GetServiceProxies();
    }
}
