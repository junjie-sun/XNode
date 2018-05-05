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
    public static class ChannelHandlerContextExtensions
    {
        public static IPEndPoint GetRemoteAddress(this IChannelHandlerContext context)
        {
            return (IPEndPoint)context.Channel.RemoteAddress;
        }

        public static string GetRemoteNetString(this IChannelHandlerContext context)
        {
            var address = context.GetRemoteAddress();
            return $"{address.Address.MapToIPv4().ToString()}:{address.Port}";
        }

        public static string GetRemoteIP(this IChannelHandlerContext context)
        {
            var address = context.GetRemoteAddress();
            return address.Address.MapToIPv4().ToString();
        }

        public static int GetRemotePort(this IChannelHandlerContext context)
        {
            var address = context.GetRemoteAddress();
            return address.Port;
        }

        public static IPEndPoint GetLocalAddress(this IChannelHandlerContext context)
        {
            return (IPEndPoint)context.Channel.LocalAddress;
        }

        public static string GetLocalNetString(this IChannelHandlerContext context)
        {
            var address = context.GetLocalAddress();
            return $"{address.Address.MapToIPv4().ToString()}:{address.Port}";
        }

        public static string GetLocalIP(this IChannelHandlerContext context)
        {
            var address = context.GetLocalAddress();
            return address.Address.MapToIPv4().ToString();
        }

        public static int GetLocalPort(this IChannelHandlerContext context)
        {
            var address = context.GetLocalAddress();
            return address.Port;
        }
    }

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
