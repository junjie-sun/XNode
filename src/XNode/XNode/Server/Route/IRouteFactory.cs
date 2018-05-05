// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Server.Route
{
    /// <summary>
    /// 路由工厂
    /// </summary>
    public interface IRouteFactory
    {
        /// <summary>
        /// 创建路由描述器对象实例
        /// </summary>
        /// <returns></returns>
        IRouteDescriptor CreateRouteDescriptor();

        /// <summary>
        /// 创建路由管理器对象实例
        /// </summary>
        /// <returns></returns>
        IRouteManager CreateRouteManager();
    }
}
