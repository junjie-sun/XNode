// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Client
{
    /// <summary>
    /// NodeClient容器接口
    /// </summary>
    public interface INodeClientContainer
    {
        /// <summary>
        /// 代理名称
        /// </summary>
        string ProxyName { get; set; }

        /// <summary>
        /// 容器中包含的NodeClient数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 向容器中添加NodeClient对象
        /// </summary>
        /// <param name="nodeClient"></param>
        void Add(INodeClient nodeClient);

        /// <summary>
        /// 从容器中移除指定NodeClient对象
        /// </summary>
        /// <param name="host">客户端地址</param>
        /// <param name="port">客户端端口</param>
        void Remove(string host, int port);

        /// <summary>
        /// 获取可用的NodeClient对象
        /// </summary>
        /// <param name="serviceId">服务Id</param>
        /// <param name="actionId">ActionId</param>
        /// <param name="paramList">Action参数列表</param>
        /// <param name="returnType">Action返回类型</param>
        /// <param name="Attachments">服务调用附加数据</param>
        INodeClient Get(int serviceId, int actionId, object[] paramList, Type returnType, IDictionary<string, byte[]> Attachments);

        /// <summary>
        /// 获取容器中所有NodeClient对象
        /// </summary>
        /// <returns></returns>
        IList<INodeClient> GetAll();

        /// <summary>
        /// 为容器中所有NodeClient执行连接操作
        /// </summary>
        /// <returns></returns>
        Task ConnectAsync();

        /// <summary>
        /// 关闭中容器中所有NodeClient连接
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();
    }
}
