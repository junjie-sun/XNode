// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Server
{
    /// <summary>
    /// 服务执行异常
    /// </summary>
    public class ServiceInvokeException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">异常信息</param>
        public ServiceInvokeException(string message) : base(message) { }
    }
}
