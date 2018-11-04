// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using XNode.Client;
using XNode.Server;
using XNode.Server.Configuration;

namespace XNode.Communication.DotNetty
{
    /// <summary>
    /// ChannelHandler扩展方法
    /// </summary>
    public static class ChannelHandlerContextExtensions
    {
        /// <summary>
        /// 获取远程地址
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IPEndPoint GetRemoteAddress(this IChannelHandlerContext context)
        {
            return (IPEndPoint)context.Channel.RemoteAddress;
        }

        /// <summary>
        /// 获取远程IP与端口（IP:Port）
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetRemoteNetString(this IChannelHandlerContext context)
        {
            var address = context.GetRemoteAddress();
            return $"{address.Address.MapToIPv4().ToString()}:{address.Port}";
        }

        /// <summary>
        /// 获取远程IP
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetRemoteIP(this IChannelHandlerContext context)
        {
            var address = context.GetRemoteAddress();
            return address.Address.MapToIPv4().ToString();
        }

        /// <summary>
        /// 获取远程端口
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static int GetRemotePort(this IChannelHandlerContext context)
        {
            var address = context.GetRemoteAddress();
            return address.Port;
        }

        /// <summary>
        /// 获取本地地址
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IPEndPoint GetLocalAddress(this IChannelHandlerContext context)
        {
            return (IPEndPoint)context.Channel.LocalAddress;
        }

        /// <summary>
        /// 获取本地IP和端口（IP:Port）
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetLocalNetString(this IChannelHandlerContext context)
        {
            var address = context.GetLocalAddress();
            return $"{address.Address.MapToIPv4().ToString()}:{address.Port}";
        }

        /// <summary>
        /// 获取本地IP
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetLocalIP(this IChannelHandlerContext context)
        {
            var address = context.GetLocalAddress();
            return address.Address.MapToIPv4().ToString();
        }

        /// <summary>
        /// 获取本地端口
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static int GetLocalPort(this IChannelHandlerContext context)
        {
            var address = context.GetLocalAddress();
            return address.Port;
        }
    }

    /// <summary>
    /// NodeServer构造器扩展方法
    /// </summary>
    public static class NodeServerBuilderExtensions
    {
        /// <summary>
        /// 加载DotNetty服务端
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="serverInfo">服务器信息</param>
        /// <returns></returns>
        public static INodeServerBuilder UseDotNetty(this INodeServerBuilder builder, ServerInfo serverInfo)
        {
            return builder.ConfigCommunication(new DotNettyServer(serverInfo.Host, serverInfo.Port));
        }
    }

    /// <summary>
    /// NodeClient构造器扩展方法
    /// </summary>
    public static class NodeClientBuilderExtensions
    {
        /// <summary>
        /// 加载DotNetty客户端
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static INodeClientBuilder UseDotNetty(this INodeClientBuilder builder)
        {
            return builder.ConfigCommunicationFactory((c) => { return new DotNettyClient(c.Host, c.Port, c.LocalHost, c.LocalPort, c.ReconnectCount, c.ReconnectInterval); });
        }
    }
}
