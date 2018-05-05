// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Client
{
    /// <summary>
    /// 服务调用器基类
    /// </summary>
    public abstract class ServiceCallerBase : IServiceCaller
    {
        /// <summary>
        /// 下一个服务调用器
        /// </summary>
        public virtual IServiceCaller Next { get; set; }

        /// <summary>
        /// 服务调用
        /// </summary>
        /// <param name="nodeClientContainer">NodeClient容器</param>
        /// <param name="info">服务调用信息</param>
        /// <returns></returns>
        public abstract Task<ServiceCallResult> CallAsync(INodeClientContainer nodeClientContainer, ServiceCallInfo info);
    }
}
