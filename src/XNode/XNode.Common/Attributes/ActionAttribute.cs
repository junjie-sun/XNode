// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Server
{
    /// <summary>
    /// 标记该特性的方法能够注册为XNode服务的Action
    /// 每个XNode服务Action需配置一个XNode服务内唯一的ActionId
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ActionAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="actionId">ActionId</param>
        /// <param name="enabled">是否启用Action</param>
        public ActionAttribute(int actionId, bool enabled = true)
        {
            ActionId = actionId;
            Enabled = enabled;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">Action名称</param>
        /// <param name="actionId">ActionId</param>
        /// <param name="enabled">是否启用Action</param>
        public ActionAttribute(string name, int actionId, bool enabled = true) : this(actionId, enabled)
        {
            Name = name;
        }

        /// <summary>
        /// Action名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// ActionId
        /// </summary>
        public int ActionId { get; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; }
    }
}
