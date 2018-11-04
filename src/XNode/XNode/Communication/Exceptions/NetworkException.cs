// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Communication
{
    /// <summary>
    /// 网络异常
    /// </summary>
    public class NetworkException : Exception
    {
        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host">host</param>
        /// <param name="port">port</param>
        /// <param name="message">异常信息</param>
        public NetworkException(string host, int port, string message) : base(message)
        {
            Host = host;
            Port = port;
        }
    }
}
