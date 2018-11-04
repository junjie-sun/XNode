// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using XNode.Security;
using XNode.Serializer;
using XNode.Server.Configuration;
using XNode.Server.Processors;
using XNode.ProtocolStack;
using XNode.Server.Route;
using XNode.Communication;
using Microsoft.Extensions.Logging;
using XNode.Logging;

namespace XNode.Server
{
    /// <summary>
    /// XNode服务端构造类
    /// </summary>
    public class NodeServerBuilder : INodeServerBuilder
    {
        private bool isBuild = false;

        private IServiceProcessor lastServiceProcessor;

        private Func<NodeServerConfig, INodeServer> nodeServerFactory;

        private NodeServerConfig config = new NodeServerConfig();

        /// <summary>
        /// 配置服务器基本信息
        /// </summary>
        /// <param name="host">服务器地址</param>
        /// <param name="port">服务器端口</param>
        /// <returns></returns>
        public INodeServerBuilder ConfigServerInfo(string host, int port)
        {
            CheckIsBuild();

            config.Host = host;
            config.Port = port;

            return this;
        }

        /// <summary>
        /// 应用服务端配置
        /// </summary>
        /// <param name="serverConfig">服务端配置</param>
        /// <returns></returns>
        public INodeServerBuilder ApplyConfig(ServerConfig serverConfig)
        {
            CheckIsBuild();

            if (serverConfig == null)
            {
                return this;
            }

            if (serverConfig.ServerInfo != null)
            {
                config.Host = serverConfig.ServerInfo.Host;
                config.Port = serverConfig.ServerInfo.Port;
            }

            config.ServiceConfigs = serverConfig.Services;

            return this;
        }

        /// <summary>
        /// 配置服务器工厂
        /// </summary>
        /// <param name="factory">XNode服务器工厂</param>
        /// <returns></returns>
        public INodeServerBuilder ConfigNodeServerFactory(Func<NodeServerConfig, INodeServer> factory)
        {
            CheckIsBuild();

            this.nodeServerFactory = factory;

            return this;
        }

        /// <summary>
        /// 配置服务提供器
        /// </summary>
        /// <param name="serviceProvider">服务提供器实例</param>
        /// <returns></returns>
        public INodeServerBuilder ConfigServiceProvider(IServiceProvider serviceProvider)
        {
            CheckIsBuild();

            config.ServiceProvider = serviceProvider;

            return this;
        }

        /// <summary>
        /// 配置协议栈工厂
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public INodeServerBuilder ConfigProtocolStackFactory(IProtocolStackFactory factory)
        {
            CheckIsBuild();

            config.ProtocolStackFactory = factory;

            return this;
        }

        /// <summary>
        /// 配置路由工厂
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public INodeServerBuilder ConfigRouteFactory(IRouteFactory factory)
        {
            CheckIsBuild();

            config.RouteFactory = factory;

            return this;
        }

        /// <summary>
        /// 配置序列化器
        /// </summary>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public INodeServerBuilder ConfigSerializer(ISerializer serializer)
        {
            CheckIsBuild();

            config.Serializer = serializer;

            return this;
        }

        /// <summary>
        /// 添加服务处理器
        /// </summary>
        /// <param name="serviceProcessor"></param>
        /// <returns></returns>
        public INodeServerBuilder AddServiceProcessor(IServiceProcessor serviceProcessor)
        {
            CheckIsBuild();

            if (serviceProcessor == null)
            {
                return this;
            }

            if (config.ServiceProcessor == null)
            {
                config.ServiceProcessor = lastServiceProcessor = serviceProcessor;
            }
            else
            {
                lastServiceProcessor.Next = serviceProcessor;
                lastServiceProcessor = serviceProcessor;
            }

            return this;
        }

        /// <summary>
        /// 配置服务调用器
        /// </summary>
        /// <param name="serviceInvoker"></param>
        /// <returns></returns>
        public INodeServerBuilder ConfigServiceInvoker(IServiceInvoker serviceInvoker)
        {
            CheckIsBuild();

            config.ServiceInvoker = serviceInvoker;

            return this;
        }

        /// <summary>
        /// 配置登录验证器
        /// </summary>
        /// <param name="loginValidator"></param>
        /// <returns></returns>
        public INodeServerBuilder ConfigLoginValidator(ILoginValidator loginValidator)
        {
            CheckIsBuild();

            config.LoginValidator = loginValidator;

            return this;
        }

        /// <summary>
        /// 配置底层服务端通信组件
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public INodeServerBuilder ConfigCommunication(IServer server)
        {
            CheckIsBuild();

            config.Communication = server;

            return this;
        }

        /// <summary>
        /// 构造XNode服务端实例
        /// </summary>
        /// <returns></returns>
        public INodeServer Build()
        {
            BuildConfig();

            BuildLoginValidator();

            BuildServiceInvoker();

            BuildServiceProcessor();

            var nodeServer = BuildNodeServer();

            isBuild = true;

            return nodeServer;
        }

        #region 私有方法

        private void CheckIsBuild()
        {
            if (isBuild)
            {
                throw new InvalidOperationException("XNode server has built.");
            }
        }

        private void BuildConfig()
        {
            CheckIsBuild();

            if (config.Communication == null)
            {
                throw new InvalidOperationException("Communication is not config.");
            }

            if (config.Serializer == null)
            {
                throw new InvalidOperationException("Serializer is not config.");
            }

            if (config.ServiceProvider == null)
            {
                throw new InvalidOperationException("ServiceProvider is not config.");
            }

            if (config.ProtocolStackFactory == null)
            {
                config.ProtocolStackFactory = new DefaultProtocolStackFactory();
            }

            if (config.RouteFactory == null)
            {
                config.RouteFactory = new DefaultRouteFactory();
            }

            if (config.LoginValidator == null)
            {
                config.LoginValidator = new DefaultLoginValidator();
            }

            if (config.ServiceInvoker == null)
            {
                config.ServiceInvoker = new DefaultServiceInvoker();
            }

            if (config.ServiceProcessor == null)
            {
                config.ServiceProcessor = new DefaultServiceProcessor();
            }
            else
            {
                IServiceProcessor last = config.ServiceProcessor;
                while (last.Next != null)
                {
                    last = last.Next;
                }
                last.Next = new DefaultServiceProcessor();
            }
        }

        private void BuildServiceInvoker()
        {
            var serviceInvoker = config.ServiceInvoker;
            serviceInvoker.ServiceProvider = config.ServiceProvider;
        }

        private void BuildServiceProcessor()
        {
            var processor = config.ServiceProcessor;
            while (processor != null)
            {
                processor.ProtocolStackFactory = config.ProtocolStackFactory;
                processor.Serializer = config.Serializer;
                processor.ServiceInvoker = config.ServiceInvoker;
                processor = processor.Next;
            }
        }

        private void BuildLoginValidator()
        {
            var loginValidator = config.LoginValidator;
            loginValidator.Serializer = config.Serializer;
        }

        private INodeServer BuildNodeServer()
        {
            INodeServer nodeServer = nodeServerFactory == null ? new DefaultNodeServer(config) : nodeServerFactory(config);

            if (nodeServer == null)
            {
                throw new InvalidOperationException("Load XNode server instance failed.");
            }

            return nodeServer;
        }

        #endregion
    }
}
