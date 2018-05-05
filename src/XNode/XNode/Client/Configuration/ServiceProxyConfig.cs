// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Client.Configuration
{
    /// <summary>
    /// 服务代理配置
    /// </summary>
    public class ServiceProxyConfig
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ProxyName { get; set; }

        /// <summary>
        /// 连接配置
        /// </summary>
        public List<ConnectionInfo> Connections { get; set; }

        /// <summary>
        /// 服务配置
        /// </summary>
        public List<ServiceInfo> Services { get; set; }
    }
}
