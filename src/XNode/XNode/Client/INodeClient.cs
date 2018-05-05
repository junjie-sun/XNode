// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Serializer;
using XNode.ProtocolStack;

namespace XNode.Client
{
    /// <summary>
    /// NodeClient接口
    /// </summary>
    public interface INodeClient
    {
        /// <summary>
        /// 服务地址
        /// </summary>
        string Host { get; }

        /// <summary>
        /// 服务端口
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 本地地址
        /// </summary>
        string LocalHost { get; }

        /// <summary>
        /// 本地商端口
        /// </summary>
        int? LocalPort { get; }

        /// <summary>
        /// 序列化器
        /// </summary>
        ISerializer Serializer { get; }

        /// <summary>
        /// 协议栈工厂
        /// </summary>
        IProtocolStackFactory ProtocolStackFactory { get; }

        /// <summary>
        /// 是否与服务端连接成功
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 与服务端创建连接
        /// </summary>
        /// <returns></returns>
        Task ConnectAsync();

        /// <summary>
        /// 与服务端断开连接
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();

        /// <summary>
        /// 调用服务
        /// </summary>
        /// <param name="serviceId">服务Id</param>
        /// <param name="actionId">ActionId</param>
        /// <param name="paramList">Action参数列表</param>
        /// <param name="retureType">Action返回类型</param>
        /// <param name="timeout">Action调用超时时长（毫秒）</param>
        /// <param name="attachments">Action调用的附加数据</param>
        /// <returns></returns>
        Task<ServiceCallResult> CallServiceAsync(int serviceId, int actionId, object[] paramList, Type retureType, int timeout, IDictionary<string, byte[]> attachments);
    }
}
