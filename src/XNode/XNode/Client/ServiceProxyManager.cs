// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

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
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        public virtual IServiceProxy GetServiceProxy(Type serviceType)
        {
            return serviceProxyCache.ContainsKey(serviceType) ? serviceProxyCache[serviceType] : null;
        }

        /// <summary>
        /// 注册服务代理
        /// </summary>
        /// <param name="serviceProxy">服务代理</param>
        public virtual void Regist(IServiceProxy serviceProxy)
        {
            if (serviceProxy == null)
            {
                throw new InvalidOperationException("ServiceProxy is null.");
            }

            if (serviceProxy.ServiceTypes == null)
            {
                return;
            }

            foreach (var serviceType in serviceProxy.ServiceTypes)
            {
                if (serviceProxyCache.ContainsKey(serviceType))
                {
                    throw new InvalidOperationException($"ServiceProxy has registed. ServiceType={serviceType.FullName}");
                }
                serviceProxyCache.Add(serviceType, serviceProxy);
            }
            serviceProxyList.Add(serviceProxy.ProxyName, serviceProxy);
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
    }
}
