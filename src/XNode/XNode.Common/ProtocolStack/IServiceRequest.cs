// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.ProtocolStack
{
    /// <summary>
    /// 服务请求接口
    /// </summary>
    public interface IServiceRequest
    {
        /// <summary>
        /// 服务Id
        /// </summary>
        int ServiceId { get; set; }

        /// <summary>
        /// ActionId
        /// </summary>
        int ActionId { get; set; }

        /// <summary>
        /// Action参数列表
        /// </summary>
        List<byte[]> ParamList { get; set; }
    }
}
