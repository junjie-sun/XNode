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
    public class IdleStateHearBeatRespHandler : ChannelHandlerAdapter
    {
        private ILogger logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        public IdleStateHearBeatRespHandler(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<IdleStateHearBeatRespHandler>();
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var xnodeMessage = (Message)message;
            if (xnodeMessage.Header != null && xnodeMessage.Header.Type == MessageType.HEARTBEAT_REQ)
            {
                logger.LogDebug($"Receive heart beat message request. Message={xnodeMessage}, Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
                var heartBeat = BuildHeatBeat();
                logger.LogDebug($"Send heart beat message response. HeartBeat={heartBeat}, Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
                context.WriteAndFlushAsync(heartBeat);
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
            header.Type = MessageType.HEARTBEAT_RESP;
            message.Header = header;
            return message;
        }
    }
}
