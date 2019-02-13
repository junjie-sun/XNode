// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XNode.Client;
using XNode.Client.Configuration;

namespace XNode.ServiceDiscovery.Zookeeper
{
    /// <summary>
    /// 服务代理构造器
    /// </summary>
    public class ServiceProxyCreator : IServiceProxyCreator
    {
        private ILogger logger;

        private Func<ServiceProxyArgs, IServiceProxy> serviceProxyFactory;

        private IList<ServiceInfo> serviceConfigs;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="serviceProxyFactory">ServiceProxy工厂</param>
        /// <param name="serviceConfigs">服务配置</param>
        public ServiceProxyCreator(ILoggerFactory loggerFactory,
            Func<ServiceProxyArgs, IServiceProxy> serviceProxyFactory,
            IList<ServiceInfo> serviceConfigs = null)
        {
            logger = loggerFactory.CreateLogger<ServiceProxyCreator>();
            this.serviceProxyFactory = serviceProxyFactory;
            this.serviceConfigs = serviceConfigs;
        }

        /// <summary>
        /// 创建服务代理实例
        /// </summary>
        /// <param name="proxyName">代理名称</param>
        /// <param name="serviceProxyType">服务代理类型</param>
        /// <param name="nodeClientList">NodeClient实例</param>
        /// <returns></returns>
        public IServiceProxy Create(string proxyName,
            Type serviceProxyType,
            IList<INodeClient> nodeClientList)
        {
            var serviceProxyAttr = serviceProxyType.GetServiceProxyAttribute();

            if (serviceProxyAttr == null)
            {
                throw new InvalidOperationException($"ServiceProxyType has not set ServiceProxyAttribute. Type={serviceProxyType.FullName}");
            }

            var config = GetServiceInfo(serviceProxyAttr);

            var serviceProxy = serviceProxyFactory(new ServiceProxyArgs()
            {
                Name = proxyName,
                ServiceInfo = config,
                ServiceType = serviceProxyType
            }).AddService(serviceProxyType);

            serviceProxy.AddClients(nodeClientList);

            return serviceProxy;
        }

        /// <summary>
        /// 创建默认ServiceProxyFactory
        /// </summary>
        /// <param name="serviceCaller"></param>
        /// <returns></returns>
        public static Func<ServiceProxyArgs, IServiceProxy> CreateDefaultServiceProxyFactory(IServiceCaller serviceCaller)
        {
            IServiceProxy serviceProxyFactory(ServiceProxyArgs args)
            {
                return new ServiceProxy(
                args.Name,
                new List<ServiceInfo>() { args.ServiceInfo },
                serviceCaller);
            }
            return serviceProxyFactory;
        }

        #region 私有方法

        private ServiceInfo GetServiceInfo(ServiceProxyAttribute serviceProxyAttr)
        {
            ServiceInfo config = null;
            if (serviceConfigs != null && serviceConfigs.Count > 0)
            {
                config = serviceConfigs.Where(c => c.ServiceId == serviceProxyAttr.ServiceId).FirstOrDefault();
            }
            return config;
        }

        #endregion
    }
}
