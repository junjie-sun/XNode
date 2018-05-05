// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Server
{
    /// <summary>
    /// 标记该特性的类或接口能够注册为XNode服务
    /// 每个XNode服务类需配置一个全局唯一的ServiceId
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ServiceAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceId">服务Id</param>
        /// <param name="enabled">是否启用服务代理，如果为false则该服务下所有Action将停用</param>
        public ServiceAttribute(int serviceId, bool enabled = false)
        {
            ServiceId = serviceId;
            Enabled = enabled;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">服务名称</param>
        /// <param name="serviceId">服务Id</param>
        /// <param name="enabled">是否启用服务代理，如果为false则该服务下所有Action将停用</param>
        public ServiceAttribute(string name, int serviceId, bool enabled = false) : this(serviceId, enabled)
        {
            Name = name;
        }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 服务Id
        /// </summary>
        public int ServiceId { get; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; }
    }
}
