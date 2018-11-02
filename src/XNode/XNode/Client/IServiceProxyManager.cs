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
        /// <param name="serviceProxy">服务代理</param>
        void Regist(IServiceProxy serviceProxy);

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
