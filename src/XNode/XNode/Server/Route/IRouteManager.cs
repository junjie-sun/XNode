// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Server.Route
{
    /// <summary>
    /// 路由管理器接口
    /// </summary>
    public interface IRouteManager
    {
        /// <summary>
        /// 添加路由
        /// </summary>
        /// <param name="route"></param>
        void AddRoute(RouteDescription route);

        /// <summary>
        /// 获取指定路由
        /// </summary>
        /// <param name="serviceId">服务Id</param>
        /// <param name="actionId">ActionId</param>
        /// <returns></returns>
        RouteDescription GetRoute(int serviceId, int actionId);

        /// <summary>
        /// 获取所有路由
        /// </summary>
        /// <returns></returns>
        IList<RouteDescription> GetAllRoutes();
    }
}
