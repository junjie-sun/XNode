// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Security
{
    /// <summary>
    /// 服务授权异常
    /// </summary>
    public class ServiceAuthorizeException : Exception
    {
        /// <summary>
        /// 服务授权异常类型
        /// </summary>
        public ServiceAuthorizeExceptionType ServiceAuthorizeExceptionType { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type">服务授权异常类型</param>
        /// <param name="message">异常描述</param>
        public ServiceAuthorizeException(ServiceAuthorizeExceptionType type, string message) : base(message)
        {
            ServiceAuthorizeExceptionType = type;
        }
    }

    /// <summary>
    /// 服务授权异常类型
    /// </summary>
    public enum ServiceAuthorizeExceptionType
    {
        /// <summary>
        /// 未授权
        /// </summary>
        NoAuthorize,

        /// <summary>
        /// 日期限制
        /// </summary>
        DateLimit,

        /// <summary>
        /// 时间限制
        /// </summary>
        TimeLimit
    }
}
