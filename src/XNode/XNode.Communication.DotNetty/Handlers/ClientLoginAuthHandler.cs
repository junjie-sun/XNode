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
    /// <summary>
    /// 用于握手认证，在通道激活时发起握手请求
    /// </summary>
    public class ClientLoginAuthHandler : ChannelHandlerAdapter
    {
        private ILogger logger;

        private Func<string, Task<LoginRequestData>> getLoginRequestDataHandler;

        private Func<string, byte[], IDictionary<string, byte[]>, Task<byte>> loginResponseHandler;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="getLoginRequestDataHandler">获取登录请求数据Handler</param>
        /// <param name="loginResponseHandler">登录响应Handler</param>
        public ClientLoginAuthHandler(ILoggerFactory loggerFactory, Func<string, Task<LoginRequestData>> getLoginRequestDataHandler, Func<string, byte[], IDictionary<string, byte[]>, Task<byte>> loginResponseHandler)
        {
            logger = loggerFactory.CreateLogger<ClientLoginAuthHandler>();
            this.getLoginRequestDataHandler = getLoginRequestDataHandler;
            this.loginResponseHandler = loginResponseHandler;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            if (getLoginRequestDataHandler != null)
            {
                var ip = context.GetRemoteAddress().Address.MapToIPv4().ToString();
                var port = context.GetRemotePort();
                getLoginRequestDataHandler($"{ip}:{port}").ContinueWith((t) =>
                {
                    context.WriteAndFlushAsync(BuildLoginReq(t.Result));
                });
            }
            else
            {
                context.WriteAndFlushAsync(BuildLoginReq());
            }
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var msg = (Message)message;

            //如果是握手应答消息，需要判断是否认证成功
            if (msg.Header != null && msg.Header.Type == MessageType.LOGIN_RESP)
            {
                if (loginResponseHandler != null)
                {
                    Task.Run(() =>
                    {
                        var ip = context.GetRemoteAddress().Address.MapToIPv4().ToString();
                        var port = context.GetRemotePort();
                        loginResponseHandler($"{ip}:{port}", msg.Body, msg.Header.Attachments).ContinueWith(t =>
                        {
                            var loginResult = t.Result;
                            if (loginResult == 0)
                            {
                                logger.LogInformation($"Login success. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}, Message={msg}");
                            }
                            else
                            {
                                logger.LogError($"Login failed. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}, LoginResult={loginResult}");
                                //握手失败
                                context.CloseAsync();
                            }
                        });
                    });
                }
                else
                {
                    var loginResult = msg.Body;
                    if (loginResult.Length != 1 || loginResult[0] != (byte)0)
                    {
                        logger.LogError($"Login failed. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}, LoginResult={loginResult[0]}");
                        //握手失败
                        context.CloseAsync();
                    }
                    else
                    {
                        logger.LogInformation($"Login success. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}, Message={msg}");
                        context.FireChannelRead(message);
                    }
                }
            }
            else
            {
                context.FireChannelRead(message);
            }
        }

        private Message BuildLoginReq(LoginRequestData reqData = null)
        {
            var message = new Message();
            var header = new MessageHeader
            {
                Type = MessageType.LOGIN_REQ,
                Attachments = reqData?.Attachments
            };
            message.Header = header;
            message.Body = reqData?.Body;
            return message;
        }
    }
}
