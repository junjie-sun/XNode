// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Logging
{
    /// <summary>
    /// 日志管理器
    /// </summary>
    public static class LoggerManager
    {
        /// <summary>
        /// 客户端日志工厂
        /// </summary>
        public static ILoggerFactory ClientLoggerFactory { get; set; }

        /// <summary>
        /// 服务器日志工厂
        /// </summary>
        public static ILoggerFactory ServerLoggerFactory { get; set; }

        static LoggerManager()
        {
            ClientLoggerFactory = new LoggerFactory();
            ServerLoggerFactory = new LoggerFactory();
        }
    }
}
