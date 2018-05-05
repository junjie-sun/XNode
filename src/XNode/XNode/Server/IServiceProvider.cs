// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Server
{
    /// <summary>
    /// 服务提供器
    /// </summary>
    public interface IServiceProvider
    {
        /// <summary>
        /// 返回所有需要注册为XNode服务的实例类型，必须为可实例化的Class
        /// </summary>
        /// <returns></returns>
        IList<Type> GetNodeServiceTypes();

        /// <summary>
        /// 返回指定类型的XNode服务实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        object GetNodeServiceInstance(Type serviceType);
    }
}
