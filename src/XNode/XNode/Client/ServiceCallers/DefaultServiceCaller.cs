// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Client.ServiceCallers
{
    /// <summary>
    /// 默认服务调用器
    /// </summary>
    public class DefaultServiceCaller : ServiceCallerBase
    {
        /// <summary>
        /// 调用服务
        /// </summary>
        /// <param name="nodeClientContainer">NodeClient容器</param>
        /// <param name="info">服务调用信息</param>
        /// <returns></returns>
        public override Task<ServiceCallResult> CallAsync(INodeClientContainer nodeClientContainer, ServiceCallInfo info)
        {
            var nodeClient = nodeClientContainer.Get(info.ServiceId, info.ActionId, info.ParamList, info.ReturnType, info.Attachments);

            if (nodeClient == null)
            {
                throw new InvalidOperationException("Not found connected NodeClient.");
            }

            return nodeClient.CallServiceAsync(info.ServiceId, info.ActionId, info.ParamList, info.ReturnType, info.Timeout, info.Attachments);
        }
    }

    public static class DefaultServiceCallerBuilderExtension
    {
        /// <summary>
        /// 使用DefaultServiceCaller
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ServiceCallerBuilder UseDefault(this ServiceCallerBuilder builder)
        {
            builder.Append(new DefaultServiceCaller());
            return builder;
        }
    }
}
