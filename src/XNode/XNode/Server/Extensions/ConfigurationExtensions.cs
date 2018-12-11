// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using XNode.Security;

namespace XNode.Server.Configuration
{
    /// <summary>
    /// Server配置扩展方法
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// 获取Server服务配置
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static ServerConfig GetServerConfig(this IConfiguration config)
        {
            var serverConfig = new ServerConfig();
            config.GetSection("xnode:server")
                .Bind(serverConfig);
            return serverConfig;
        }

        /// <summary>
        /// 获取默认登录验证器配置
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static DefaultLoginValidatorConfig GetDefaultLoginValidatorConfig(this IConfiguration config)
        {
            var loginValidatorConfig = new DefaultLoginValidatorConfig();
            config.GetSection("xnode:server:security:loginValidator")
                .Bind(loginValidatorConfig);
            return loginValidatorConfig;
        }

        /// <summary>
        /// 获取默认服务授权配置
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IList<DefaultServiceAuthorizeConfig> GetDefaultServiceAuthorizeConfig(this IConfiguration config)
        {
            var list = new List<DefaultServiceAuthorizeConfig>();
            config.GetSection("xnode:server:services")
                .Bind(list);
            return list;
        }
    }
}
