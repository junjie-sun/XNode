using System;
using System.Collections.Generic;
using System.Text;
using XNode.Client;
using XNode.Client.Configuration;
using XNode.Security;

namespace XNode.ServiceDiscovery.Zookeeper
{
    /// <summary>
    /// 客户端服务发现配置类
    /// </summary>
    public class ZookeeperClientConfig
    {
        /// <summary>
        /// 安全配置
        /// </summary>
        public List<ClientSecurityConfig> Security { get; set; }

        /// <summary>
        /// 意外关闭处理策略配置
        /// </summary>
        public List<ClientPassiveClosedStrategy> PassiveClosedStrategy { get; set; }

        /// <summary>
        /// 服务配置
        /// </summary>
        public List<ServiceInfo> Services { get; set; }
    }

    /// <summary>
    /// 客户端安全配置
    /// </summary>
    public class ClientSecurityConfig
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 默认登录验证配置
        /// </summary>
        public DefaultLoginHandlerConfig Config { get; set; }
    }

    /// <summary>
    /// 客户端意外关闭处理策略配置
    /// </summary>
    public class ClientPassiveClosedStrategy
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 默认处理策略配置
        /// </summary>
        public DefaultPassiveClosedStrategyConfig Config { get; set; }
    }
}
