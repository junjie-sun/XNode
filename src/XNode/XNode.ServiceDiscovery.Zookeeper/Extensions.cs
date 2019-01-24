// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

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
        /// <param name="config"></param>
        /// <returns></returns>
        public static ZookeeperConfig GetZookeeperConfig(this IConfiguration config)
        {
            var zookepperConfig = new ZookeeperConfig();
            config
                .GetSection("xnode:client:zookeeper")
                .Bind(zookepperConfig);
            return zookepperConfig;
        }
    }
}
