// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using XNode.Security;

namespace XNode.Server.Configuration
{
    public static class ConfigurationExtensions
    {
        public static ServerConfig GetServerConfig(this IConfigurationRoot config)
        {
            var serverConfig = new ServerConfig();
            config.GetSection("xnode:server")
                .Bind(serverConfig);
            return serverConfig;
        }

        public static DefaultLoginValidatorConfig GetDefaultLoginValidatorConfig(this IConfigurationRoot config)
        {
            var loginValidatorConfig = new DefaultLoginValidatorConfig();
            config.GetSection("xnode:server:security:loginValidator")
                .Bind(loginValidatorConfig);
            return loginValidatorConfig;
        }

        public static IList<DefaultServiceAuthorizeConfig> GetDefaultServiceAuthorizeConfig(this IConfigurationRoot config)
        {
            var list = new List<DefaultServiceAuthorizeConfig>();
            config.GetSection("xnode:server:services")
                .Bind(list);
            return list;
        }
    }
}
