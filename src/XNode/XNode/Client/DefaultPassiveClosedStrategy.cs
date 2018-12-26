using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Communication;

namespace XNode.Client
{
    /// <summary>
    /// 默认客户端被动关闭处理策略
    /// </summary>
    public class DefaultPassiveClosedStrategy : IPassiveClosedStrategy
    {
        private ILogger logger;

        private DefaultPassiveClosedStrategyConfig config;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="loggerFactory">日志工厂</param>
        public DefaultPassiveClosedStrategy(DefaultPassiveClosedStrategyConfig config, ILoggerFactory loggerFactory)
        {
            this.config = config;
            logger = loggerFactory.CreateLogger<DefaultPassiveClosedStrategy>();
        }

        /// <summary>
        /// 客户端被动关闭处理
        /// </summary>
        /// <param name="client">客户端通信对象</param>
        /// <returns></returns>
        public async Task Handle(IClient client)
        {
            if (config.ReconnectCount == 0)
            {
                return;
            }

            int i = 0;

            while (config.ReconnectCount == -1 || i < config.ReconnectCount)
            {
                if (config.ReconnectCount != -1) { i++; }

                await Task.Delay(config.ReconnectInterval);

                if (client.Status != ClientStatus.PassiveClosed) { break; }

                try
                {
                    logger.LogDebug($"Client reconnect: connecting. Host={client.Host}, Port={client.Port}, LocalHost={client.LocalHost}, LocalPort={client.LocalPort}");
                    await client.ConnectAsync();
                    logger.LogDebug($"Client reconnect: connect success. Host={client.Host}, Port={client.Port}, LocalHost={client.LocalHost}, LocalPort={client.LocalPort}");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Client reconnect: connect error. Host={client.Host}, Port={client.Port}, LocalHost={client.LocalHost}, LocalPort={client.LocalPort}, ExceptionMessage={ex.Message}, ExceptionStackTrace={ex.StackTrace}");
                }
            }

            logger.LogDebug($"Client reconnect finished. Host={client.Host}, Port={client.Port}, LocalHost={client.LocalHost}, LocalPort={client.LocalPort}");
        }
    }

    /// <summary>
    /// 默认客户端被动关闭处理策略配置
    /// </summary>
    public class DefaultPassiveClosedStrategyConfig
    {
        /// <summary>
        /// 当发生网络错误时尝试重新连接的次数，-1表示无限，默认为-1
        /// </summary>
        public int ReconnectCount { get; set; } = -1;

        /// <summary>
        /// 每次尝试重新连接的时间间隔，单位：毫秒，默认为3000毫秒
        /// </summary>
        public int ReconnectInterval { get; set; } = 3000;
    }
}
