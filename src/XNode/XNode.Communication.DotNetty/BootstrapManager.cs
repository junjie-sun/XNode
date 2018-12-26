using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using XNode.Communication.DotNetty.Handlers;
using XNode.Logging;

namespace XNode.Communication.DotNetty
{
    /// <summary>
    /// ClientBootstrap管理器
    /// </summary>
    public static class BootstrapManager
    {
        private static ILogger logger;

        private static Bootstrap bootstrap;

        private static IEventLoopGroup group;

        private static ConcurrentDictionary<string, DotNettyClientInfo> clientInfoList = new ConcurrentDictionary<string, DotNettyClientInfo>();

        /// <summary>
        /// 初始化ClientBootstrap
        /// </summary>
        public static void Init()
        {
            logger = LoggerManager.ClientLoggerFactory.CreateLogger("BootstrapManager");
            bootstrap = new Bootstrap();
            group = new MultithreadEventLoopGroup();

            var getLoginRequestDataHandler = CreateGetLoginRequestDataHandler();
            var loginResponseHandler = CreateLoginResponseHandler();
            var serviceResponseHandler = CreateServiceResponseHandler();
            var inactiveHandler = CreateInactiveHandler();

            bootstrap.Group(group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    channel.Pipeline.AddLast("IdleStateHandler", new IdleStateHandler(0, 0, 10));        //自带心跳包方案，每隔指定时间检查是否有读和写操作，如果没有则触发userEventTriggered事件
                    channel.Pipeline.AddLast("MessageDecoder", new MessageDecoder(LoggerManager.ClientLoggerFactory, 1024 * 1024, 4, 4, -8, 0));
                    channel.Pipeline.AddLast("MessageEncoder", new MessageEncoder(LoggerManager.ClientLoggerFactory));
                    channel.Pipeline.AddLast("LoginAuthHandler", new ClientLoginAuthHandler(LoggerManager.ClientLoggerFactory, getLoginRequestDataHandler, loginResponseHandler));
                    channel.Pipeline.AddLast("ServiceResultHandler", new ClientServiceHandler(LoggerManager.ClientLoggerFactory, serviceResponseHandler));
                    channel.Pipeline.AddLast("IdleStateHearBeatReqHandler", new IdleStateHearBeatReqHandler(LoggerManager.ClientLoggerFactory));
                    channel.Pipeline.AddLast("InactiveHandler", new ClientExceptionHandler(LoggerManager.ClientLoggerFactory, inactiveHandler));
                }));
        }

        /// <summary>
        /// 连接服务端
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public async static Task<IChannel> ConnectAsync(DotNettyClientInfo info)
        {
            var clientInfoAddResult = clientInfoList.TryAdd(info.ChannelName, info);

            if (!clientInfoAddResult)
            {
                logger.LogError($"Channel is exist. Host={info.Host}, Port={info.Port}, LocalHost={info.LocalHost}, LocalPort={info.LocalPort}");
                throw new InvalidOperationException("Channel is exist.");
            }

            try
            {
                IChannel channel;

                if (!string.IsNullOrEmpty(info.LocalHost) && info.LocalPort != null)
                {
                    channel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(info.Host), info.Port), new IPEndPoint(IPAddress.Parse(info.LocalHost), info.LocalPort.Value));
                }
                else
                {
                    channel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(info.Host), info.Port));
                }

                info.Channel = channel;
                return channel;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }
                logger.LogError(ex, $"Channel connect failed. Host={info.Host}, Port={info.Port}, LocalHost={info.LocalHost}, LocalPort={info.LocalPort}");
                clientInfoList.TryRemove(info.ChannelName, out info);
                throw ex;
            }
        }

        /// <summary>
        /// 关闭与服务端的连接
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public static Task CloseAsync(string channelName)
        {
            if (clientInfoList.TryRemove(channelName, out DotNettyClientInfo info))
            {
                return info.Channel.CloseAsync();
            }
            else
            {
                return Task.FromException(new InvalidOperationException($"Channel is not exist. Host={info.Host}, Port={info.Port}, LocalHost={info.LocalHost}, LocalPort={info.LocalPort}"));
            }
        }

        /// <summary>
        /// 释放Client事件循环线程
        /// </summary>
        /// <returns></returns>
        public async static Task Disable()
        {
            List<Task> tasks = new List<Task>();
            var channelNameList = clientInfoList.Keys.ToArray();
            foreach (var channelName in channelNameList)
            {
                tasks.Add(CloseAsync(channelName));
            }
            await Task.WhenAll(tasks.ToArray());
            await group.ShutdownGracefullyAsync();
            logger.LogDebug($"Group shutdowned.");
        }

        private static Func<string, Task<LoginRequestData>> CreateGetLoginRequestDataHandler()
        {
            return new Func<string, Task<LoginRequestData>>(channelName =>
            {
                clientInfoList.TryGetValue(channelName, out DotNettyClientInfo result);
                if (result != null)
                {
                    return result.GetLoginRequestDataHandler();
                }
                return Task.FromResult(new LoginRequestData());
            });
        }

        private static Func<string, byte[], IDictionary<string, byte[]>, Task<byte>> CreateLoginResponseHandler()
        {
            return new Func<string, byte[], IDictionary<string, byte[]>, Task<byte>>((channelName, message, attachments) =>
            {
                clientInfoList.TryGetValue(channelName, out DotNettyClientInfo result);
                if (result != null)
                {
                    return result.LoginResponseHandler(message, attachments);
                }
                return Task.FromResult(message[0]);
            });
        }

        private static Func<string, RequestManager> CreateServiceResponseHandler()
        {
            return new Func<string, RequestManager>(channelName =>
            {
                clientInfoList.TryGetValue(channelName, out DotNettyClientInfo result);
                return result != null ? result.RequestManager : null;
            });
        }

        private static Func<string, Task> CreateInactiveHandler()
        {
            return new Func<string, Task>(channelName =>
            {
                clientInfoList.TryGetValue(channelName, out DotNettyClientInfo result);
                return result != null ? result.InactiveHandler() : Task.CompletedTask;
            });
        }
    }

    /// <summary>
    /// DotNettyClient必要信息
    /// </summary>
    public class DotNettyClientInfo
    {
        /// <summary>
        /// 远程Host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 远程端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 本地Host
        /// </summary>
        public string LocalHost { get; set; }

        /// <summary>
        /// 本地端口
        /// </summary>
        public int? LocalPort { get; set; }

        /// <summary>
        /// Request管理器
        /// </summary>
        public RequestManager RequestManager { get; set; }

        /// <summary>
        /// 连接断开Handler
        /// </summary>
        public Func<Task> InactiveHandler { get; set; }

        /// <summary>
        /// 登录响应Handler，对服务端返回的登录验证信息进行解析并返回登录验证结果
        /// </summary>
        public Func<byte[], IDictionary<string, byte[]>, Task<byte>> LoginResponseHandler { get; set; }

        /// <summary>
        /// 获取登录请求数据Handler，从此Handler中可以获取用于发送为服务端进行登录请求的数据
        /// </summary>
        public Func<Task<LoginRequestData>> GetLoginRequestDataHandler { get; set; }

        /// <summary>
        /// Client底层通信组件
        /// </summary>
        public IChannel Channel { get; set; }

        /// <summary>
        /// Channel名称，格式：Host:Port
        /// </summary>
        public string ChannelName
        {
            get
            {
                return $"{Host}:{Port}";
            }
        }
    }
}
