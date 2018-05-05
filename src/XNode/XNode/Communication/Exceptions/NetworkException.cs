// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Communication
{
    public class NetworkException : Exception
    {
        public string Host { get; }

        public int Port { get; }

        public NetworkException(string host, int port, string message) : base(message)
        {
            Host = host;
            Port = port;
        }
    }
}
