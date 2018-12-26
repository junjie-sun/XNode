// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Client.Configuration
{
    /// <summary>
    /// 连接配置
    /// </summary>
    public class ConnectionInfo
    {
        /// <summary>
        /// 服务地址
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 本地绑定地址
        /// </summary>
        public string LocalHost { get; set; }

        /// <summary>
        /// 本地绑定IP
        /// </summary>
        public int? LocalPort { get; set; }
    }
}
