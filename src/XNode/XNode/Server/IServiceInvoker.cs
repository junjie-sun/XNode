// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Serializer;
using XNode.Server.Route;

namespace XNode.Server
{
    /// <summary>
    /// 服务调用器接口
    /// </summary>
    public interface IServiceInvoker
    {
        /// <summary>
        /// 服务提供器
        /// </summary>
        IServiceProvider ServiceProvider { set; }

        /// <summary>
        /// 调用服务
        /// </summary>
        /// <param name="serializer">序列化器</param>
        /// <param name="route">路由信息</param>
        /// <param name="paramList">服务参数列表</param>
        /// <returns></returns>
        Task<byte[]> Invoke(ISerializer serializer, RouteDescription route, IList<byte[]> paramList);
    }
}
