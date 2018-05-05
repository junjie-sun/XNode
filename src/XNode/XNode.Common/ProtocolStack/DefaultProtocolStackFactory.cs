// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.ProtocolStack
{
    /// <summary>
    /// 默认协议栈工厂
    /// </summary>
    public class DefaultProtocolStackFactory : IProtocolStackFactory
    {
        /// <summary>
        /// 服务请求对象类型
        /// </summary>
        public Type ServiceRequestType
        {
            get
            {
                return typeof(ServiceRequest);
            }
        }

        /// <summary>
        /// 服务响应对象类型
        /// </summary>
        public Type ServiceResponseType
        {
            get
            {
                return typeof(ServiceResponse);
            }
        }

        /// <summary>
        /// 创建服务请求对象实例
        /// </summary>
        /// <returns></returns>
        public virtual IServiceRequest CreateServiceRequest()
        {
            return new ServiceRequest();
        }

        /// <summary>
        /// 创建服务响应对象实例
        /// </summary>
        /// <returns></returns>
        public virtual IServiceResponse CreateServiceResponse()
        {
            return new ServiceResponse();
        }
    }
}
