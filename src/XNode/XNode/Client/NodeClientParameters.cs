// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using XNode.Client.Configuration;
using XNode.Security;
using XNode.Serializer;
using XNode.ProtocolStack;
using XNode.Communication;
using Microsoft.Extensions.Logging;

namespace XNode.Client
{
    /// <summary>
    /// XNode客户端参数类
    /// </summary>
    public class NodeClientParameters
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
        /// 序列化器实例
        /// </summary>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// 协议栈工厂实例
        /// </summary>
        public IProtocolStackFactory ProtocolStackFactory { get; set; }

        /// <summary>
        /// 登录处理器
        /// </summary>
        public ILoginHandler LoginHandler { get; set; }

        /// <summary>
        /// 底层客户端通信组件
        /// </summary>
        public IClient Communication { get; set; }

        /// <summary>
        /// 创建NodeClientParameters实例
        /// </summary>
        /// <param name="connectionInfoList">连接信息</param>
        /// <param name="serializer">序列化器实例</param>
        /// <param name="communicationFactory">底层客户端通信组件工厂</param>
        /// <param name="loginHandler">登录处理器</param>
        /// <param name="protocolStackFactory">协议栈工厂实例</param>
        /// <returns></returns>
        public static IList<NodeClientParameters> Create(IList<ConnectionInfo> connectionInfoList, ISerializer serializer, Func<ConnectionInfo, IClient> communicationFactory, ILoginHandler loginHandler = null, IProtocolStackFactory protocolStackFactory = null)
        {
            var list = new List<NodeClientParameters>();
            foreach (var connectionInfo in connectionInfoList)
            {
                var item = new NodeClientParameters()
                {
                    Host = connectionInfo.Host,
                    Port = connectionInfo.Port,
                    LocalHost = connectionInfo.LocalHost,
                    LocalPort = connectionInfo.LocalPort,
                    Serializer = serializer,
                    ProtocolStackFactory = protocolStackFactory,
                    LoginHandler = loginHandler,
                    Communication = communicationFactory(connectionInfo)
                };
                list.Add(item);
            }
            return list;
        }
    }
}
