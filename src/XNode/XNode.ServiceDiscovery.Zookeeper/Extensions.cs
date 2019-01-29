// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using XNode.Client;

namespace XNode.ServiceDiscovery.Zookeeper
{
    /// <summary>
    /// Zookeeper配置扩展方法类
    /// </summary>
    public static class ZookeeperConfigurationExtensions
    {
        /// <summary>
        /// 获取Zookeeper配置
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <returns></returns>
        public static ZookeeperConfig GetZookeeperConfig(this IConfiguration config)
        {
            var zookepperConfig = new ZookeeperConfig();
            config
                .GetSection("xnode:zookeeper")
                .Bind(zookepperConfig);
            return zookepperConfig;
        }
    }

    /// <summary>
    /// ServiceSubscriber扩展方法类
    /// </summary>
    public static class ServiceSubscriberExtensions
    {
        /// <summary>
        /// 服务订阅
        /// </summary>
        /// <param name="serviceSubscriber"></param>
        /// <param name="useNewClient">是否强制使用新的NodeClient，当为false时会多个服务代理共享一个NodeClient实例</param>
        /// <returns></returns>
        public static IServiceSubscriber Subscribe<ServiceProxyType>(this IServiceSubscriber serviceSubscriber, bool useNewClient = false)
        {
            var serviceProxyType = typeof(ServiceProxyType);

            return serviceSubscriber.Subscribe(serviceProxyType, useNewClient);
        }

        /// <summary>
        /// 服务订阅
        /// </summary>
        /// <param name="serviceSubscriber"></param>
        /// <param name="serviceProxyTypes">服务代理类型列表</param>
        /// <param name="useNewClient">是否强制使用新的NodeClient，当为false时会多个服务代理共享一个NodeClient实例</param>
        /// <returns></returns>
        public static IServiceSubscriber Subscribe(this IServiceSubscriber serviceSubscriber, IEnumerable<Type> serviceProxyTypes, bool useNewClient = false)
        {
            foreach (var serviceProxyType in serviceProxyTypes)
            {
                serviceSubscriber.Subscribe(serviceProxyType, useNewClient);
            }

            return serviceSubscriber;
        }

        /// <summary>
        /// 向ServiceProxyManager注册所有订阅服务的代理对象
        /// </summary>
        /// <param name="serviceSubscriber"></param>
        /// <param name="serviceProxyManager">服务代理管理器</param>
        public static IServiceSubscriber RegistTo(this IServiceSubscriber serviceSubscriber, IServiceProxyManager serviceProxyManager)
        {
            var serviceProxies = serviceSubscriber.GetServiceProxies();
            foreach (var serviceProxy in serviceProxies)
            {
                serviceProxyManager.Regist(serviceProxy);
            }

            return serviceSubscriber;
        }
    }
}
