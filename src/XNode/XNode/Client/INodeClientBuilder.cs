using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using XNode.Client.Configuration;
using XNode.Communication;
using XNode.ProtocolStack;
using XNode.Security;
using XNode.Serializer;

namespace XNode.Client
{
    /// <summary>
    /// XNode客户端构造接口
    /// </summary>
    public interface INodeClientBuilder
    {
        /// <summary>
        /// 配置客户端连接
        /// </summary>
        /// <param name="connectionInfos">客户端连接信息</param>
        /// <returns></returns>
        INodeClientBuilder ConfigConnections(IList<ConnectionInfo> connectionInfos);

        /// <summary>
        /// 配置序列化器
        /// </summary>
        /// <param name="serializer">序列化器</param>
        /// <returns></returns>
        INodeClientBuilder ConfigSerializer(ISerializer serializer);

        /// <summary>
        /// 配置协议工厂
        /// </summary>
        /// <param name="protocolStackFactory">协议工厂</param>
        /// <returns></returns>
        INodeClientBuilder ConfigProtocolStackFactory(IProtocolStackFactory protocolStackFactory);

        /// <summary>
        /// 配置登录处理器
        /// </summary>
        /// <param name="loginHandler">登录处理器</param>
        /// <returns></returns>
        INodeClientBuilder ConfigLoginHandler(ILoginHandler loginHandler);

        /// <summary>
        /// 配置客户端工厂
        /// </summary>
        /// <param name="nodeClientFactory"></param>
        /// <returns></returns>
        INodeClientBuilder ConfigNodeClientFactory(Func<NodeClientParameters, INodeClient> nodeClientFactory);

        /// <summary>
        /// 配置底层客户端通信组件工厂
        /// </summary>
        /// <param name="communicationFactory"></param>
        /// <returns></returns>
        INodeClientBuilder ConfigCommunicationFactory(Func<ConnectionInfo, IClient> communicationFactory);

        /// <summary>
        /// 构造XNode客户端实例集合
        /// </summary>
        /// <returns></returns>
        IList<INodeClient> Build();
    }
}
