// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using XNode.Communication.ProtocolStack;

namespace XNode.Communication.DotNetty.Handlers
{
    /// <summary>
    /// 心跳包请求handler
    /// </summary>
    public class IdleStateHearBeatReqHandler : ChannelHandlerAdapter
    {
        private ILogger logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        public IdleStateHearBeatReqHandler(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<IdleStateHearBeatReqHandler>();
        }

        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            var idleStateEvt = evt as IdleStateEvent;

            if (idleStateEvt == null)
            {
                base.UserEventTriggered(context, evt);
                return;
            }

            if (idleStateEvt.First && idleStateEvt.State == IdleStateEvent.AllIdleStateEvent.State)
            {
                var heartBeat = BuildHeatBeat();
                logger.LogDebug($"Send heart beat message request. HeartBeat={heartBeat}, Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
                context.WriteAndFlushAsync(heartBeat);
            }
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var xnodeMessage = (Message)message;
            if (xnodeMessage.Header != null && xnodeMessage.Header.Type == MessageType.HEARTBEAT_RESP)
            {
                logger.LogDebug($"Receive heart beat message response. Message={xnodeMessage}, Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
            }
            else
            {
                base.ChannelRead(context, message);
            }
        }

        private Message BuildHeatBeat()
        {
            var message = new Message();
            var header = new MessageHeader();
            header.Type = MessageType.HEARTBEAT_REQ;
            message.Header = header;
            return message;
        }
    }
}
