// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace XNode.Client
{
    /// <summary>
    /// Type扩展方法类
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// 判断指定类型是否能实例化为XNode服务代理
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNodeServiceProxyType(this Type type)
        {
            return type.GetServiceProxyAttribute() != null;
        }

        /// <summary>
        /// 获取XNode服务代理的Attribute
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ServiceProxyAttribute GetServiceProxyAttribute(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.GetCustomAttribute<ServiceProxyAttribute>(false);
        }
    }
}
