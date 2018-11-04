// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Microsoft.Extensions.Logging;
using System;
using XNode.Server;

namespace XNode.Autofac
{
    /// <summary>
    /// NodeServer构造器扩展方法
    /// </summary>
    public static class NodeServerBuilderExtensions
    {
        /// <summary>
        /// 加载Autofac
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="container">Autofac容器</param>
        /// <returns></returns>
        public static INodeServerBuilder UseAutofac(this INodeServerBuilder builder, IContainer container)
        {
            return builder.ConfigServiceProvider(new AutofacServiceProvider(container));
        }
    }
}
