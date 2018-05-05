// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using XNode.Client.Configuration;

namespace XNode.Client
{
    /// <summary>
    /// 服务代理管理器
    /// </summary>
    public class ServiceProxyManager : IServiceProxyManager
    {
        private IDictionary<Type, IServiceProxy> serviceProxyCache = new Dictionary<Type, IServiceProxy>();

        private IDictionary<string, IServiceProxy> serviceProxyList = new Dictionary<string, IServiceProxy>();

        /// <summary>
        /// 判断指定类型是否为服务代理
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual bool IsServiceProxy(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.GetCustomAttribute<ServiceProxyAttribute>() == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取指定服务代理
        /// </summary>
        /// <param name="proxyName">代理名称</param>
        /// <returns></returns>
        public virtual IServiceProxy GetServiceProxy(string proxyName)
        {
            return serviceProxyList.ContainsKey(proxyName) ? serviceProxyList[proxyName] : null;
        }

        /// <summary>
        /// 根据服务类型获取指定服务代理
        /// </summary>
        /// <param name="proxyName">服务类型</param>
        /// <returns></returns>
        public virtual IServiceProxy GetServiceProxy(Type serviceType)
        {
            return serviceProxyCache.ContainsKey(serviceType) ? serviceProxyCache[serviceType] : null;
        }

        /// <summary>
        /// 注册服务代理
        /// </summary>
        /// <param name="proxyName">代理名称</param>
        /// <param name="serviceTypeList">服务类型列表</param>
        /// <param name="serviceCaller">服务调用器对象，默认为DefaultServiceCaller</param>
        /// <param name="nodeClientContainer">NodeClient容器，默认为DefaultNodeClientContainer</param>
        /// <param name="serviceProxyFactory">服务代理工厂，默认为ServiceProxy</param>
        /// <returns></returns>
        public virtual IServiceProxy Regist(
            string proxyName,
            IList<Type> serviceTypeList,
            IServiceCaller serviceCaller = null,
            INodeClientContainer nodeClientContainer = null,
            Func<string, IServiceCaller, INodeClientContainer, IServiceProxy> serviceProxyFactory = null)
        {
            if (string.IsNullOrEmpty(proxyName))
            {
                throw new InvalidOperationException("ProxyName is null.");
            }

            if (serviceProxyList.ContainsKey(proxyName))
            {
                throw new InvalidOperationException($"Proxy has registed. ProxyName={proxyName}");
            }

            if (serviceTypeList == null)
            {
                throw new InvalidOperationException("ServiceTypeList is null.");
            }

            var serviceProxy = serviceProxyFactory == null ? new ServiceProxy(proxyName, serviceCaller, nodeClientContainer) : serviceProxyFactory(proxyName, serviceCaller, nodeClientContainer);

            foreach (var serviceType in serviceTypeList)
            {
                var serviceProxyInfoList = CreateServiceProxyInfos(proxyName, serviceType);
                if (serviceProxyInfoList.Count > 0)
                {
                    foreach (var serviceProxyInfo in serviceProxyInfoList)
                    {
                        serviceProxy.AddAction(serviceProxyInfo);
                    }
                    serviceProxyCache.Add(serviceType, serviceProxy);
                }
            }

            serviceProxyList.Add(proxyName, serviceProxy);

            return serviceProxy;
        }

        /// <summary>
        /// 注册服务代理
        /// </summary>
        /// <param name="config">服务代理配置</param>
        /// <param name="serviceCaller">服务调用器对象，默认为DefaultServiceCaller</param>
        /// <param name="nodeClientContainer">NodeClient容器，默认为DefaultNodeClientContainer</param>
        /// <param name="serviceProxyFactory">服务代理工厂，默认为ServiceProxy</param>
        /// <returns></returns>
        public virtual IServiceProxy Regist(
            ServiceProxyConfig config,
            IServiceCaller serviceCaller = null,
            INodeClientContainer nodeClientContainer = null,
            Func<string, IServiceCaller, INodeClientContainer, IServiceProxy> serviceProxyFactory = null)
        {
            if (config == null)
            {
                throw new InvalidOperationException("ServiceProxyConfig is null.");
            }

            var serviceProxy = serviceProxyFactory == null ? new ServiceProxy(config.ProxyName, serviceCaller, nodeClientContainer) : serviceProxyFactory(config.ProxyName, serviceCaller, nodeClientContainer);

            if (config.Services != null)
            {
                foreach (var serviceProxyConfig in config.Services)
                {
                    var serviceProxyType = Type.GetType(serviceProxyConfig.TypeName);
                    var serviceProxyInfoList = CreateServiceProxyInfos(config.ProxyName, serviceProxyConfig, serviceProxyType);
                    foreach (var serviceProxyInfo in serviceProxyInfoList)
                    {
                        serviceProxy.AddAction(serviceProxyInfo);
                    }
                    serviceProxyCache.Add(serviceProxyType, serviceProxy);
                }
            }

            serviceProxyList.Add(config.ProxyName, serviceProxy);

            return serviceProxy;
        }

        /// <summary>
        /// 为指定的代理执行连接操作，如果proxyName为null则为所有已注册的代理执行连接操作
        /// </summary>
        /// <param name="proxyName">代理名称</param>
        /// <returns></returns>
        public Task ConnectAsync(string proxyName = null)
        {
            if (string.IsNullOrEmpty(proxyName))
            {
                var taskList = new List<Task>();
                foreach (var serviceProxy in serviceProxyList.Values)
                {
                    taskList.Add(serviceProxy.ConnectAsync());
                }
                return Task.WhenAll(taskList);
            }

            var proxy = GetServiceProxy(proxyName);
            if (proxy == null)
            {
                throw new InvalidOperationException($"Proxy is not exist. ProxyName={proxyName}");
            }

            return proxy.ConnectAsync();
        }

        /// <summary>
        /// 为指定的代理执行断开连接操作，如果proxyName为null则为所有已注册的代理执行断开连接操作
        /// </summary>
        /// <param name="proxyName">代理名称</param>
        /// <returns></returns>
        public Task CloseAsync(string proxyName = null)
        {
            if (string.IsNullOrEmpty(proxyName))
            {
                var taskList = new List<Task>();
                foreach (var serviceProxy in serviceProxyList.Values)
                {
                    taskList.Add(serviceProxy.CloseAsync());
                }
                return Task.WhenAll(taskList);
            }

            var proxy = GetServiceProxy(proxyName);
            if (proxy == null)
            {
                throw new InvalidOperationException($"Proxy is not exist. ProxyName={proxyName}");
            }

            return proxy.CloseAsync();
        }

        #region 私有方法

        private IList<ServiceProxyInfo> CreateServiceProxyInfos(string proxyName, Type serviceProxyType)
        {
            var list = new List<ServiceProxyInfo>();
            var typeInfo = serviceProxyType.GetTypeInfo();
            var serviceProxyAttr = typeInfo.GetCustomAttribute<ServiceProxyAttribute>();
            if (serviceProxyAttr == null)
            {
                throw new InvalidOperationException($"ServiceProxyType has not set ServiceProxyAttribute. Type={serviceProxyType}");
            }

            var methods = serviceProxyType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var actionProxyType in methods)
            {
                var actionProxyAttr = actionProxyType.GetCustomAttribute<ActionProxyAttribute>();
                if (actionProxyAttr == null)
                {
                    continue;
                }

                var serviceProxyInfo = CreateServiceProxyInfo(serviceProxyType, actionProxyType, proxyName, serviceProxyAttr.ServiceId, serviceProxyAttr.Name, serviceProxyAttr.Enabled, actionProxyAttr.ActionId, actionProxyAttr.Name, actionProxyAttr.Enabled, actionProxyAttr.Timeout);
                list.Add(serviceProxyInfo);
            }

            return list;
        }

