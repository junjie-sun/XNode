// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using XNode.ProtocolStack;

namespace XNode.Server
{
    /// <summary>
    /// 服务处理器相关工具类
    /// </summary>
    public static class ServiceProcessorUtils
    {
        /// <summary>
        /// 创建异常ServiceProcessResult
        /// </summary>
        /// <param name="exceptionId">异常Id</param>
        /// <param name="protocolStackFactory">协议栈工厂</param>
        /// <returns></returns>
        public static ServiceProcessResult CreateServiceExceptionResult(int exceptionId, IProtocolStackFactory protocolStackFactory)
        {
            return CreateExceptionResult(exceptionId, ExceptionMap.ServiceExceptions[exceptionId], protocolStackFactory);
        }

        /// <summary>
        /// 创建异常ServiceProcessResult
        /// </summary>
        /// <param name="exceptionId">异常Id</param>
        /// <param name="protocolStackFactory">协议栈工厂</param>
        /// <returns></returns>
        public static ServiceProcessResult CreateSystemExceptionResult(int exceptionId, IProtocolStackFactory protocolStackFactory)
        {
            return CreateExceptionResult(exceptionId, ExceptionMap.SystemExceptions[exceptionId], protocolStackFactory);
        }

        /// <summary>
        /// 创建异常ServiceProcessResult
        /// </summary>
        /// <param name="exceptionId">异常Id</param>
        /// <param name="exceptionMessage">异常信息</param>
        /// <param name="protocolStackFactory">协议栈工厂</param>
        /// <returns></returns>
        public static ServiceProcessResult CreateExceptionResult(int exceptionId, string exceptionMessage, IProtocolStackFactory protocolStackFactory)
        {
            var response = protocolStackFactory.CreateServiceResponse();
            var result = new ServiceProcessResult()
            {
                ServiceResponse = response
            };
            response.HasException = true;
            response.ExceptionId = exceptionId;
            response.ExceptionMessage = exceptionMessage;
            return result;
        }
    }
}
