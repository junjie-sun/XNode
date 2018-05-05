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

        /// <summary>
        /// 当发生网络错误时尝试重新连接的次数，-1表示无限，默认为-1
        /// </summary>
        public int ReconnectCount { get; set; } = -1;

        /// <summary>
        /// 每次尝试重新连接的时间间隔，单位：毫秒，默认为30000毫秒
        /// </summary>
        public int ReconnectInterval { get; set; } = 30000;
    }
}
