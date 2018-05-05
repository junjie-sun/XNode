// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.ProtocolStack
{
    /// <summary>
    /// 服务响应接口
    /// </summary>
    public interface IServiceResponse
    {
        /// <summary>
        /// 服务返回值
        /// </summary>
        byte[] ReturnValue { get; set; }

        /// <summary>
        /// 服务是否有异常
        /// </summary>
        bool HasException { get; set; }

        /// <summary>
        /// 服务异常Id
        /// </summary>
        int ExceptionId { get; set; }

        /// <summary>
        /// 服务异常信息
        /// </summary>
        string ExceptionMessage { get; set; }
    }
}
