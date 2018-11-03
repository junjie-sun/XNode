// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace XNode.Server.Route
{
    /// <summary>
    /// 路由描述对象
    /// </summary>
    public class RouteDescription
    {
        /// <summary>
        /// 服务Id
        /// </summary>
        public int ServiceId { get; internal protected set; }

        /// <summary>
        /// ActionId
        /// </summary>
        public int ActionId { get; internal protected set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; internal protected set; }

        /// <summary>
        /// Action名称
        /// </summary>
        public string ActionName { get; internal protected set; }

        /// <summary>
        /// 服务对象类型
        /// </summary>
        public Type ServiceType { get; internal protected set; }

        /// <summary>
        /// Action对象类型
        /// </summary>
        public MethodInfo ActionType { get; internal protected set; }

        /// <summary>
        /// Action是否启用
        /// </summary>
        public bool Enabled { get; internal protected set; }
    }
}
