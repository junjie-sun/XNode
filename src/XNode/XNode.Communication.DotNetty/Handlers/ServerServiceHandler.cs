// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Communication.ProtocolStack;

namespace XNode.Communication.DotNetty.Handlers
{
    public class ServerServiceHandler : ChannelHandlerAdapter
    {
        private ILogger logger;

        private Func<byte[], IDictionary<string, byte[]>, LoginState, Task<ResponseData>> serviceRecieveHandler;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="serviceRecieveHandler">服务请求接收Handler</param>
        public ServerServiceHandler(ILoggerFactory loggerFactory, Func<byte[], IDictionary<string, byte[]>, LoginState, Task<ResponseData>> serviceRecieveHandler)
        {
            logger = loggerFactory.CreateLogger<ServerServiceHandler>();
            this.serviceRecieveHandler = serviceRecieveHandler;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {

            var msg = (Message)message;

            var loginState = ChannelState.GetLoginState(context);

            //如果是服务请求消息则进行处理，其它消息则透传
            if (msg.Header != null && msg.Header.Type == MessageType.SERVICE_REQ)
            {
                logger.LogDebug($"Recieve service message. RequestId={msg.Header.RequestId}, Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
                if (serviceRecieveHandler != null)
                {
                    Task.Run(() =>
                    {
                        var handlerTask = serviceRecieveHandler(msg.Body, msg.Header.Attachments, loginState);
                        if (handlerTask != null)
                        {
                            handlerTask.ContinueWith(task =>
                            {
                                context.WriteAndFlushAsync(BuildResponse(msg.Header.RequestId, task.Result));
                                logger.LogDebug($"Service is response. RequestId={msg.Header.RequestId}, Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
                            });
                        }
                    });
                }
            }
            else if (msg.Header != null && msg.Header.Type == MessageType.ONE_WAY)
            {
                logger.LogDebug($"Recieve oneway service message. RequestId={msg.Header.RequestId}, Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
                if (serviceRecieveHandler != null)
                {
                    Task.Run(() =>
                    {
                        serviceRecieveHandler?.Invoke(msg.Body, msg.Header.Attachments, loginState);
                    });
                }
            }
            else
            {
                context.FireChannelRead(message);
            }
        }

        private Message BuildResponse(long requestId, ResponseData res)
        {
            var message = new Message();
            var header = new MessageHeader();
            header.Type = MessageType.SERVICE_RESP;
            header.RequestId = requestId;
            header.Attachments = res.Attachments;
            message.Header = header;
            message.Body = res.Data;
            return message;
        }
    }
}
