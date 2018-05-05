// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System.Linq;
using XNode.Security;

namespace XNode.Client.Configuration
{
    public static class ConfigurationExtensions
    {
        public static ClientConfig GetClientConfig(this IConfigurationRoot config)
        {
            var clientConfig = new ClientConfig();
            config.GetSection("xnode:client")
                .Bind(clientConfig);
            return clientConfig;
        }

        public static DefaultLoginHandlerConfig GetDefaultLoginHandlerConfig(this IConfigurationRoot config, string proxyName)
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
    }
}
