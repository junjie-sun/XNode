// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using XNode.Client;

namespace XNode.ServiceDiscovery.Zookeeper
{
    internal static class Utils
    {
        public static string GetHostName(string host, int port)
        {
            return $"{host}:{port}";
        }

        public static ServiceProxyAttribute GetServiceProxyAttribute(Type serviceProxyType)
        {
            var typeInfo = serviceProxyType.GetTypeInfo();
            var serviceProxyAttr = typeInfo.GetCustomAttribute<ServiceProxyAttribute>();

            if (serviceProxyAttr == null)
            {
                throw new InvalidOperationException($"ServiceProxyType has not set ServiceProxyAttribute. Type={serviceProxyType.FullName}");
            }

            return serviceProxyAttr;
        }
    }
}
