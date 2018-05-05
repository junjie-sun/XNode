// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Server.Configuration
{
    /// <summary>
    /// 服务端配置
    /// </summary>
    public class ServerConfig
    {
        /// <summary>
        /// 服务器基本配置
        /// </summary>
        public ServerInfo ServerInfo { get; set; }

        /// <summary>
        /// 服务列表
        /// </summary>
        public List<ServiceInfo> Services { get; set; }
    }
}
