// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XNode.Client.Configuration;

namespace XNode.Client
{
    /// <summary>
    /// 服务代理接口
    /// </summary>
    public interface IServiceProxy
    {
        /// <summary>
        /// 代理名称
        /// </summary>
        string ProxyName { get; }

        /// <summary>
        /// 获取当前代理关联的所有ServiceType
        /// </summary>
        IEnumerable<Type> ServiceTypes { get; }

        /// <summary>
        /// 获取当前代理关联的所有服务代理信息
        /// </summary>
        IEnumerable<ServiceProxyInfo> ServiceProxyInfos { get; }

        /// <summary>
        /// 获取服务代理信息
        /// </summary>
        /// <param name="actionType">Action类型</param>
        ServiceProxyInfo GetServiceProxyInfo(MethodInfo actionType);

        /// <summary>
        /// 添加Service
        /// </summary>
        /// <param name="serviceProxyType">Service类型</param>
        IServiceProxy AddService(Type serviceProxyType);

        /// <summary>
        /// 添加Client
        /// </summary>
        /// <param name="nodeClient">nodeClient实例</param>
        IServiceProxy AddClient(INodeClient nodeClient);

        /// <summary>
        /// 移除Client
        /// </summary>
        /// <param name="host">Client地址</param>
        /// <param name="port">Client端口</param>
        IServiceProxy RemoveClient(string host, int port);

        /// <summary>
        /// 调用远程服务
        /// </summary>
        /// <param name="actionType">Action类型</param>
        /// <param name="paramList">参数列表</param>
        /// <returns></returns>
        Task<object> CallRemoteServiceAsync(MethodInfo actionType, object[] paramList);

        /// <summary>
        /// 执行连接操作
        /// </summary>
        /// <returns></returns>
        Task ConnectAsync();

        /// <summary>
        /// 执行断开连接操作
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();
    }
}
