// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace XNode.ServiceDiscovery.Zookeeper
{
    /// <summary>
    /// 服务发布信息
    /// </summary>
    [DataContract]
    public class ServicePublishInfo
    {
        /// <summary>
        /// 服务Host
        /// </summary>
        [DataMember(Order = 1)]
        public string Host { get; set; }

        /// <summary>
        /// 服务端口
        /// </summary>
        [DataMember(Order = 2)]
        public int Port { get; set; }

        /// <summary>
        /// 序列化器名称
        /// </summary>
        [DataMember(Order = 3)]
        public string SerializerName { get; set; }
    }
}
