// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace XNode.ProtocolStack
{
    /// <summary>
    /// 服务响应
    /// </summary>
    [DataContract]
    public class ServiceResponse : IServiceResponse
    {
        /// <summary>
        /// 服务返回值
        /// </summary>
        [DataMember(Order = 1)]
        public byte[] ReturnValue { get; set; }

        /// <summary>
        /// 服务是否有异常
        /// </summary>
        [DataMember(Order = 2)]
        public bool HasException { get; set; }

        /// <summary>
        /// 服务异常Id
        /// </summary>
        [DataMember(Order = 3)]
        public int ExceptionId { get; set; }

        /// <summary>
        /// 服务异常信息
        /// </summary>
        [DataMember(Order = 4)]
        public string ExceptionMessage { get; set; }
    }
}
