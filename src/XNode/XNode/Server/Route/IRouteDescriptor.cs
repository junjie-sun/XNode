// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using XNode.Server.Configuration;

namespace XNode.Server.Route
{
    /// <summary>
    /// 路由描述器接口
    /// </summary>
    public interface IRouteDescriptor
    {
        /// <summary>
        /// 创建路由描述信息
        /// </summary>
        /// <param name="serviceType">服务对象类型</param>
        /// <returns></returns>
        IList<RouteDescription> CreateRouteDescription(Type serviceType);

        /// <summary>
        /// 设置服务相关配置
        /// </summary>
        /// <param name="serviceConfigs"></param>
        void SetServiceConfig(IList<ServiceInfo> serviceConfigs);
    }
}
