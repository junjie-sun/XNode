﻿// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using XNode.Common;
using XNode.Communication.ProtocolStack;
using XNode.Logging;

namespace XNode.Communication.DotNetty
{
    /// <summary>
    /// 客户端底层实现
    /// </summary>
    public class DotNettyClient : IClient
    {
        private ILoggerFactory loggerFactory;

        private ILogger logger;

        private RequestManager requestManager;

        private string channelName;

        private IChannel channel;

        private TaskCompletionSource<object> connectTcs;

        private CancellationTokenSource connectCts;

        private int isInactiveHandle = 0;

        /// <summary>
        /// 提交登录请求事件
        /// </summary>
        public event SubmitLoginRequestDelegate OnSubmitLoginRequest;

        /// <summary>
        /// 接收登录响应事件
        /// </summary>
        public event RecieveLoginResponseDelegate OnRecieveLoginResponse;

        /// <summary>
        /// 被动关闭事件
        /// </summary>
        public event PassiveClosedDelegate OnPassiveClosed;

        /// <summary>
        /// 客户端状态
        /// </summary>
        public ClientStatus Status { get; private set; }

        /// <summary>
        /// 服务地址
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// 本地地址
        /// </summary>
        public string LocalHost { get; }

        /// <summary>
        /// 本地端口
        /// </summary>
        public int? LocalPort { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host">服务端地址</param>
        /// <param name="port">服务端端口</param>
        /// <param name="localHost">本地地址</param>
        /// <param name="localPort">本地端口</param>
        public DotNettyClient(string host, int port, string localHost = null, int? localPort = null)
        {
            loggerFactory = LoggerManager.ClientLoggerFactory;
            logger = loggerFactory.CreateLogger<DotNettyClient>();
            requestManager = new RequestManager(loggerFactory);
            Port = port;
            var hostIPAddress = host.ToIPAddress().Result;
            Host = hostIPAddress.ToIPString();
            LocalPort = localPort;
            if (!string.IsNullOrWhiteSpace(localHost))
            {
                var localHostIPAddress = localHost.ToIPAddress().Result;
                LocalHost = localHostIPAddress.ToIPString();
            }
        }

        static DotNettyClient()
        {
            BootstrapManager.Init();
        }

        /// <summary>
        /// 向服务端发起连接
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            await DoConnectAsync();
        }

        /// <summary>
        /// 单向发送消息到服务器
        /// </summary>
        /// <param name="msg">消息数据</param>
        /// <param name="timeout">超时时长（毫秒）</param>
        /// <param name="attachments">消息附加数据</param>
        /// <returns></returns>
        public async Task SendOneWayAsync(byte[] msg, int timeout = 30000, IDictionary<string, byte[]> attachments = null)
        {
            if (Status != ClientStatus.Connected)
            {
                throw new Exception("Client is not connected");
            }

            var request = requestManager.CreateRequest(timeout);
            logger.LogDebug($"Client send one way message. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}, RequestId={request.Id}");
            await channel.WriteAndFlushAsync(BuildServiceReqMessage(request.Id, attachments, msg, true));
            await request.Task;
        }

        /// <summary>
        /// 发送消息到服务器
        /// </summary>
        /// <param name="msg">消息数据</param>
        /// <param name="timeout">超时时长（毫秒）</param>
        /// <param name="attachments">消息附加数据</param>
        /// <returns></returns>
        public async Task<RequestResult> SendAsync(byte[] msg, int timeout = 30000, IDictionary<string, byte[]> attachments = null)
        {
            if (Status != ClientStatus.Connected)
            {
                throw new Exception("Client is not connected");
            }

            var request = requestManager.CreateRequest(timeout);
            logger.LogDebug($"Client send message. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}, RequestId={request.Id}");
            try
            {
                await channel.WriteAndFlushAsync(BuildServiceReqMessage(request.Id, attachments, msg, false));
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is ClosedChannelException || ex.InnerException is SocketException)
                {
                    throw new NetworkException(Host, Port, ex.InnerException.Message);
                }
                throw ex;
            }
            var result = await request.Task;
            return new RequestResult() { Data = result.Body, Attachments = result.Header.Attachments };
        }

