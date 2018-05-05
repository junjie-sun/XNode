// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Client
{
    /// <summary>
    /// Action代理
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ActionProxyAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="actionId">ActionId</param>
        /// <param name="enabled">是否启用Action</param>
        /// <param name="timeout">服务调用超时时长（毫秒）</param>
        public ActionProxyAttribute(int actionId, bool enabled = true, int timeout = 30000)
        {
            ActionId = actionId;
            Enabled = enabled;
            Timeout = timeout;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">Action名称</param>
        /// <param name="actionId">ActionId</param>
        /// <param name="enabled">是否启用Action</param>
        /// <param name="timeout">服务调用超时时长（毫秒）</param>
        public ActionProxyAttribute(string name, int actionId, bool enabled = true, int timeout = 30000) : this(actionId, enabled, timeout)
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
        /// 服务调用超时时长（毫秒）
        /// </summary>
        public int Timeout { get; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; }
    }
}
