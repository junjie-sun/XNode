// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode
{
    /// <summary>
    /// 异常工具类
    /// </summary>
    public static class ExceptionMap
    {
        /// <summary>
        /// 服务异常
        /// </summary>
        public static IDictionary<int, string> ServiceExceptions = new Dictionary<int, string>();

        /// <summary>
        /// 系统异常
        /// </summary>
        public static IDictionary<int, string> SystemExceptions = new Dictionary<int, string>();

        static ExceptionMap()
        {
            BuildServiceExceptions();

            BuildSystemExceptions();
        }

        private static void BuildServiceExceptions()
        {
            ServiceExceptions.Add(ServiceExceptionKeys.SERVER_CONFIG_ERROR, "Server config error.");
            ServiceExceptions.Add(ServiceExceptionKeys.SERVICE_NOT_EXIST_ERROR, "Service not exist.");
            ServiceExceptions.Add(ServiceExceptionKeys.SERVICE_DISABLED_ERROR, "Service is disabled.");
            ServiceExceptions.Add(ServiceExceptionKeys.SERVICE_INVOKE_ERROR, "Service invoke error.");
            ServiceExceptions.Add(ServiceExceptionKeys.SERVICE_NO_AUTHORIZE, "Service no authorize.");
            ServiceExceptions.Add(ServiceExceptionKeys.SERVICE_DATE_LIMIT, "Service date limit.");
            ServiceExceptions.Add(ServiceExceptionKeys.SERVICE_TIME_LIMIT, "Service time limit.");
        }

        private static void BuildSystemExceptions()
        {
            ServiceExceptions.Add(SystemExceptionKeys.SYSTEM_ERROR, "系统错误");
        }
    }

    /// <summary>
    /// 服务异常Key常量
    /// </summary>
    public static class ServiceExceptionKeys
    {
        /// <summary>
        /// 服务器配置错误
        /// </summary>
        public const int SERVER_CONFIG_ERROR = -10001;
        /// <summary>
        /// 服务不存在错误
        /// </summary>
        public const int SERVICE_NOT_EXIST_ERROR = -10002;
        /// <summary>
        /// 服务禁用错误
        /// </summary>
        public const int SERVICE_DISABLED_ERROR = -10003;
        /// <summary>
        /// 服务调用错误
        /// </summary>
        public const int SERVICE_INVOKE_ERROR = -10004;
        /// <summary>
        /// 服务未授权错误
        /// </summary>
        public const int SERVICE_NO_AUTHORIZE = -10005;
        /// <summary>
        /// 服务日期限制错误
        /// </summary>
        public const int SERVICE_DATE_LIMIT = -10006;
        /// <summary>
        /// 服务时间限制错误
        /// </summary>
        public const int SERVICE_TIME_LIMIT = -10007;
    }

    /// <summary>
    /// 系统异常Key常量
    /// </summary>
    public static class SystemExceptionKeys
    {
        /// <summary>
        /// 系统错误
        /// </summary>
        public const int SYSTEM_ERROR = -90001;
    }
}
