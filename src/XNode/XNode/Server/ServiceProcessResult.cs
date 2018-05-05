// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using XNode.ProtocolStack;

namespace XNode.Server
{
    /// <summary>
    /// 服务处理结果
    /// </summary>
    public class ServiceProcessResult
    {
        /// <summary>
        /// 服务响应对象
        /// </summary>
        public IServiceResponse ServiceResponse { get; set; }

        /// <summary>
        /// 服务处理附加数据
        /// </summary>
        public IDictionary<string, byte[]> Attachments { get; set; }
    }
}
