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
        /// <param name="useNewClient">是否强制使用新的NodeClient，当为false时会多个服务代理共享一个NodeClient实例</param>
        /// <returns></returns>
        ServiceSubscriber Subscribe<ServiceProxyType>(bool useNewClient = false);

        /// <summary>
        /// 获取所有订阅服务的代理对象
        /// </summary>
        /// <returns></returns>
        IList<IServiceProxy> GetServiceProxies();
    }
}
