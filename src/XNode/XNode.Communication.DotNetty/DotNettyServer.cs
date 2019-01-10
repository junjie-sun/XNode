// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Handlers.Logging;
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
using System.Threading.Tasks;
using XNode.Common;
using XNode.Communication.DotNetty.Handlers;
using XNode.Logging;
using DotNettyLogging = DotNetty.Handlers.Logging;

namespace XNode.Communication.DotNetty
{
    /// <summary>
    /// 服务端底层实现
    /// </summary>
    public class DotNettyServer : IServer
    {
        private ILoggerFactory loggerFactory;

        private ILogger logger;

        private IChannel channel;

        private IEventLoopGroup bossGroup, workerGroup;

        private IPAddress hostIPAddress;

        private ChannelHandlerContextManager channelHandlerContextManager = new ChannelHandlerContextManager();

        /// <summary>
        /// 服务地址
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// 接收登录请求事件
        /// </summary>
        public event RecieveLoginRequestDelegate OnRecieveLoginRequest;

        /// <summary>
        /// 接收服务请求事件
        /// </summary>
        public event RecieveServiceRequestDelegate OnRecieveServiceRequest;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host">服务端地址</param>
        /// <param name="port">服务端端口</param>
        public DotNettyServer(string host, int port)
        {
            loggerFactory = LoggerManager.ServerLoggerFactory;
            logger = loggerFactory.CreateLogger<DotNettyServer>();
            hostIPAddress = host.ToIPAddress().Result;
            Host = hostIPAddress.ToIPString();
            Port = port;
        }

        /// <summary>
        /// 绑定与启动
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            logger.LogDebug($"Server bind beginning. Host={Host}, Port={Port}");

            //配置服务端的NIO线程组
            bossGroup = new MultithreadEventLoopGroup(1);
            workerGroup = new MultithreadEventLoopGroup();

            var loginRecieveHandler = CreateRecieveLoginRequestHandler(OnRecieveLoginRequest);
            var serviceRecieveHandler = CreateRecieveServiceRequestHandler(OnRecieveServiceRequest);

            try
            {
                ServerBootstrap b = new ServerBootstrap();

                b.Group(bossGroup, workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 100)
                    .Option(ChannelOption.SoReuseaddr, true)        //启用端口复用以支持热更新
                    .Handler(new LoggingHandler(DotNettyLogging.LogLevel.INFO))
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        channel.Pipeline.AddLast("IdleStateHandler", new IdleStateHandler(0, 0, 30));        //自带心跳包方案，每隔指定时间检查是否有读和写操作，如果没有则触发userEventTriggered事件
                        channel.Pipeline.AddLast("MessageDecoder", new MessageDecoder(loggerFactory, 1024 * 1024, 4, 4, -8, 0));
                        channel.Pipeline.AddLast("MessageEncoder", new MessageEncoder(loggerFactory));
                        channel.Pipeline.AddLast("ServerLoginAuthHandler", new ServerLoginAuthHandler(loggerFactory, loginRecieveHandler, channelHandlerContextManager));
                        channel.Pipeline.AddLast("ServerServiceHandler", new ServerServiceHandler(loggerFactory, serviceRecieveHandler));
                        channel.Pipeline.AddLast("IdleStateHearBeatReqHandler", new IdleStateHearBeatReqHandler(loggerFactory));
                        channel.Pipeline.AddLast("IdleStateHearBeatRespHandler", new IdleStateHearBeatRespHandler(loggerFactory));
                        channel.Pipeline.AddLast("ServerExceptionHandler", new ServerExceptionHandler(loggerFactory));
                    }));

                
                channel = await b.BindAsync(new IPEndPoint(hostIPAddress, Port));
                logger.LogDebug($"Server bind finished. Host={Host}, Port={Port}");
            }
            catch (Exception ex)
            {
                var t = CloseAsync();
                logger.LogError(ex, $"Server bind has error. Host={Host}, Port={Port}, ExceptionMessage={ex.Message}");
                throw ex;
            }
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            try
            {
                logger.LogDebug($"Server close beginning. Host={Host}, port={Port}");
                //等待服务端监听端口关闭
                await channel.CloseAsync();
                await channelHandlerContextManager.CloseAllAsync();
                logger.LogDebug($"Server close finished. Host={Host}, port={Port}");
            }
            finally
            {
                //释放资源
                var t1 = workerGroup.ShutdownGracefullyAsync()
                    .ContinueWith(t => logger.LogDebug($"Server workerGroup shutdow. Host={Host}, port={Port}"));
                var t2 = bossGroup.ShutdownGracefullyAsync()
                    .ContinueWith(t => logger.LogDebug($"Server bossGroup shutdow. Host={Host}, port={Port}"));
            }
        }

        private Func<byte[], IDictionary<string, byte[]>, LoginState, Task<ResponseData>> CreateRecieveServiceRequestHandler(RecieveServiceRequestDelegate recieveServiceRequestDelegate)
        {
            if (recieveServiceRequestDelegate == null)
            {
                return null;
            }

            return new Func<byte[], IDictionary<string, byte[]>, LoginState, Task<ResponseData>>((message, attachments, loginState) =>
            {
                return recieveServiceRequestDelegate(message, attachments, loginState);
            });
        }

        private Func<LoginAuthInfo, Task<LoginResponseData>> CreateRecieveLoginRequestHandler(RecieveLoginRequestDelegate recieveLoginRequestDelegate)
        {
            if (recieveLoginRequestDelegate == null)
            {
                return null;
            }

            return new Func<LoginAuthInfo, Task<LoginResponseData>>((loginInfo) =>
            {
                return recieveLoginRequestDelegate(loginInfo);
            });
        }
    }
}
