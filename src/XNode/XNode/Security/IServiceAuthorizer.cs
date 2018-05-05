// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Server;

namespace XNode.Security
{
    /// <summary>
    /// 服务授权验证接口
    /// </summary>
    public interface IServiceAuthorizer
    {
        /// <summary>
        /// 授权验证
        /// </summary>
        /// <param name="context">服务上下文</param>
        /// <param name="serviceId">服务Id</param>
        /// <param name="actionId">ActionId</param>
        /// <param name="attachments">附加数据</param>
        Task Validate(ServiceContext context, int serviceId, int actionId, IDictionary<string, byte[]> attachments);
    }
}
