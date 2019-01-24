// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using XNode.Client.Configuration;

namespace XNode.ServiceDiscovery.Zookeeper
{
    /// <summary>
    /// 创建ServiceProxy相关参数
    /// </summary>
    public class ServiceProxyArgs
    {
        /// <summary>
        /// 代理名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 服务代理配置信息
        /// </summary>
        public ServiceInfo ServiceInfo { get; set; }

        /// <summary>
        /// 服务代理类型
        /// </summary>
        public Type ServiceType { get; set; }
    }
}
