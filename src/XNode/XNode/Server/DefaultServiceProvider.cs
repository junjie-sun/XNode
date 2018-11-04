// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XNode.Logging;

namespace XNode.Server
{
    /// <summary>
    /// 服务提供器默认实现
    /// </summary>
    public class DefaultServiceProvider : ServiceProviderBase
    {
        private ILogger logger = LoggerManager.ServerLoggerFactory.CreateLogger<DefaultServiceProvider>();

        /// <summary>
        /// 服务实例映射列表
        /// </summary>
        protected IDictionary<Type, object> serviceMap = new Dictionary<Type, object>();

        #region 接口实现

        /// <summary>
        /// 返回指定类型的XNode服务实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public override object GetNodeServiceInstance(Type serviceType)
        {
            logger.LogDebug($"Get node service instance from DefaultServiceProvider. ServiceType={serviceType}");

            return serviceMap.ContainsKey(serviceType) ? serviceMap[serviceType] : null;
        }

        /// <summary>
        /// 获取所有服务类型
        /// </summary>
        /// <returns></returns>
        protected override IList<Type> GetServiceTypes()
        {
            logger.LogDebug($"Get node service types from DefaultServiceProvider.");

            return serviceMap.Keys.ToList();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 注册XNode服务
        /// </summary>
        /// <param name="serviceType">服务对应的类或接口</param>
        /// <param name="instanceType">服务实例化类型。该参数可以是实现或继承serviceType的类型，当serviceType为接口时该参数不能为空</param>
        /// <param name="createServiceInstanceArgs">实例化服务构造函数所需的参数</param>
        /// <returns></returns>
        public virtual DefaultServiceProvider RegistService(Type serviceType, Type instanceType = null, object[] createServiceInstanceArgs = null)
        {
            if (serviceType == null)
            {
                return this;
            }

            var serviceTypeInfo = serviceType.GetTypeInfo();

            if (!serviceTypeInfo.IsInterface && instanceType == null)
            {
                instanceType = serviceType;
            }
            else if (serviceTypeInfo.IsInterface && instanceType == null)
            {
                throw new InvalidOperationException("instanceType can not null when serviceType is interface.");
            }

            if (IsNodeService(serviceType) && !serviceMap.ContainsKey(serviceType) && serviceType.IsAssignableFrom(instanceType))
            {
                object serviceObj = Activator.CreateInstance(instanceType, createServiceInstanceArgs);
                serviceMap.Add(serviceType, serviceObj);
                logger.LogInformation($"Regist service success. ServiceType={serviceType}, InstanceType={instanceType}");
            }

            return this;
        }

        /// <summary>
        /// 注册XNode服务
        /// </summary>
        /// <param name="serviceType">服务对应的类或接口</param>
        /// <param name="createServiceInstance">创建服务实例的委托</param>
        /// <returns></returns>
        public virtual DefaultServiceProvider RegistService(Type serviceType, Func<object> createServiceInstance)
        {
            if (serviceType == null || createServiceInstance == null)
            {
                return this;
            }

            if (IsNodeService(serviceType) && !serviceMap.ContainsKey(serviceType))
            {
                object serviceObj = createServiceInstance();
                if(serviceObj != null && serviceType.IsAssignableFrom(serviceObj.GetType()))
                {
                    serviceMap.Add(serviceType, serviceObj);
                    logger.LogInformation($"Regist service success. ServiceType={serviceType}, InstanceType={serviceObj.GetType()}");
                }
            }

            return this;
        }

        /// <summary>
        /// 注册XNode服务
        /// </summary>
        /// <param name="serviceObj">服务对应的实例对象</param>
        /// <returns></returns>
        public virtual DefaultServiceProvider RegistService(object serviceObj)
        {
            if (serviceObj == null)
            {
                return this;
            }

            var serviceType = serviceObj.GetType();
            if (IsNodeService(serviceType) && !serviceMap.ContainsKey(serviceType))
            {
                serviceMap.Add(serviceType, serviceObj);
                logger.LogInformation($"Regist service success. ServiceType={serviceType}, InstanceType={serviceType}");
            }

            return this;
        }

        #endregion
    }
}
