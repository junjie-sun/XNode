// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace XNode.ProtocolStack
{
    /// <summary>
    /// 服务请求
    /// </summary>
    [DataContract]
    public class ServiceRequest : IServiceRequest
    {
        /// <summary>
        /// 服务Id
        /// </summary>
        [DataMember(Order = 1)]
        public int ServiceId { get; set; }

        /// <summary>
        /// ActionId
        /// </summary>
        [DataMember(Order = 2)]
        public int ActionId { get; set; }

        /// <summary>
        /// Action参数列表
        /// </summary>
        [DataMember(Order = 3)]
        public List<byte[]> ParamList { get; set; }
    }
}
