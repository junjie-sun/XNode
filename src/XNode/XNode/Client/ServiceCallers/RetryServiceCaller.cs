// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Communication;

namespace XNode.Client.ServiceCallers
{
    /// <summary>
    /// 当调用服务出现网络错误时进行重试
    /// </summary>
    public class RetryServiceCaller : ServiceCallerBase
    {
        /// <summary>
        /// 调用服务
        /// </summary>
        /// <param name="nodeClientContainer">NodeClient容器</param>
        /// <param name="info">服务调用信息</param>
        /// <returns></returns>
        public async override Task<ServiceCallResult> CallAsync(INodeClientContainer nodeClientContainer, ServiceCallInfo info)
        {
            if (Next == null)
            {
                return null;
            }

            int callCount = 0;
            NetworkException exception;
            do
            {
                try
                {
                    return await Next.CallAsync(nodeClientContainer, info);
                }
                catch (NetworkException ex)
                {
                    callCount++;
                    exception = ex;
                }
            } while (callCount < nodeClientContainer.Count);

            throw exception;
        }
    }

    public static class RetryServiceCallerBuilderExtensions
    {
        /// <summary>
        /// 使用RetryServiceCaller
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ServiceCallerBuilder UseRetry(this ServiceCallerBuilder builder)
        {
            builder.Append(new RetryServiceCaller());
            return builder;
        }
    }
}
