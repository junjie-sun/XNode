// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Client.Configuration
{
    /// <summary>
    /// 服务配置
    /// </summary>
    public class ServiceInfo
    {
        /// <summary>
        /// 服务Id
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 服务对应的代理类或接口全名
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 当前服务下的所有Action是否启用
        /// 配置中设置的Enabled优先级大于Attribute中的设置
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Action列表
        /// </summary>
        public List<ActionInfo> Actions { get; set; }
    }
}
