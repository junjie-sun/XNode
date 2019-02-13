// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.ServiceDiscovery.Zookeeper
{
    /// <summary>
    /// Zookeeper相关配置
    /// </summary>
    public class ZookeeperConfig
    {
        /// <summary>
        /// Zookeeper连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Zookeeper根路径
        /// </summary>
        public string BasePath { get; set; } = "/XNode";

        /// <summary>
        /// 等待ZooKeeper连接的时间。
        /// </summary>
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 执行ZooKeeper操作的重试等待时间。
        /// </summary>
        public TimeSpan OperatingTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// zookeeper会话超时时间。
        /// </summary>
        public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromSeconds(20);

        /// <summary>
        /// 是否只读，默认为false。
        /// </summary>
        public bool ReadOnly { get; set; } = false;

        /// <summary>
        /// 会话Id。
        /// </summary>
        public long SessionId { get; set; } = 0;

        /// <summary>
        /// 会话密码。
        /// </summary>
        public byte[] SessionPasswd { get; set; } = null;
    }
}
