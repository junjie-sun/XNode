// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Communication
{
    /// <summary>
    /// 请求响应结果
    /// </summary>
    public class RequestResult
    {
        /// <summary>
        /// 响应数据
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 响应附加数据
        /// </summary>
        public IDictionary<string, byte[]> Attachments { get; set; }
    }
}
