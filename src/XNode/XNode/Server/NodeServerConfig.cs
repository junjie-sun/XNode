// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using XNode.Security;
using XNode.Serializer;
using XNode.Server.Configuration;
using XNode.ProtocolStack;
using XNode.Server.Route;
using XNode.Communication;

namespace XNode.Server
{
    /// <summary>
    /// XNode服务端配置类
    /// </summary>
    public class NodeServerConfig
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
        /// 服务配置
        /// </summary>
        public IList<ServiceInfo> ServiceConfigs { get; set; }

        /// <summary>
        /// 服务提供器实例
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 协议栈工厂实例
        /// </summary>
        public IProtocolStackFactory ProtocolStackFactory { get; set; }

        /// <summary>
        /// 路由工厂实例
        /// </summary>
        public IRouteFactory RouteFactory { get; set; }

        /// <summary>
        /// 序列化器实例
        /// </summary>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// 服务处理器实例
        /// </summary>
        public IServiceProcessor ServiceProcessor { get; set; }

        /// <summary>
        /// 服务调用器实例
        /// </summary>
        public IServiceInvoker ServiceInvoker { get; set; }

        /// <summary>
        /// 登录验证器实例
        /// </summary>
        public ILoginValidator LoginValidator { get; set; }

        /// <summary>
        /// 服务端底层通信实现
        /// </summary>
        public IServer Communication { get; set; }
    }
}
