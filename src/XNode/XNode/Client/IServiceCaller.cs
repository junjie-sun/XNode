// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Client
{
    /// <summary>
    /// 以管线方式调用服务
    /// </summary>
    public interface IServiceCaller
    {
        /// <summary>
        /// 下一个服务调用器
        /// </summary>
        IServiceCaller Next { get; set; }

        /// <summary>
        /// 服务调用
        /// </summary>
        /// <param name="nodeClientContainer">NodeClient容器</param>
        /// <param name="info">服务调用信息</param>
        /// <returns></returns>
        Task<ServiceCallResult> CallAsync(INodeClientContainer nodeClientContainer, ServiceCallInfo info);
    }

    /// <summary>
    /// 服务调用信息
    /// </summary>
    public class ServiceCallInfo
    {
        /// <summary>
        /// 代理名称
        /// </summary>
        public string ProxyName { get; set; }

        /// <summary>
        /// 服务Id
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// ActionId
        /// </summary>
        public int ActionId { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Action名称
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// Action调用参数列表
        /// </summary>
        public object[] ParamList { get; set; }

        /// <summary>
        /// Action调用返回类型
        /// </summary>
        public Type ReturnType { get; set; }

        /// <summary>
        /// Action调用超时时长（毫秒）
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Action调用附加数据
        /// </summary>
        public IDictionary<string, byte[]> Attachments { get; set; }
    }
}
