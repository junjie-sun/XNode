// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.ServiceDiscovery.Zookeeper
{
    /// <summary>
    /// 服务发布器接口
    /// </summary>
    public interface IServicePublisher : IDisposable
    {
        /// <summary>
        /// 服务订阅
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="host">服务Host</param>
        /// <param name="port">服务端口</param>
        /// <param name="serializerName">序列化器名称</param>
        /// <returns></returns>
        IServicePublisher Publish(Type serviceType, string host, int port, string serializerName);
    }
}
