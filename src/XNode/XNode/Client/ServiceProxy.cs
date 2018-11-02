// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using XNode.Client.NodeClientContainers;
using XNode.Client.ServiceCallers;
using XNode.Logging;
using XNode.Client.Configuration;

namespace XNode.Client
{
    /// <summary>
    /// 服务代理
    /// </summary>
    public class ServiceProxy : IServiceProxy
    {
        private ILogger logger;

        private IList<ServiceInfo> serviceInfos;

        private IServiceCaller serviceCaller;

        private INodeClientContainer nodeClientContainer;

        private IDictionary<MethodInfo, ServiceProxyInfo> serviceProxyInfoList = new Dictionary<MethodInfo, ServiceProxyInfo>();

        public virtual string ProxyName { get; }

        /// <summary>
        /// 获取当前代理关联的所有ServiceType
        /// </summary>
        public virtual IList<Type> ServiceTypes
        {
            get
            {
                return serviceProxyInfoList.Keys.Select(s => s.DeclaringType).Distinct().ToList();
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="proxyName">服务名称</param>
        /// <param name="serviceInfos">服务配置信息</param>
        /// <param name="serviceCaller">服务调用器对象，默认为DefaultServiceCaller</param>
        /// <param name="nodeClientContainer">NodeClient容器，默认为DefaultNodeClientContainer</param>
        public ServiceProxy(string proxyName,
            IList<ServiceInfo> serviceInfos = null,
            IServiceCaller serviceCaller = null,
            INodeClientContainer nodeClientContainer = null)
        {
            ProxyName = proxyName;
            logger = LoggerManager.ClientLoggerFactory.CreateLogger<ServiceProxy>();
            this.serviceInfos = serviceInfos;
            this.serviceCaller = serviceCaller ?? new DefaultServiceCaller();
            this.nodeClientContainer = nodeClientContainer ?? new DefaultNodeClientContainer();
        }

        public virtual IServiceProxy AddService(Type serviceProxyType)
        {
            var serviceProxyInfoList = CreateServiceProxyInfos(ProxyName, serviceProxyType);
            foreach (var serviceProxyInfo in serviceProxyInfoList)
            {
                AddAction(serviceProxyInfo);
            }
            return this;
        }

        /// <summary>
        /// 获取服务代理信息
        /// </summary>
        /// <param name="actionType">Action类型</param>
        public virtual ServiceProxyInfo GetServiceProxyInfo(MethodInfo actionType)
        {
            return serviceProxyInfoList.ContainsKey(actionType) ? serviceProxyInfoList[actionType] : null;
        }

        /// <summary>
        /// 添加Client
        /// </summary>
        /// <param name="nodeClient">nodeClient实例</param>
        public virtual IServiceProxy AddClient(INodeClient nodeClient)
        {
            nodeClientContainer.Add(nodeClient);
            logger.LogInformation($"Add client success. ProxyName={ProxyName}, Host={nodeClient.Host}, Port={nodeClient.Port}, LocalHost={nodeClient.LocalHost}, LocalPort={nodeClient.LocalPort}");
            return this;
        }

        /// <summary>
        /// 移除Client
        /// </summary>
        /// <param name="host">Client地址</param>
        /// <param name="port">Client端口</param>
        public virtual IServiceProxy RemoveClient(string host, int port)
        {
            nodeClientContainer.Remove(host, port);
            logger.LogInformation($"Remove client success. ProxyName={ProxyName}, Host={host}, Port={port}");
            return this;
        }

        /// <summary>
        /// 调用远程服务
        /// </summary>
        /// <param name="actionType">Action类型</param>
        /// <param name="paramList">参数列表</param>
        /// <returns></returns>
        public virtual async Task<object> CallRemoteServiceAsync(MethodInfo actionType, object[] paramList)
        {
            ServiceProxyInfo serviceProxyInfo = GetServiceProxyInfo(actionType);

            if (serviceProxyInfo == null)
            {
                throw new InvalidOperationException($"Action not found. ProxyName={serviceProxyInfo.ProxyName}, ServiceId={serviceProxyInfo.ServiceId}，ActionId={serviceProxyInfo.ActionId}");
            }

            logger.LogInformation($"Call remote service beginning. ProxyName={serviceProxyInfo.ProxyName}, ServiceId={serviceProxyInfo.ServiceId}, ActionId={serviceProxyInfo.ActionId}");

            logger.LogDebug($"Service proxy detail info. ProxyName={serviceProxyInfo.ProxyName}, ServiceId={serviceProxyInfo.ServiceId}, ActionId={serviceProxyInfo.ActionId}, Enabled={serviceProxyInfo.Enabled}, Timeout={serviceProxyInfo.Timeout}, ActionProxyType={serviceProxyInfo.ActionProxyType}, ReturnType={serviceProxyInfo.ReturnType}");

            if (!serviceProxyInfo.Enabled)
            {
                throw new InvalidOperationException($"Action is disabled. ProxyName={serviceProxyInfo.ProxyName}, ServiceId={serviceProxyInfo.ServiceId}，ActionId={serviceProxyInfo.ActionId}");
            }

            logger.LogDebug($"Invoke ServiceCaller.CallAsync. ProxyName={serviceProxyInfo.ProxyName}, ServiceId={serviceProxyInfo.ServiceId}, ActionId={serviceProxyInfo.ActionId}, ParamList={(paramList == null || paramList.Length == 0 ? string.Empty : string.Join("|", paramList))}");
            ServiceCallResult result = await serviceCaller.CallAsync(nodeClientContainer, new ServiceCallInfo()
            {
                ProxyName = serviceProxyInfo.ProxyName,
                ServiceId = serviceProxyInfo.ServiceId,
                ActionId = serviceProxyInfo.ActionId,
                ServiceName = serviceProxyInfo.ServiceName,
                ActionName = serviceProxyInfo.ActionName,
                ParamList = paramList,
                ReturnType = serviceProxyInfo.ReturnType,
                Timeout = serviceProxyInfo.Timeout
            });

            logger.LogInformation($"Call remote service finished. ProxyName={serviceProxyInfo.ProxyName}, ServiceId={serviceProxyInfo.ServiceId}, ActionId={serviceProxyInfo.ActionId}");

            return result.ReturnVal;
        }

        /// <summary>
        /// 执行连接操作
        /// </summary>
        /// <returns></returns>
        public virtual Task ConnectAsync()
        {
            return nodeClientContainer.ConnectAsync();
        }

        /// <summary>
        /// 执行断开连接操作
        /// </summary>
        /// <returns></returns>
        public virtual Task CloseAsync()
        {
            return nodeClientContainer.CloseAsync();
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

            var serviceProxyConfig = serviceInfos != null ? serviceInfos.Where(info => info.ServiceId == serviceProxyAttr.ServiceId).SingleOrDefault() : null;

            var methods = serviceProxyType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var actionProxyType in methods)
            {
                var actionProxyAttr = actionProxyType.GetCustomAttribute<ActionProxyAttribute>();
                if (actionProxyAttr == null)
                {
                    continue;
                }
                var actionProxyConfig = serviceProxyConfig != null && serviceProxyConfig.Actions != null ? serviceProxyConfig.Actions.Where(a => a.ActionId == actionProxyAttr.ActionId).SingleOrDefault() : null;
                list.Add(CreateServiceProxyInfo(
                    serviceProxyType,
                    actionProxyType,
                    proxyName,
                    serviceProxyAttr.ServiceId,
                    serviceProxyConfig != null && !string.IsNullOrEmpty(serviceProxyConfig.Name) ? serviceProxyConfig.Name : serviceProxyAttr.Name,
                    serviceProxyConfig != null ? serviceProxyConfig.Enabled : serviceProxyAttr.Enabled,
                    actionProxyAttr.ActionId,
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

        /// <summary>
        /// 添加Action
        /// </summary>
        /// <param name="serviceProxyInfo">服务代理信息</param>
        private void AddAction(ServiceProxyInfo serviceProxyInfo)
        {
            if (serviceProxyInfoList.ContainsKey(serviceProxyInfo.ActionProxyType))
            {
                throw new InvalidOperationException($"ActionType has exist. ProxyName={ProxyName}, ActionType={serviceProxyInfo.ActionProxyType}");
            }
            serviceProxyInfo.ProxyName = ProxyName;
            serviceProxyInfoList.Add(serviceProxyInfo.ActionProxyType, serviceProxyInfo);
            logger.LogInformation($"Add action success. ProxyName={ProxyName}, ServiceId={serviceProxyInfo.ServiceId}, ActionId={serviceProxyInfo.ActionId}, Enabled={serviceProxyInfo.Enabled}, Timeout={serviceProxyInfo.Timeout}, ActionProxyType={serviceProxyInfo.ActionProxyType}, ReturnType={serviceProxyInfo.ReturnType}");
        }

        #endregion
    }
}
