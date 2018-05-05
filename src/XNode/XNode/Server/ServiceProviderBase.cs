// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace XNode.Server
{
    /// <summary>
    /// 服务提供器基类
    /// </summary>
    public abstract class ServiceProviderBase : IServiceProvider
    {
        private IList<Type> serviceList;

        /// <summary>
        /// 返回指定类型的XNode服务实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public abstract object GetNodeServiceInstance(Type serviceType);

        /// <summary>
        /// 返回所有需要注册为XNode服务的实例类型，必须为可实例化的Class
        /// </summary>
        /// <returns></returns>
        public virtual IList<Type> GetNodeServiceTypes()
        {
            if (serviceList != null)
            {
                return serviceList;
            }

            serviceList = new List<Type>();

            var serviceTypes = GetServiceTypes();

            if (serviceTypes == null || serviceTypes.Count == 0)
            {
                return serviceList;
            }

            foreach (var type in serviceTypes)
            {
                if (IsNodeService(type))
                {
                    serviceList.Add(type);
                }
            }

            return serviceList;
        }

        /// <summary>
        /// 返回所有需要注册为XNode服务的实例类型，必须为可实例化的Class
        /// </summary>
        /// <returns></returns>
        protected abstract IList<Type> GetServiceTypes();

        /// <summary>
        /// 判断指定类型是否能实例化为XNode服务
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected bool IsNodeService(Type type)
        {
            if (type == null)
            {
                return false;
            }

            var typeInfo = type.GetTypeInfo();
            var serviceAttr = typeInfo.GetCustomAttribute<ServiceAttribute>(false);
            return serviceAttr != null;
        }
    }
}
