// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using XNode.Logging;
using XNode.Server.Configuration;

namespace XNode.Server.Route
{
    /// <summary>
    /// 默认实现
    /// 支持从Service类实例中搜索所有公共、非公共以及继承链类型中的公共、受保护拥有ActionAttribute的实例方法作为Action
    /// </summary>
    public class DefaultRouteDescriptor : IRouteDescriptor
    {
        private ILogger logger = LoggerManager.ServerLoggerFactory.CreateLogger<DefaultRouteDescriptor>();

        private IList<ServiceInfo> serviceConfigs;

        /// <summary>
        /// 设置服务相关配置
        /// </summary>
        /// <param name="serviceConfigs"></param>
        public void SetServiceConfig(IList<ServiceInfo> serviceConfigs)
        {
            this.serviceConfigs = serviceConfigs;
        }

        /// <summary>
        /// 创建路由描述信息
        /// </summary>
        /// <param name="serviceType">服务对象类型</param>
        /// <returns></returns>
        public IList<RouteDescription> CreateRouteDescription(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new InvalidOperationException("ServiceType is null.");
            }

            var serviceAttr = serviceType.GetServiceAttribute();

            if (serviceAttr == null)
            {
                throw new InvalidOperationException($"Not found ServiceAttribute on {serviceType.FullName}");
            }

            return CreateRouteDescriptions(serviceType, serviceAttr);
        }

        private IList<RouteDescription> CreateRouteDescriptions(Type serviceType, ServiceAttribute serviceAttr)
        {
            var list = new List<RouteDescription>();

            //返回类型中所有公共、非公共以及继承链类型中的公共、受保护的实例方法
            var methods = serviceType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var method in methods)
            {
                var actionAttr = method.GetCustomAttribute<ActionAttribute>();
                if (actionAttr != null)
                {
                    var routeDescription = new RouteDescription()
                    {
                        ServiceId = serviceAttr.ServiceId,
                        ServiceName = serviceAttr.Name,
                        ServiceType = serviceType,
                        ActionId = actionAttr.ActionId,
                        ActionName = actionAttr.Name,
                        ActionType = method,
                        Enabled = serviceAttr.Enabled && actionAttr.Enabled
                    };
                    list.Add(routeDescription);
                    logger.LogDebug($"Create RouteDesctiption. ServiceId={routeDescription.ServiceId}, ActionId={routeDescription.ActionId}, ServiceType={routeDescription.ServiceType}, ActionType={routeDescription.ActionType}");

                    if (serviceConfigs != null && serviceConfigs.Count > 0)
                    {
                        var serviceConfig = serviceConfigs.Where(c => c.ServiceId == serviceAttr.ServiceId).FirstOrDefault();
                        if (serviceConfig != null)
                        {
                            routeDescription.ServiceName = string.IsNullOrEmpty(serviceConfig.Name) ? routeDescription.ServiceName : serviceConfig.Name;
                            routeDescription.Enabled = serviceConfig.Enabled;
                            var config = serviceConfig.Actions?.Where(c => c.ActionId == actionAttr.ActionId).FirstOrDefault();
                            if (config != null)
                            {
                                routeDescription.ActionName = string.IsNullOrEmpty(config.Name) ? routeDescription.ActionName : config.Name;
                                routeDescription.Enabled = serviceConfig.Enabled && config.Enabled;
                            }
                            logger.LogDebug($"One service is changed by service config. ServiceId={routeDescription.ServiceId}, ActionId={routeDescription.ActionId}");
                        }
                    }
                }
            }

            return list;
        }
    }
}
