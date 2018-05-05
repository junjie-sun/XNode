// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XNode.Client.Configuration;

namespace XNode.Client
{
    /// <summary>
    /// 服务代理管理器接口
    /// </summary>
    public interface IServiceProxyManager
    {
        /// <summary>
        /// 判断指定类型是否为服务代理
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsServiceProxy(Type type);

        /// <summary>
        /// 获取指定服务代理
        /// </summary>
        /// <param name="proxyName">代理名称</param>
        /// <returns></returns>
        IServiceProxy GetServiceProxy(string proxyName);

        /// <summary>
        /// 根据服务类型获取指定服务代理
        /// </summary>
        /// <param name="proxyName">服务类型</param>
        /// <returns></returns>
        IServiceProxy GetServiceProxy(Type serviceType);

        /// <summary>
        /// 注册服务代理
        /// </summary>
        /// <param name="proxyName">代理名称</param>
        /// <param name="serviceTypeList">服务类型列表</param>
        /// <param name="serviceCaller">服务调用器对象，默认为DefaultServiceCaller</param>
        /// <param name="nodeClientContainer">NodeClient容器，默认为DefaultNodeClientContainer</param>
        /// <param name="serviceProxyFactory">服务代理工厂，默认为ServiceProxy</param>
        /// <returns></returns>
        IServiceProxy Regist(
            string proxyName,
            IList<Type> serviceTypeList,
            IServiceCaller serviceCaller = null,
            INodeClientContainer nodeClientContainer = null,
            Func<string, IServiceCaller, INodeClientContainer, IServiceProxy> serviceProxyFactory = null);

        /// <summary>
        /// 注册服务代理
        /// </summary>
        /// <param name="config">服务代理配置</param>
        /// <param name="serviceCaller">服务调用器对象，默认为DefaultServiceCaller</param>
        /// <param name="nodeClientContainer">NodeClient容器，默认为DefaultNodeClientContainer</param>
        /// <param name="serviceProxyFactory">服务代理工厂，默认为ServiceProxy</param>
        /// <returns></returns>
        IServiceProxy Regist(
            ServiceProxyConfig config,
            IServiceCaller serviceCaller = null,
            INodeClientContainer nodeClientContainer = null,
            Func<string, IServiceCaller, INodeClientContainer, IServiceProxy> serviceProxyFactory = null);

        /// <summary>
        /// 为指定的代理执行连接操作，如果proxyName为null则为所有已注册的代理执行连接操作
        /// </summary>
        /// <param name="proxyName">代理名称</param>
        /// <returns></returns>
        Task ConnectAsync(string proxyName = null);

        /// <summary>
        /// 为指定的代理执行断开连接操作，如果proxyName为null则为所有已注册的代理执行断开连接操作
        /// </summary>
        /// <param name="proxyName">代理名称</param>
        /// <returns></returns>
        Task CloseAsync(string proxyName = null);
    }
}
