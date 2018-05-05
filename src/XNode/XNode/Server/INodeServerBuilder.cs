// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using XNode.Security;
using XNode.Serializer;
using XNode.Server.Configuration;
using XNode.ProtocolStack;
using XNode.Server.Route;
using XNode.Communication;
using Microsoft.Extensions.Logging;

namespace XNode.Server
{
    /// <summary>
    /// XNode服务端构造接口
    /// </summary>
    public interface INodeServerBuilder
    {
        /// <summary>
        /// 配置服务器基本信息
        /// </summary>
        /// <param name="host">服务器地址</param>
        /// <param name="port">服务器端口</param>
        /// <returns></returns>
        INodeServerBuilder ConfigServerInfo(string host, int port);

        /// <summary>
        /// 应用服务端配置
        /// </summary>
        /// <param name="config">服务端配置</param>
        /// <returns></returns>
        INodeServerBuilder ApplyConfig(ServerConfig serverConfig);

        /// <summary>
        /// 配置XNode服务器工厂
        /// </summary>
        /// <param name="factory">XNode服务器工厂</param>
        /// <returns></returns>
        INodeServerBuilder ConfigNodeServerFactory(Func<NodeServerConfig, INodeServer> factory);

        /// <summary>
        /// 配置服务提供器
        /// </summary>
        /// <param name="serviceProvider">服务提供器实例</param>
        /// <returns></returns>
        INodeServerBuilder ConfigServiceProvider(IServiceProvider serviceProvider);

        /// <summary>
        /// 配置协议栈工厂
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        INodeServerBuilder ConfigProtocolStackFactory(IProtocolStackFactory factory);

        /// <summary>
        /// 配置路由工厂
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        INodeServerBuilder ConfigRouteFactory(IRouteFactory factory);

        /// <summary>
        /// 配置序列化器
        /// </summary>
        /// <param name="serializer"></param>
        /// <returns></returns>
        INodeServerBuilder ConfigSerializer(ISerializer serializer);

        /// <summary>
        /// 添加服务处理器
        /// </summary>
        /// <param name="serviceProcessor"></param>
        /// <returns></returns>
        INodeServerBuilder AddServiceProcessor(IServiceProcessor serviceProcessor);

        /// <summary>
        /// 配置服务调用器
        /// </summary>
        /// <param name="serviceInvoker"></param>
        /// <returns></returns>
        INodeServerBuilder ConfigServiceInvoker(IServiceInvoker serviceInvoker);

        /// <summary>
        /// 配置登录验证器
        /// </summary>
        /// <param name="loginValidator"></param>
        /// <returns></returns>
        INodeServerBuilder ConfigLoginValidator(ILoginValidator loginValidator);

        /// <summary>
        /// 配置底层通信组件
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        INodeServerBuilder ConfigCommunication(IServer server);

        /// <summary>
        /// 构造XNode服务端实例
        /// </summary>
        /// <returns></returns>
        INodeServer Build();
    }
}
