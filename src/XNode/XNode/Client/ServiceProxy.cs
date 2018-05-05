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

namespace XNode.Client
{
    /// <summary>
    /// 服务代理
    /// </summary>
    public class ServiceProxy : IServiceProxy
    {
        private ILogger logger;

        private IServiceCaller serviceCaller;

        private INodeClientContainer nodeClientContainer;

        private IDictionary<MethodInfo, ServiceProxyInfo> serviceProxyInfoList = new Dictionary<MethodInfo, ServiceProxyInfo>();

        public virtual string ProxyName { get; }

        /// <summary>
        /// 获取当前代理关联的所有ServiceType
        /// </summary>
        public virtual IEnumerable<Type> ServiceTypes
        {
            get
            {
                return serviceProxyInfoList.Keys.Select(s => s.DeclaringType);
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="proxyName">服务名称</param>
        /// <param name="serviceCaller">服务调用器对象，默认为DefaultServiceCaller</param>
        /// <param name="nodeClientContainer">NodeClient容器，默认为DefaultNodeClientContainer</param>
        public ServiceProxy(string proxyName,
            IServiceCaller serviceCaller = null,
            INodeClientContainer nodeClientContainer = null)
        {
            ProxyName = proxyName;
            logger = LoggerManager.ClientLoggerFactory.CreateLogger<ServiceProxy>();
            this.serviceCaller = serviceCaller ?? new DefaultServiceCaller();
            this.nodeClientContainer = nodeClientContainer ?? new DefaultNodeClientContainer();
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
        /// 添加Action
        /// </summary>
        /// <param name="serviceProxyInfo">服务代理信息</param>
        public virtual void AddAction(ServiceProxyInfo serviceProxyInfo)
        {
            if (serviceProxyInfoList.ContainsKey(serviceProxyInfo.ActionProxyType))
            {
                throw new InvalidOperationException($"ActionType has exist. ProxyName={ProxyName}, ActionType={serviceProxyInfo.ActionProxyType}");
            }
            serviceProxyInfo.ProxyName = ProxyName;
            serviceProxyInfoList.Add(serviceProxyInfo.ActionProxyType, serviceProxyInfo);
            logger.LogInformation($"Add action success. ProxyName={ProxyName}, ServiceId={serviceProxyInfo.ServiceId}, ActionId={serviceProxyInfo.ActionId}, Enabled={serviceProxyInfo.Enabled}, Timeout={serviceProxyInfo.Timeout}, ActionProxyType={serviceProxyInfo.ActionProxyType}, ReturnType={serviceProxyInfo.ReturnType}");
        }

        /// <summary>
        /// 添加Client
        /// </summary>
        /// <param name="nodeClient">nodeClient实例</param>
        public virtual void AddClient(INodeClient nodeClient)
        {
            nodeClientContainer.Add(nodeClient);
            logger.LogInformation($"Add client success. ProxyName={ProxyName}, Host={nodeClient.Host}, Port={nodeClient.Port}, LocalHost={nodeClient.LocalHost}, LocalPort={nodeClient.LocalPort}");
        }

        /// <summary>
        /// 移除Client
        /// </summary>
        /// <param name="host">Client地址</param>
        /// <param name="port">Client端口</param>
        public virtual void RemoveClient(string host, int port)
        {
            nodeClientContainer.Remove(host, port);
            logger.LogInformation($"Remove client success. ProxyName={ProxyName}, Host={host}, Port={port}");
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
    }
}
