// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace XNode.Client
{
    /// <summary>
    /// 服务代理信息
    /// </summary>
    public class ServiceProxyInfo
    {
        /// <summary>
        /// 代理名称
        /// </summary>
        public string ProxyName { get; internal protected set; }

        /// <summary>
        /// 服务Id
        /// </summary>
        public int ServiceId { get; internal protected set; }

        /// <summary>
        /// ActionId
        /// </summary>
        public int ActionId { get; internal protected set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; internal protected set; }

        /// <summary>
        /// Action名称
        /// </summary>
        public string ActionName { get; internal protected set; }

        /// <summary>
        /// 超时时长（毫秒）
        /// </summary>
        public int Timeout { get; internal protected set; }

        /// <summary>
        /// 服务代理类型
        /// </summary>
        public Type ServiceProxyType { get; internal protected set; }

        /// <summary>
        /// Action代理类型
        /// </summary>
        public MethodInfo ActionProxyType { get; internal protected set; }

        /// <summary>
        /// Action执行返回类型
        /// </summary>
        public Type ReturnType { get; internal protected set; }

        /// <summary>
        /// Action是否启用
        /// </summary>
        public bool Enabled { get; internal protected set; }
    }
}
