// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace XNode.Server
{
    /// <summary>
    /// ServiceProxy扩展方法
    /// </summary>
    public static class NodeServerExtensions
    {
        /// <summary>
        /// 将所有服务设置为启用，可在运行时动态设置服务
        /// </summary>
        /// <param name="nodeServer"></param>
        /// <returns></returns>
        public static INodeServer EnableAll(this INodeServer nodeServer)
        {
            var routeList = nodeServer.RouteManager.GetAllRoutes();
            foreach (var route in routeList)
            {
                route.Enabled = true;
            }
            return nodeServer;
        }

        /// <summary>
        /// 将所有服务设置为禁用，可在运行时动态设置服务
        /// </summary>
        /// <param name="nodeServer"></param>
        /// <returns></returns>
        public static INodeServer DisableAll(this INodeServer nodeServer)
        {
            var routeList = nodeServer.RouteManager.GetAllRoutes();
            foreach (var route in routeList)
            {
                route.Enabled = false;
            }
            return nodeServer;
        }

        /// <summary>
        /// 将指定服务设置为启用
        /// </summary>
        /// <param name="nodeServer"></param>
        /// <param name="serviceId"></param>
        /// <param name="actionId"></param>
        /// <returns></returns>
        public static INodeServer Enable(this INodeServer nodeServer, int serviceId, int? actionId = null)
        {
            if (actionId != null)
            {
                var route = nodeServer.RouteManager.GetRoute(serviceId, actionId.Value);
                if (route != null)
                {
                    route.Enabled = true;
                }
            }
            else
            {
                var routeList = nodeServer.RouteManager.GetAllRoutes().Where(r => r.ServiceId == serviceId);
                foreach (var route in routeList)
                {
                    route.Enabled = true;
                }
            }
            return nodeServer;
        }

        /// <summary>
        /// 将指定服务设置为禁用
        /// </summary>
        /// <param name="nodeServer"></param>
        /// <param name="serviceId"></param>
        /// <param name="actionId"></param>
        /// <returns></returns>
        public static INodeServer Disable(this INodeServer nodeServer, int serviceId, int? actionId = null)
        {
            if (actionId != null)
            {
                var route = nodeServer.RouteManager.GetRoute(serviceId, actionId.Value);
                if (route != null)
                {
                    route.Enabled = false;
                }
            }
            else
            {
                var routeList = nodeServer.RouteManager.GetAllRoutes().Where(r => r.ServiceId == serviceId);
                foreach (var route in routeList)
                {
                    route.Enabled = false;
                }
            }
            return nodeServer;
        }
    }
}
