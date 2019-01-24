// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.ServiceDiscovery.Zookeeper
{
    internal static class Utils
    {
        public static string GetHostName(string host, int port)
        {
            return $"{host}:{port}";
        }
    }
}
