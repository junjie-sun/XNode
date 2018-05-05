// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.ProtocolStack
{
    /// <summary>
    /// 协议栈工厂接口
    /// </summary>
    public interface IProtocolStackFactory
    {
        /// <summary>
        /// 创建服务请求对象实例
        /// </summary>
        /// <returns></returns>
        IServiceRequest CreateServiceRequest();

        /// <summary>
        /// 创建服务响应对象实例
        /// </summary>
        /// <returns></returns>
        IServiceResponse CreateServiceResponse();

        /// <summary>
        /// 服务请求对象类型
        /// </summary>
        Type ServiceRequestType { get; }

        /// <summary>
        /// 服务响应对象类型
        /// </summary>
        Type ServiceResponseType { get; }
    }
}
