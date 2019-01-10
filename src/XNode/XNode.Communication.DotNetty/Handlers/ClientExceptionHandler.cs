// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using XNode.Logging;

namespace XNode.Communication.DotNetty.Handlers
{
    /// <summary>
    /// 客户端异常Handler
    /// </summary>
    public class ClientExceptionHandler : ChannelHandlerAdapter
    {
        private ILogger logger;

        private Func<string, Task> inactiveHandler;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="inactiveHandler">网络异常Handler</param>
        public ClientExceptionHandler(ILoggerFactory loggerFactory, Func<string, Task> inactiveHandler)
        {
            logger = loggerFactory.CreateLogger<ClientExceptionHandler>();
            this.inactiveHandler = inactiveHandler;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        public async override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            if (exception is SocketException)
            {
                logger.LogError(exception, $"Socket exception. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}, ExceptionMessage={exception.Message}");
            }
            else
            {
                logger.LogError(exception, $"Unhandle exception. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}, ExceptionMessage={exception.Message}");
            }
            await context.CloseAsync();
            logger.LogInformation($"Channel closed because has an unhandle exception. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
            context.FireExceptionCaught(exception);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public async override void ChannelInactive(IChannelHandlerContext context)
        {
            logger.LogInformation($"Channel inactived. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
            base.ChannelInactive(context);

            var channelName = context.GetChannelName();
            await inactiveHandler?.Invoke(channelName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void ChannelUnregistered(IChannelHandlerContext context)
        {
            logger.LogInformation($"Channel unregistered. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
            base.ChannelUnregistered(context);
        }
    }
}
