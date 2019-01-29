// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using XNode.Client;
using XNode.Server;

namespace XNode.Autofac
{
    /// <summary>
    /// Container扩展类
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        /// 获取XNode服务类型
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IList<Type> GetNodeServiceTypes(this IContainer container)
        {
            var list = new List<Type>();

            foreach (var reg in container.ComponentRegistry.Registrations)
            {
                var e = reg.Services.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current is TypedService t && t.ServiceType.IsNodeServiceType())
                    {
                        list.Add(t.ServiceType);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 获取XNode服务代理类型
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IList<Type> GetNodeServiceProxyTypes(this IContainer container)
        {
            var list = new List<Type>();

            foreach (var reg in container.ComponentRegistry.Registrations)
            {
                var e = reg.Services.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current is TypedService t && t.ServiceType.IsNodeServiceProxyType())
                    {
                        list.Add(t.ServiceType);
                    }
                }
            }

            return list;
        }
    }
}
