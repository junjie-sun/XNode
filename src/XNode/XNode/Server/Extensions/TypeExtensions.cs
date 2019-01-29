// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace XNode.Server
{
    /// <summary>
    /// Type扩展方法类
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// 判断指定类型是否能实例化为XNode服务
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNodeServiceType(this Type type)
        {
            return type.GetServiceAttribute() != null;
        }

        /// <summary>
        /// 获取XNode服务的Attribute
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ServiceAttribute GetServiceAttribute(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.GetCustomAttribute<ServiceAttribute>(false);
        }
    }
}
