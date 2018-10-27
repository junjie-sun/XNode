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
    public static class BootstrapManager
    {
        private static ILogger logger;

        private static Bootstrap bootstrap;

        private static IEventLoopGroup group;

        private static ConcurrentDictionary<string, DotNettyClientInfo> clientInfoList = new ConcurrentDictionary<string, DotNettyClientInfo>();

        public static void Init()
        {
            logger = LoggerManager.ClientLoggerFactory.CreateLogger("BootstrapManager");
            bootstrap = new Bootstrap();
            group = new MultithreadEventLoopGroup();

            var getLoginRequestDataHandler = CreateGetLoginRequestDataHandler();
            var loginResponseHandler = CreateLoginResponseHandler();
            var serviceResponseHandler = CreateServiceResponseHandler();
            var exceptionHandler = CreateExceptionHandler();

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
                    channel.Pipeline.AddLast("ExceptionHandler", new ClientExceptionHandler(LoggerManager.ClientLoggerFactory, exceptionHandler));
                }));
        }

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

        private static Func<string, Task> CreateExceptionHandler()
        {
            return new Func<string, Task>(channelName =>
            {
                clientInfoList.TryGetValue(channelName, out DotNettyClientInfo result);
                return result != null ? result.ExceptionHandler() : Task.CompletedTask;
            });
        }
    }

    public class DotNettyClientInfo
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string LocalHost { get; set; }

        public int? LocalPort { get; set; }

        public RequestManager RequestManager { get; set; }

        public Func<Task> ExceptionHandler { get; set; }

        public Func<byte[], IDictionary<string, byte[]>, Task<byte>> LoginResponseHandler { get; set; }

        public Func<Task<LoginRequestData>> GetLoginRequestDataHandler { get; set; }

        public IChannel Channel { get; set; }

        public string ChannelName
        {
            get
            {
                return $"{Host}:{Port}";
            }
        }
    }
}
