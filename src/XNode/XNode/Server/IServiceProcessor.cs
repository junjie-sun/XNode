// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Serializer;
using XNode.ProtocolStack;
using XNode.Server.Route;
using Microsoft.Extensions.Logging;

namespace XNode.Server
{
    /// <summary>
    /// 服务处理器接口
    /// </summary>
    public interface IServiceProcessor
    {
        /// <summary>
        /// 协议栈工厂
        /// </summary>
        IProtocolStackFactory ProtocolStackFactory { set; }

        /// <summary>
        /// 序列化器
        /// </summary>
        ISerializer Serializer { set; }

        /// <summary>
        /// 服务调用器
        /// </summary>
        IServiceInvoker ServiceInvoker { set; }

        /// <summary>
        /// 下一个服务处理器
        /// </summary>
        IServiceProcessor Next { get; set; }

        /// <summary>
        /// 对服务请求进行处理
        /// </summary>
        /// <param name="context">服务上下文，每次服务调用共享一个实例</param>
        /// <returns></returns>
        Task<ServiceProcessResult> ProcessAsync(ServiceContext context);
    }
}
