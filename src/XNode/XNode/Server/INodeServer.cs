// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Server.Route;

namespace XNode.Server
{
    /// <summary>
    /// NodeServer启动时触发的委托
    /// </summary>
    /// <param name="arg"></param>
    public delegate void NodeServerStartDelegate(NodeServerStartEventArg arg);

    /// <summary>
    /// NodeServer停止时触发的委托
    /// </summary>
    /// <param name="arg"></param>
    public delegate void NodeServerStopDelegate(NodeServerStopEventArg arg);

    /// <summary>
    /// XNode服务端接口
    /// </summary>
    public interface INodeServer
    {
        /// <summary>
        /// XNode服务器启动前触发
        /// </summary>
        event NodeServerStartDelegate OnStarting;

        /// <summary>
        /// XNode服务器启动完成后触发
        /// </summary>
        event NodeServerStartDelegate OnStarted;

        /// <summary>
        /// XNode服务器停止前触发
        /// </summary>
        event NodeServerStopDelegate OnStopping;

        /// <summary>
        /// XNode服务器停止完成后触发
        /// </summary>
        event NodeServerStopDelegate OnStopped;

        /// <summary>
        /// 获取路由管理器
        /// </summary>
        IRouteManager RouteManager { get; }

        /// <summary>
        /// 启动XNode服务器
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// 关闭XNode服务器
        /// </summary>
        Task StopAsync();
    }

    /// <summary>
    /// 服务器启动事件参数
    /// </summary>
    public class NodeServerStartEventArg
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host">服务地址</param>
        /// <param name="port">服务端口</param>
        /// <param name="routes">服务路由</param>
        public NodeServerStartEventArg(string host, int port, IEnumerable<RouteDescription> routes)
        {
            Host = host;
            Port = port;
            Routes = routes;
        }

        /// <summary>
        /// 获取服务地址
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// 获取服务端口
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// 获取服务路由
        /// </summary>
        public IEnumerable<RouteDescription> Routes { get; }
    }

    /// <summary>
    /// 服务器停止事件参数
    /// </summary>
    public class NodeServerStopEventArg
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host">服务地址</param>
        /// <param name="port">服务端口</param>
        /// <param name="routes">服务路由</param>
        public NodeServerStopEventArg(string host, int port, IEnumerable<RouteDescription> routes)
        {
            Host = host;
            Port = port;
            Routes = routes;
        }

        /// <summary>
        /// 获取服务地址
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// 获取服务端口
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// 获取服务路由
        /// </summary>
        public IEnumerable<RouteDescription> Routes { get; }
    }
}
