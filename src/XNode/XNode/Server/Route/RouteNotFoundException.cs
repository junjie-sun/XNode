// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Server.Route
{
    /// <summary>
    /// 服务路由未找到异常类
    /// </summary>
    public class RouteNotFoundException : Exception
    {
        /// <summary>
        /// 服务Id
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// ActionId
        /// </summary>
        public int ActionId { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceId">服务Id</param>
        /// <param name="actionId">ActionId</param>
        public RouteNotFoundException(int serviceId, int actionId)
            : base($"Route not found. ServiceId={serviceId}, ActionId={actionId}")
        {
            ServiceId = serviceId;
            ActionId = actionId;
        }
    }
}
