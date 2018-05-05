// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Client
{
    /// <summary>
    /// ServiceCaller构造器
    /// </summary>
    public class ServiceCallerBuilder
    {
        private IServiceCaller serviceCaller;

        private IServiceCaller lastServiceCaller;

        /// <summary>
        /// 添加ServiceCaller
        /// </summary>
        /// <param name="serviceCaller">ServiceCaller实例</param>
        /// <returns></returns>
        public ServiceCallerBuilder Append(IServiceCaller serviceCaller)
        {
            if (serviceCaller == null)
            {
                return this;
            }

            if (this.serviceCaller == null)
            {
                this.serviceCaller = lastServiceCaller = serviceCaller;
            }
            else
            {
                lastServiceCaller.Next = serviceCaller;
                lastServiceCaller = serviceCaller;
            }

            return this;
        }

        /// <summary>
        /// 构造ServiceCaller
        /// </summary>
        /// <returns></returns>
        public IServiceCaller Build()
        {
            return serviceCaller;
        }
    }
}
