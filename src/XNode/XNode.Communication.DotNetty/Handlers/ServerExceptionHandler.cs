// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using XNode.Logging;

namespace XNode.Communication.DotNetty.Handlers
{
    /// <summary>
    /// 服务端异常Handler
    /// </summary>
    public class ServerExceptionHandler : ChannelHandlerAdapter
    {
        private ILogger logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        public ServerExceptionHandler(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<ServerExceptionHandler>();
        }

        public async override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            if (exception is ReadTimeoutException)
            {
                logger.LogError($"Timeout. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
                await context.CloseAsync();
                logger.LogInformation($"Connect closed because timeout. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
            }
            else
            {
                logger.LogError(exception, $"Unhandle exception. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}, ExceptionMessage={exception.Message}");
                await context.CloseAsync();                 //发生错误时主动关闭连接
                context.FireExceptionCaught(exception);     //如果只通过FireExceptionCaught抛出的异常不会导致进程崩溃和连接中断
            }
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            logger.LogInformation($"Channel inactived. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
        }

        public override void ChannelUnregistered(IChannelHandlerContext context)
        {
            logger.LogInformation($"Channel unregistered. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
        }
    }
}
