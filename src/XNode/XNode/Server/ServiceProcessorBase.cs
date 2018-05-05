// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.ProtocolStack;
using XNode.Serializer;

namespace XNode.Server
{
    public abstract class ServiceProcessorBase : IServiceProcessor
    {
        /// <summary>
        /// 协议栈工厂
        /// </summary>
        public virtual IProtocolStackFactory ProtocolStackFactory { protected get; set; }

        /// <summary>
        /// 序列化器
        /// </summary>
        public virtual ISerializer Serializer { protected get; set; }

        /// <summary>
        /// 服务调用器
        /// </summary>
        public virtual IServiceInvoker ServiceInvoker { protected get; set; }

        /// <summary>
        /// 下一个服务处理器
        /// </summary>
        public virtual IServiceProcessor Next { get; set; }

        /// <summary>
        /// 对服务请求进行处理
        /// </summary>
        /// <param name="context">服务上下文，每次服务调用共享一个实例</param>
        /// <returns></returns>
        public abstract Task<ServiceProcessResult> ProcessAsync(ServiceContext context);
    }
}
