// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using XNode.Communication.ProtocolStack;

namespace XNode.Communication.DotNetty.Handlers
{
    /// <summary>
    /// 用于处理服务响应消息
    /// </summary>
    public class ClientServiceHandler : ChannelHandlerAdapter
    {
        private ILogger logger;

        private RequestManager requestManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="requestManager">请求管理器</param>
        public ClientServiceHandler(ILoggerFactory loggerFactory, RequestManager requestManager)
        {
            logger = loggerFactory.CreateLogger<ClientServiceHandler>();
            this.requestManager = requestManager;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var msg = (Message)message;

            //如果是服务响应消息，处理，其它消息透传
            if (msg.Header != null && msg.Header.Type == MessageType.SERVICE_RESP)
            {
                logger.LogDebug($"Get request result. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
                requestManager.CompleteRequest(msg.Header.RequestId, msg);
            }
            else
            {
                context.FireChannelRead(message);
            }
        }
    }
}
