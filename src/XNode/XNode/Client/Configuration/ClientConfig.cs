// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Client.Configuration
{
    /// <summary>
    /// 客户端配置
    /// </summary>
    public class ClientConfig
    {
        /// <summary>
        /// 服务代理列表
        /// </summary>
        public List<ServiceProxyConfig> ServiceProxies { get; set; }
    }
}
