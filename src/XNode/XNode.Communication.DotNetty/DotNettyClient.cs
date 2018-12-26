// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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

        private string host, localHost;

        private int port;

        private int? localPort;

        private int reconnectCount;

        private int reconnectInterval;

        private TaskCompletionSource<object> connectTcs;

        private CancellationTokenSource connectCts;

        private ClientStatus status;

        private bool allowReconnect = false;

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
        /// 客户端状态
        /// </summary>
        public ClientStatus Status
        {
            get
            {
                return status;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host">服务端地址</param>
        /// <param name="port">服务端端口</param>
        /// <param name="localHost">本地地址</param>
        /// <param name="localPort">本地端口</param>
        /// <param name="reconnectCount">当发生网络错误时尝试重新连接的次数，-1表示无限，默认为-1</param>
        /// <param name="reconnectInterval">每次尝试重新连接的时间间隔，单位：毫秒，默认为3000毫秒</param>
        public DotNettyClient(string host, int port, string localHost = null, int? localPort = null, int reconnectCount = -1, int reconnectInterval = 3000)
        {
            loggerFactory = LoggerManager.ClientLoggerFactory;
            logger = loggerFactory.CreateLogger<DotNettyClient>();
            requestManager = new RequestManager(loggerFactory);
            this.port = port;
            this.host = host;
            this.localPort = localPort;
            this.localHost = localHost;
            this.reconnectCount = reconnectCount;
            this.reconnectInterval = reconnectInterval;
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

            allowReconnect = true;
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
            if (status != ClientStatus.Connected)
            {
                throw new Exception("Client is not connected");
            }

            var request = requestManager.CreateRequest(timeout);
            logger.LogDebug($"Client send one way message. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}, RequestId={request.Id}");
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
            if (status != ClientStatus.Connected)
            {
                throw new Exception("Client is not connected");
            }

            var request = requestManager.CreateRequest(timeout);
            logger.LogDebug($"Client send message. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}, RequestId={request.Id}");
            try
            {
                await channel.WriteAndFlushAsync(BuildServiceReqMessage(request.Id, attachments, msg, false));
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is ClosedChannelException || ex.InnerException is SocketException)
                {
                    throw new NetworkException(host, port, ex.InnerException.Message);
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
            status = ClientStatus.Closed;
            allowReconnect = false;
            await BootstrapManager.CloseAsync(channelName);
            logger.LogDebug($"Client closed. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}");
        }

        /// <summary>
        /// 执行与服务端连接
        /// </summary>
        /// <returns></returns>
        private async Task DoConnectAsync()
        {
            logger.LogDebug($"Client connect beginning. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}");

            if (connectTcs != null)
            {
                logger.LogError($"Client connect has begun. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}");
                throw new InvalidOperationException($"Client connect has begun. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}");
            }

            try
            {
                status = ClientStatus.Connecting;
                var dotNettyClientInfo = new DotNettyClientInfo()
                {
                    Host = host,
                    Port = port,
                    LocalHost = localHost,
                    LocalPort = localPort,
                    RequestManager = requestManager,
                    InactiveHandler = InactiveHandler,
                    GetLoginRequestDataHandler = GetLoginRequestData,
                    LoginResponseHandler = LoginResponse
                };
                channelName = dotNettyClientInfo.ChannelName;
                //发起异步连接操作
                channel = await BootstrapManager.ConnectAsync(dotNettyClientInfo);
            }
            catch (Exception ex)
            {
                status = ClientStatus.Closed;
                logger.LogError(ex, $"Client connect has error. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}, ExceptionMessage={ex.Message}, ExceptionStackTrace={ex.StackTrace}");
                throw new NetworkException(host, port, ex.Message);
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
                status = ClientStatus.Connected;
                logger.LogDebug($"Client connect finished. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}");
            }
            catch (Exception ex)
            {
                await CloseAsync();
                logger.LogError(ex, $"Client login has error. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}, ExceptionMessage={ex.Message}, ExceptionStackTrace={ex.StackTrace}");
                throw new NetworkException(host, port, ex.Message);
            }
            finally
            {
                connectCts = null;
                connectTcs = null;
            }
        }

        /// <summary>
        /// 断开连接处理器
        /// </summary>
        /// <returns></returns>
        private Task InactiveHandler()
        {
            if (reconnectCount == 0 || Interlocked.CompareExchange(ref isInactiveHandle, 1, 0) == 1)
            {
                return Task.CompletedTask;
            }

            return Task.Run(async () =>
            {
                status = ClientStatus.Closed;
                await BootstrapManager.CloseAsync(channelName);

                logger.LogDebug($"Client reconnect: close connect. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}");

                int i = 0;

                while (reconnectCount == -1 || i < reconnectCount)
                {
                    if (reconnectCount != -1) { i++; }

                    Thread.Sleep(reconnectInterval);

                    if (!allowReconnect) { break; }

                    try
                    {
                        logger.LogDebug($"Client reconnect: connecting. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}");
                        await DoConnectAsync();
                        logger.LogDebug($"Client reconnect: connect success. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Client reconnect: connect error. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}, ExceptionMessage={ex.Message}, ExceptionStackTrace={ex.StackTrace}");
                    }
                }
                isInactiveHandle = 0;
                logger.LogDebug($"Client reconnect finished. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}");
            });
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
                    logger.LogError(ex, $"DotNettyClient.OnRecieveLoginResponse error. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}, ExceptionMessage={ex.Message}, ExceptionStackTrace={ex.StackTrace}");
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
