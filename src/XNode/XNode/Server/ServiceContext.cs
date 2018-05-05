// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using XNode.Server.Route;

namespace XNode.Server
{
    /// <summary>
    /// 服务上下文，每次服务调用请求共享一个实例
    /// </summary>
    public class ServiceContext
    {
        private static AsyncLocal<ServiceContext> current = new AsyncLocal<ServiceContext>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host">服务地址</param>
        /// <param name="port">服务端口</param>
        /// <param name="identity">身份标识</param>
        /// <param name="remoteAddress">客户端地址</param>
        /// <param name="route">服务路由信息</param>
        /// <param name="actionParamList">Action请求所传递的参数列表</param>
        /// <param name="attachments">服务请求所传递的附加数据</param>
        public ServiceContext(string host,
            int port,
            string identity,
            IPEndPoint remoteAddress,
            RouteDescription route,
            IList<byte[]> actionParamList,
            IDictionary<string, byte[]> attachments)
        {
            if (current.Value != null)
            {
                throw new InvalidOperationException("ServiceContext has instantiated.");
            }

            Identity = identity;
            RemoteAddress = remoteAddress;
            Route = route;
            ActionParamList = actionParamList;
            Attachments = attachments;

            current.Value = this;
        }

        /// <summary>
        /// 获取当前服务上下文
        /// </summary>
        public static ServiceContext Current
        {
            get
            {
                return current.Value;
            }
        }

        /// <summary>
        /// 服务地址
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// 身份标识
        /// </summary>
        public string Identity { get; }

        /// <summary>
        /// 客户端地址
        /// </summary>
        public IPEndPoint RemoteAddress { get; }

        /// <summary>
        /// 服务路由信息
        /// </summary>
        public RouteDescription Route { get; }

        /// <summary>
        /// Action请求所传递的参数列表
        /// </summary>
        public IList<byte[]> ActionParamList { get; }

        /// <summary>
        /// 服务请求所传递的附加数据
        /// </summary>
        public IDictionary<string, byte[]> Attachments { get; }

        /// <summary>
        /// 用于服务调用过程中共享的数据
        /// </summary>
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();
    }
}
