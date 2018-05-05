// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Client
{
    /// <summary>
    /// 服务调用异常
    /// </summary>
    public class ServiceCallException : Exception
    {
        /// <summary>
        /// 服务Id
        /// </summary>
        public int ServiceId { get; private set; }

        /// <summary>
        /// ActionId
        /// </summary>
        public int ActionId { get; private set; }

        /// <summary>
        /// 异常编号
        /// </summary>
        public int ExceptionId { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceId">服务Id</param>
        /// <param name="actionId">ActionId</param>
        /// <param name="exceptionId">异常编号</param>
        /// <param name="message">异常信息</param>
        public ServiceCallException(int serviceId, int actionId, int exceptionId, string message) : base(message)
        {
            ServiceId = serviceId;
            ActionId = actionId;
            ExceptionId = exceptionId;
        }
    }
}
