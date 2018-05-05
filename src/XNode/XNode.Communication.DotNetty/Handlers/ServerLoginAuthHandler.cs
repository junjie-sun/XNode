// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using XNode.Communication.ProtocolStack;

namespace XNode.Communication.DotNetty.Handlers
{
    /// <summary>
    /// 握手接入和安全认证
    /// </summary>
    public class ServerLoginAuthHandler : ChannelHandlerAdapter
    {
        private ILogger logger;

        private Func<LoginAuthInfo, Task<LoginResponseData>> loginRecieveHandler;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="loginRecieveHandler">登录请求接收Handler</param>
        public ServerLoginAuthHandler(ILoggerFactory loggerFactory, Func<LoginAuthInfo, Task<LoginResponseData>> loginRecieveHandler)
        {
            logger = loggerFactory.CreateLogger<ServerLoginAuthHandler>();
            this.loginRecieveHandler = loginRecieveHandler;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            ChannelState.SetLoginState(context, false);

            logger.LogDebug($"Channel actived. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");

            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            ChannelState.RemoveLoginState(context);

            logger.LogDebug($"Channel inactived. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");

            base.ChannelInactive(context);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var msg = (Message)message;

            var loginState = ChannelState.GetLoginState(context);

            //如果是握手请求消息，处理，其它消息透传
            if (msg.Header != null && msg.Header.Type == MessageType.LOGIN_REQ && loginState != null)
            {
                loginState.IsLoginSuccess = false;

                if (loginRecieveHandler != null)
                {
                    Task.Run(() =>
                    {
                        loginRecieveHandler(new LoginAuthInfo() { Body = msg.Body, Attachments = msg.Header.Attachments, RemoteAddress = context.GetRemoteAddress() }).ContinueWith(t =>
                        {
                            var resData = t.Result;
                            if (resData != null)
                            {
                                loginState.IsLoginSuccess = resData.AuthResult;
                                loginState.Identity = resData.AuthIdentity;
                                loginState.RemoteAddress = context.GetRemoteAddress();
                                if (loginState.IsLoginSuccess)
                                {
                                    logger.LogInformation($"Login success. Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
                                    var loginResp = BuildResponse(resData);
                                    context.WriteAndFlushAsync(loginResp);
                                }
                                else
                                {
                                    logger.LogDebug($"Login failed. MessageType={msg.Header.Type}, Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}, AuthStatusCode={resData.AuthStatusCode}, AuthFailedMessage={resData.AuthFailedMessage}");
                                    var loginResp = BuildResponse(resData);
                                    context.WriteAndFlushAsync(loginResp).ContinueWith(task =>
                                    {
                                        context.CloseAsync();
                                    });
                                }
                            }
                            else
                            {
                                logger.LogDebug($"Drop message because LoginResponseData is null. MessageType={msg.Header.Type}, Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
                                var loginResp = BuildResponse((byte)AuthStatusCodes.LoginResponseDataIsNull);
                                context.WriteAndFlushAsync(loginResp).ContinueWith(task =>
                                {
                                    context.CloseAsync();
                                });
                            }
                        });
                    });
                }
                else
                {
                    logger.LogDebug($"Drop message because LoginRecieveHandler is null. MessageType={msg.Header.Type}, Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
                    var loginResp = BuildResponse((byte)AuthStatusCodes.LoginRecieveHandlerIsNull);
                    context.WriteAndFlushAsync(loginResp).ContinueWith(task =>
                    {
                        context.CloseAsync();
                    });
                }
            }
            else
            {
                if (loginState != null && loginState.IsLoginSuccess)
                {
                    context.FireChannelRead(msg);
                }
                else
                {
                    logger.LogDebug($"Drop message because no login. MessageType={msg.Header.Type}, Local={context.GetLocalNetString()}, Remote={context.GetRemoteNetString()}");
                    var loginResp = BuildResponse((byte)AuthStatusCodes.NoLogin);
                    context.WriteAndFlushAsync(loginResp).ContinueWith(task =>
                    {
                        context.CloseAsync();
                    });
                }
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.FireExceptionCaught(exception);
        }

        private Message BuildResponse(LoginResponseData res)
        {
            var message = new Message();
            var header = new MessageHeader
            {
                Type = MessageType.LOGIN_RESP,
                Attachments = res.Attachments
            };
            message.Header = header;
            message.Body = new byte[1] { res.AuthStatusCode };
            return message;
        }

        private Message BuildResponse(byte authStatusCode)
        {
            var message = new Message();
            var header = new MessageHeader
            {
                Type = MessageType.LOGIN_RESP
            };
            message.Header = header;
            message.Body = new byte[1] { authStatusCode };
            return message;
        }
    }
}
