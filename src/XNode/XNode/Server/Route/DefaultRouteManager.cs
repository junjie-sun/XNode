// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using XNode.Logging;

namespace XNode.Server.Route
{
    /// <summary>
    /// 默认实现
    /// 支持同一个SerivceId设置在不同的类实例上
    /// 同一个Service下的Action编号必须唯一（即使ServiceId设置在不同的类实例上）
    /// </summary>
    public class DefaultRouteManager : IRouteManager
    {
        private ILogger logger = LoggerManager.ServerLoggerFactory.CreateLogger<DefaultRouteManager>();

        private IDictionary<int, IDictionary<int, RouteDescription>> routeList = new Dictionary<int, IDictionary<int, RouteDescription>>();

        private IList<RouteDescription> routeCache = new List<RouteDescription>();

        /// <summary>
        /// 添加路由
        /// </summary>
        /// <param name="route"></param>
        public void AddRoute(RouteDescription route)
        {
            if (!routeList.ContainsKey(route.ServiceId))
            {
                routeList.Add(route.ServiceId, new Dictionary<int, RouteDescription>());
            }

            var serviceRouteList = routeList[route.ServiceId];

            if (serviceRouteList.ContainsKey(route.ActionId))
            {
                throw new RouteRepeatException(route.ServiceId, route.ActionId);
            }

            serviceRouteList.Add(route.ActionId, route);
            routeCache.Add(route);
            logger.LogDebug($"Add route success. ServiceId={route.ServiceId}, ActionId={route.ActionId}");
        }

        /// <summary>
        /// 获取指定路由
        /// </summary>
        /// <param name="serviceId">服务Id</param>
        /// <param name="actionId">ActionId</param>
        /// <returns></returns>
        public RouteDescription GetRoute(int serviceId, int actionId)
        {
            if (!routeList.ContainsKey(serviceId) || !routeList[serviceId].ContainsKey(actionId))
            {
                throw new RouteNotFoundException(serviceId, actionId);
            }
            return routeList[serviceId][actionId];
        }

        /// <summary>
        /// 获取所有路由
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RouteDescription> GetAllRoutes()
        {
            return routeCache;
        }
    }
}
