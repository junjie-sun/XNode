// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System.Linq;
using XNode.Security;

namespace XNode.Client.Configuration
{
    /// <summary>
    /// Client配置扩展方法类
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// 获取Client配置
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static ClientConfig GetClientConfig(this IConfiguration config)
        {
            var clientConfig = new ClientConfig();
            config.GetSection("xnode:client")
                .Bind(clientConfig);
            return clientConfig;
        }

        /// <summary>
        /// 获取默认LoginHandler配置
        /// </summary>
        /// <param name="config"></param>
        /// <param name="proxyName"></param>
        /// <returns></returns>
        public static DefaultLoginHandlerConfig GetDefaultLoginHandlerConfig(this IConfiguration config, string proxyName)
        {
            var loginConfig = new DefaultLoginHandlerConfig();
            config
                .GetSection("xnode:client:serviceProxies")
                .GetChildren().Where(o => o.GetValue<string>("proxyName") == proxyName)
                .FirstOrDefault()
                .GetSection("security:login")
                .Bind(loginConfig);
            return loginConfig;
        }

        /// <summary>
        /// 获取默认客户端被动关闭处理策略配置
        /// </summary>
        /// <param name="config"></param>
        /// <param name="proxyName"></param>
        /// <returns></returns>
        public static DefaultPassiveClosedStrategyConfig GetDefaultPassiveClosedStrategyConfig(this IConfiguration config, string proxyName)
        {
            var strategyConfig = new DefaultPassiveClosedStrategyConfig();
            config
                .GetSection("xnode:client:serviceProxies")
                .GetChildren().Where(o => o.GetValue<string>("proxyName") == proxyName)
                .FirstOrDefault()
                .GetSection("passiveClosedStrategy")
                .Bind(strategyConfig);
            return strategyConfig;
        }
    }
}
