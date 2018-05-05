using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using XNode.Client.Configuration;
using XNode.Communication;
using XNode.Logging;
using XNode.ProtocolStack;
using XNode.Security;
using XNode.Serializer;

namespace XNode.Client
{
    public class NodeClientBuilder : INodeClientBuilder
    {
        private bool isBuild = false;

        private IList<ConnectionInfo> connectionInfos;

        private ISerializer serializer;

        private IProtocolStackFactory protocolStackFactory;

        private ILoginHandler loginHandler;

        private Func<NodeClientParameters, INodeClient> nodeClientFactory;

        private Func<ConnectionInfo, IClient> communicationFactory;

        /// <summary>
        /// 配置客户端连接
        /// </summary>
        /// <param name="connectionInfos">客户端连接信息</param>
        /// <returns></returns>
        public INodeClientBuilder ConfigConnections(IList<ConnectionInfo> connectionInfos)
        {
            CheckIsBuild();

            this.connectionInfos = connectionInfos;

            return this;
        }

        /// <summary>
        /// 配置序列化器
        /// </summary>
        /// <param name="serializer">序列化器</param>
        /// <returns></returns>
        public INodeClientBuilder ConfigSerializer(ISerializer serializer)
        {
            CheckIsBuild();

            this.serializer = serializer;

            return this;
        }

        /// <summary>
        /// 配置协议工厂
        /// </summary>
        /// <param name="protocolStackFactory">协议工厂</param>
        /// <returns></returns>
        public INodeClientBuilder ConfigProtocolStackFactory(IProtocolStackFactory protocolStackFactory)
        {
            CheckIsBuild();

            this.protocolStackFactory = protocolStackFactory;

            return this;
        }

        /// <summary>
        /// 配置登录处理器
        /// </summary>
        /// <param name="loginHandler">登录处理器</param>
        /// <returns></returns>
        public INodeClientBuilder ConfigLoginHandler(ILoginHandler loginHandler)
        {
            CheckIsBuild();

            this.loginHandler = loginHandler;

            return this;
        }

        /// <summary>
        /// 配置客户端工厂
        /// </summary>
        /// <param name="nodeClientFactory"></param>
        /// <returns></returns>
        public INodeClientBuilder ConfigNodeClientFactory(Func<NodeClientParameters, INodeClient> nodeClientFactory)
        {
            CheckIsBuild();

            this.nodeClientFactory = nodeClientFactory;

            return this;
        }

        /// <summary>
        /// 配置底层客户端通信组件工厂
        /// </summary>
        /// <param name="communicationFactory"></param>
        /// <returns></returns>
        public INodeClientBuilder ConfigCommunicationFactory(Func<ConnectionInfo, IClient> communicationFactory)
        {
            CheckIsBuild();

            this.communicationFactory = communicationFactory;

            return this;
        }

        /// <summary>
        /// 构造XNode客户端实例集合
        /// </summary>
        /// <returns></returns>
        public IList<INodeClient> Build()
        {
            BuildConfig();

            var nodeClientList = BuildNodeClient();

            isBuild = true;

            return nodeClientList;
        }

        #region 私有方法

        private void CheckIsBuild()
        {
            if (isBuild)
            {
                throw new InvalidOperationException("XNode client has built.");
            }
        }

        private void BuildConfig()
        {
            CheckIsBuild();

            if (connectionInfos == null)
            {
                throw new InvalidOperationException("Connections is not config.");
            }

            if (serializer == null)
            {
                throw new InvalidOperationException("Serializer is not config.");
            }

            if (communicationFactory == null)
            {
                throw new InvalidOperationException("CommunicationFactory is not config.");
            }

            if (protocolStackFactory == null)
            {
                protocolStackFactory = new DefaultProtocolStackFactory();
            }

            if (loginHandler == null)
            {
                loginHandler = new DefaultLoginHandler(null, serializer, LoggerManager.ClientLoggerFactory);
            }
        }

        private IList<INodeClient> BuildNodeClient()
        {
            var nodeClientList = new List<INodeClient>();
            var nodeClientParametersList = NodeClientParameters.Create(connectionInfos, serializer, communicationFactory, loginHandler, protocolStackFactory);

            foreach (var parameters in nodeClientParametersList)
            {
                var nodeClient = nodeClientFactory == null ? new DefaultNodeClient(parameters) : nodeClientFactory(parameters);

                if (nodeClient == null)
                {
                    throw new InvalidOperationException("Load XNode client instance failed.");
                }

                nodeClientList.Add(nodeClient);
            }

            return nodeClientList;
        }

        #endregion
    }
}
