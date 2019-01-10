// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
            return $"{address.Address.ToIPString()}:{address.Port}";
        }

        /// <summary>
        /// 获取远程IP
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetRemoteIP(this IChannelHandlerContext context)
        {
            var address = context.GetRemoteAddress();
            return address.Address.ToIPString();
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
            return $"{address.Address.ToIPString()}:{address.Port}";
        }

        /// <summary>
        /// 获取本地IP
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetLocalIP(this IChannelHandlerContext context)
        {
            var address = context.GetLocalAddress();
            return address.Address.ToIPString();
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
            return builder.ConfigCommunicationFactory((c) => { return new DotNettyClient(c.Host, c.Port, c.LocalHost, c.LocalPort); });
        }
    }

    /// <summary>
    /// Host字符串扩展方法 
    /// </summary>
    public static class HostStringExtensions
    {
        /// <summary>
        /// 将Host字符串转换为IPAddress
        /// </summary>
        /// <param name="host">Host字符串，可以是IP、主机名或域名</param>
        /// <returns></returns>
        public async static Task<IPAddress> ToIPAddress(this string host)
        {
            if (!IPAddress.TryParse(host, out IPAddress ipAddress))
            {
                var ips = await Dns.GetHostAddressesAsync(host);
                if (ips.Length > 0)
                {
                    foreach (var ip in ips)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddress = ip;
                            break;
                        }
                    }
                }
                if (ipAddress == null)
                {
                    throw new InvalidOperationException("Host is invalid.");
                }
            }
            return ipAddress;
        }
    }

    /// <summary>
    /// IPAddress扩展方法
    /// </summary>
    public static class IPAddressExtensions
    {
        /// <summary>
        /// 获取IP地址
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static string ToIPString(this IPAddress ipAddress)
        {
            return ipAddress.MapToIPv4().ToString();
        }
    }
}
