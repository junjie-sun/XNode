// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
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
    /// ServiceProxyManager扩展方法类
    /// </summary>
    public static class SrviceProxyManagerExtensions
    {
        /// <summary>
        /// 注册所有订阅服务的代理对象
        /// </summary>
        /// <param name="serviceProxyManager">服务代理管理器</param>
        /// <param name="serviceSubscriber">服务订阅器</param>
        public static void Regist(this IServiceProxyManager serviceProxyManager, IServiceSubscriber serviceSubscriber)
        {
            var serviceProxies = serviceSubscriber.GetServiceProxies();
            foreach (var serviceProxy in serviceProxies)
            {
                serviceProxyManager.Regist(serviceProxy);
            }
        }
    }
}
