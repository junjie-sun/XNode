// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Client
{
    /// <summary>
    /// 服务调用结果
    /// </summary>
    public class ServiceCallResult
    {
        /// <summary>
        /// 服务调用返回值
        /// </summary>
        public object ReturnVal { get; set; }

        /// <summary>
        /// 服务调用返回的附加数据
        /// </summary>
        public IDictionary<string, byte[]> Attachments { get; set; }
    }
}