        private IList<ServiceProxyInfo> CreateServiceProxyInfos(string proxyName, ServiceInfo serviceProxyConfig, Type serviceProxyType)
        {
            var list = new List<ServiceProxyInfo>();
            var typeInfo = serviceProxyType.GetTypeInfo();
            var serviceProxyAttr = typeInfo.GetCustomAttribute<ServiceProxyAttribute>();

            if (serviceProxyAttr == null)
            {
                throw new InvalidOperationException($"ServiceProxyType has not set ServiceProxyAttribute. Type={serviceProxyType}");
            }
            if (serviceProxyAttr.ServiceId != serviceProxyConfig.ServiceId)
            {
                throw new InvalidOperationException($"ServiceId in config is {serviceProxyConfig.ServiceId}, but in attribute is {serviceProxyAttr.ServiceId}.");
            }

            var methods = serviceProxyType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var actionProxyType in methods)
            {
                var actionProxyAttr = actionProxyType.GetCustomAttribute<ActionProxyAttribute>();
                if (actionProxyAttr == null)
                {
                    continue;
                }
                var actionProxyConfig = serviceProxyConfig.Actions != null ? serviceProxyConfig.Actions.Where(a => a.ActionId == actionProxyAttr.ActionId).SingleOrDefault() : null;
                list.Add(CreateServiceProxyInfo(
                    serviceProxyType,
                    actionProxyType,
                    proxyName,
                    serviceProxyConfig.ServiceId,
                    !string.IsNullOrEmpty(serviceProxyConfig.Name) ? serviceProxyConfig.Name : serviceProxyAttr.Name,
                    serviceProxyConfig.Enabled,
                    actionProxyConfig != null ? actionProxyConfig.ActionId : actionProxyAttr.ActionId,
                    actionProxyConfig != null && !string.IsNullOrEmpty(actionProxyConfig.Name) ? actionProxyConfig.Name : actionProxyAttr.Name,
                    actionProxyConfig != null ? actionProxyConfig.Enabled : actionProxyAttr.Enabled,
                    actionProxyConfig != null ? actionProxyConfig.Timeout : actionProxyAttr.Timeout));
            }

            return list;
        }

        private ServiceProxyInfo CreateServiceProxyInfo(Type serviceProxyType,
            MethodInfo actionProxyType,
            string proxyName,
            int serviceId,
            string serviceName,
            bool serviceEnabled,
            int actionId,
            string actionName,
            bool actionEnabled,
            int actionTimeout)
        {
            var proxyInfo = new ServiceProxyInfo()
            {
                ProxyName = proxyName,
                ServiceId = serviceId,
                ActionId = actionId,
                ServiceName = serviceName,
                ActionName = actionName,
                Timeout = actionTimeout,
                ServiceProxyType = serviceProxyType,
                ActionProxyType = actionProxyType,
                ReturnType = GetReturnType(actionProxyType),
                Enabled = serviceEnabled && actionEnabled
            };

            return proxyInfo;
        }

        private Type GetReturnType(MethodInfo actionProxyType)
        {
            if (actionProxyType.ReturnType == typeof(void) || actionProxyType.ReturnType == typeof(Task))
            {
                return null;
            }
            else if (typeof(Task).IsAssignableFrom(actionProxyType.ReturnType) && actionProxyType.ReturnType.GetTypeInfo().IsGenericType)
            {
                var args = actionProxyType.ReturnType.GetGenericArguments();
                return args.Length > 0 ? args[0] : null;
            }
            else
            {
                return actionProxyType.ReturnType;
            }
        }

        #endregion
    }
}
