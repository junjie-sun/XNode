// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Server.Configuration
{
    /// <summary>
    /// Action配置
    /// </summary>
    public class ActionInfo
    {
        /// <summary>
        /// Action名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ActionId
        /// </summary>
        public int ActionId { get; set; }

        /// <summary>
        /// Action是否启用
        /// 配置中设置的Enabled优先级大于Attribute中的设置
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}
