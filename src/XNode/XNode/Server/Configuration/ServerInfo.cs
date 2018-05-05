// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Server.Configuration
{
    /// <summary>
    /// 服务器基本配置
    /// </summary>
    public class ServerInfo
    {
        /// <summary>
        /// 服务地址
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port { get; set; }
    }
}
