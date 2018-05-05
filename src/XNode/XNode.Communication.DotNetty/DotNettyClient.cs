// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XNode.Communication.DotNetty.Handlers;
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

        private Bootstrap bootstrap;

        private IEventLoopGroup group;

        private IChannel channel;

        private string host, localHost;

        private int port;

        private int? localPort;

        private int reconnectCount;

        private int reconnectInterval;

        private TaskCompletionSource<object> connectTcs;

        private ClientStatus status;

        private bool allowReconnect = false;

        private int isReconnecting = 0;

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
        /// <param name="reconnectInterval">每次尝试重新连接的时间间隔，单位：毫秒，默认为30000毫秒</param>
        public DotNettyClient(string host, int port, string localHost = null, int? localPort = null, int reconnectCount = -1, int reconnectInterval = 30000)
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

        /// <summary>
        /// 向服务端发起连接
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            if (bootstrap == null)
            {
                bootstrap = new Bootstrap();
                group = new MultithreadEventLoopGroup();

                var getLoginRequestDataHandler = CreateGetLoginRequestDataHandler();
                var loginResponseHandler = CreateLoginResponseHandler();

                bootstrap.Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        channel.Pipeline.AddLast("IdleStateHandler", new IdleStateHandler(0, 0, 10));        //自带心跳包方案，每隔指定时间检查是否有读和写操作，如果没有则触发userEventTriggered事件
                        channel.Pipeline.AddLast("MessageDecoder", new MessageDecoder(loggerFactory, 1024 * 1024, 4, 4, -8, 0));
                        channel.Pipeline.AddLast("MessageEncoder", new MessageEncoder(loggerFactory));
                        channel.Pipeline.AddLast("LoginAuthHandler", new ClientLoginAuthHandler(loggerFactory, getLoginRequestDataHandler, loginResponseHandler));
                        channel.Pipeline.AddLast("ServiceResultHandler", new ClientServiceHandler(loggerFactory, requestManager));
                        channel.Pipeline.AddLast("IdleStateHearBeatReqHandler", new IdleStateHearBeatReqHandler(loggerFactory));
                        channel.Pipeline.AddLast("ExceptionHandler", new ClientExceptionHandler(loggerFactory, this.ReconnectAsync));
                    }));
            }

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
            try
            {
                status = ClientStatus.Closed;
                allowReconnect = false;
                await channel.CloseAsync();
                logger.LogDebug($"Client closed. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}");
            }
            finally
            {
                await group.ShutdownGracefullyAsync();
                logger.LogDebug($"Group shutdowned. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}");
            }
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
                throw new InvalidOperationException("Client connect has begun.");
            }

            connectTcs = new TaskCompletionSource<object>();

            try
            {
                status = ClientStatus.Connecting;
                //发起异步连接操作
                if (!string.IsNullOrEmpty(localHost) && localPort != null)
                {
                    channel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(host), port), new IPEndPoint(IPAddress.Parse(localHost), localPort.Value));
                }
                else
                {
                    channel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(host), port));
                }
                await connectTcs.Task;
                status = ClientStatus.Connected;
                logger.LogDebug($"Client connect finished. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}");
            }
            catch (AggregateException ex)
            {
                status = ClientStatus.Closed;
                connectTcs = null;
                logger.LogError(ex, $"Client connect has error. Host={host}, Port={port}, LocalHost={localHost}, LocalPort={localPort}, ExceptionMessage={ex.InnerException.Message}, ExceptionStackTrace={ex.InnerException.StackTrace}");
                throw new NetworkException(host, port, ex.InnerException.Message);
            }
        }

        /// <summary>
        /// 重新连接到服务端
        /// </summary>
        /// <returns></returns>
        private Task ReconnectAsync()
        {
            return Task.Run(async () =>
            {
                if (reconnectCount == 0 || Interlocked.CompareExchange(ref isReconnecting, 1, 0) == 1)
                {
                    return;
                }

                status = ClientStatus.Closed;
                await channel.CloseAsync();

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
                isReconnecting = 0;
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

        private Func<Task<LoginRequestData>> CreateGetLoginRequestDataHandler()
        {
            if (OnSubmitLoginRequest == null)
            {
                return null;
            }

            return new Func<Task<LoginRequestData>>(() =>
            {
                return OnSubmitLoginRequest();
            });
        }

        private Func<byte[], IDictionary<string, byte[]>, Task<byte>> CreateLoginResponseHandler()
        {
            if (OnRecieveLoginResponse == null)
            {
                return null;
            }

            return new Func<byte[], IDictionary<string, byte[]>, Task<byte>>(async (message, attachments) =>
            {
                byte result = await OnRecieveLoginResponse(message, attachments);

                if (connectTcs != null)
                {
                    var tcs = connectTcs;
                    connectTcs = null;
                    if (result == 0)
                    {
                        tcs.SetResult(null);
                    }
                    else
                    {
                        tcs.SetException(new LoginAuthException(result, $"Login failed. LoginResult={result}"));
                    }
                }

                return result;
            });
        }
    }
}