        /// <summary>
        /// 关闭与服务端的连接
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            Status = ClientStatus.Closed;
            await BootstrapManager.CloseAsync(channelName);
            logger.LogDebug($"Client closed. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}");
        }

        /// <summary>
        /// 执行与服务端连接
        /// </summary>
        /// <returns></returns>
        private async Task DoConnectAsync()
        {
            if (Status == ClientStatus.Connected)
            {
                return;
            }

            if (Status == ClientStatus.Connecting)
            {
                logger.LogError($"Client connect has begun. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}");
                throw new InvalidOperationException($"Client connect has begun. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}");
            }

            Status = ClientStatus.Connecting;

            logger.LogDebug($"Client connect beginning. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}");

            try
            {
                var dotNettyClientInfo = new DotNettyClientInfo()
                {
                    Host = Host,
                    Port = Port,
                    LocalHost = LocalHost,
                    LocalPort = LocalPort,
                    RequestManager = requestManager,
                    InactiveHandler = InactiveHandler,
                    GetLoginRequestDataHandler = GetLoginRequestData,
                    LoginResponseHandler = LoginResponse
                };
                //发起异步连接操作
                var oldChannel = channel;
                channel = await BootstrapManager.ConnectAsync(dotNettyClientInfo);
                if (oldChannel != null)
                {
                    await oldChannel.CloseAsync();
                }

                channelName = dotNettyClientInfo.ChannelName;
            }
            catch (Exception ex)
            {
                Status = ClientStatus.PassiveClosed;
                logger.LogError(ex, $"Client connect has error. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}, ExceptionMessage={ex.Message}, ExceptionStackTrace={ex.StackTrace}");
                throw new NetworkException(Host, Port, ex.Message);
            }

            try
            {
                connectTcs = new TaskCompletionSource<object>();
                connectCts = new CancellationTokenSource(3000);     //登录验证响应超时
                var token = connectCts.Token;
                token.Register(() =>
                {
                    try
                    {
                        connectTcs.SetException(new TimeoutException("Login response timeout."));
                    }
                    catch { }
                });

                await connectTcs.Task;      //等待登录验证响应
            }
            catch (Exception ex)
            {
                await CloseAsync();
                logger.LogError(ex, $"Client login has error. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}, ExceptionMessage={ex.Message}, ExceptionStackTrace={ex.StackTrace}");
                throw new NetworkException(Host, Port, ex.Message);
            }
            finally
            {
                connectCts = null;
                connectTcs = null;
            }

            Status = ClientStatus.Connected;
            logger.LogDebug($"Client connect finished. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}");
        }

        /// <summary>
        /// 断开连接处理器
        /// </summary>
        /// <returns></returns>
        private async Task InactiveHandler()
        {
            if (Interlocked.CompareExchange(ref isInactiveHandle, 1, 0) == 1)
            {
                return;
            }

            if (Status == ClientStatus.Closed || Status == ClientStatus.Connecting)
            {
                return;
            }

            Status = ClientStatus.PassiveClosed;
            await BootstrapManager.CloseAsync(channelName);

            logger.LogDebug($"Client inactive: close connect. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}");

            if (OnPassiveClosed != null)
            {
                await Task.Run(async () =>
                {
                    await OnPassiveClosed(this);
                    isInactiveHandle = 0;
                });
            }
            else
            {
                isInactiveHandle = 0;
            }
        }

        private Message BuildServiceReqMessage(long requestId, IDictionary<string, byte[]> attachments, byte[] msg, bool isOneWay)
        {
            var message = new Message();
            var header = new MessageHeader();
            header.RequestId = requestId;
            header.Type = isOneWay ? MessageType.ONE_WAY : MessageType.SERVICE_REQ;
            header.Attachments = attachments;
            message.Header = header;
            message.Body = msg;
            return message;
        }

        private async Task<byte> LoginResponse(byte[] message, IDictionary<string, byte[]> attachments)
        {
            if (connectCts == null || connectTcs == null)
            {
                return (byte)AuthStatusCodes.WaitLoginResponseTimeout;
            }

            connectCts.Dispose();

            if (connectCts == null || connectCts.IsCancellationRequested)
            {
                return (byte)AuthStatusCodes.WaitLoginResponseTimeout;
            }

            byte result;

            if (OnRecieveLoginResponse == null)
            {
                result = message[0];
            }
            else
            {
                try
                {
                    result = await OnRecieveLoginResponse(message, attachments);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"DotNettyClient.OnRecieveLoginResponse error. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}, ExceptionMessage={ex.Message}, ExceptionStackTrace={ex.StackTrace}");
                    result = (byte)AuthStatusCodes.ParseLoginResponseDataError;
                    connectTcs.SetException(ex);
                    return result;
                }
            }

            if (result == 0)
            {
                connectTcs.SetResult(null);
            }
            else
            {
                connectTcs.SetException(new LoginAuthException(result, $"Login failed. LoginResult={result}"));
            }

            return result;
        }

        private Task<LoginRequestData> GetLoginRequestData()
        {
            if (OnSubmitLoginRequest == null)
            {
                return Task.FromResult(new LoginRequestData());
            }

            return OnSubmitLoginRequest();
        }
    }
}
