// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode
{
    public static class ExceptionMap
    {
        public static IDictionary<int, string> ServiceExceptions = new Dictionary<int, string>();

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

    public static class ServiceExceptionKeys
    {
        public const int SERVER_CONFIG_ERROR = -10001;
        public const int SERVICE_NOT_EXIST_ERROR = -10002;
        public const int SERVICE_DISABLED_ERROR = -10003;
        public const int SERVICE_INVOKE_ERROR = -10004;
        public const int SERVICE_NO_AUTHORIZE = -10005;
        public const int SERVICE_DATE_LIMIT = -10006;
        public const int SERVICE_TIME_LIMIT = -10007;
    }

    public static class SystemExceptionKeys
    {
        public const int SYSTEM_ERROR = -90001;
    }
}
