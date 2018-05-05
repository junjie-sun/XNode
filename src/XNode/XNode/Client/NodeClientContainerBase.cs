// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Client
{
    /// <summary>
    /// NodeClient容器基类
    /// </summary>
    public abstract class NodeClientContainerBase : INodeClientContainer
    {
        private object nodeClientListLockObj = new object();

        protected IList<INodeClient> NodeClientList { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public NodeClientContainerBase()
        {
            NodeClientList = new List<INodeClient>();
        }

        /// <summary>
        /// 代理名称
        /// </summary>
        public virtual string ProxyName { get; set; }

        /// <summary>
        /// 容器中包含的NodeClient数量
        /// </summary>
        public virtual int Count => NodeClientList.Count;

        /// <summary>
        /// 向容器中添加NodeClient对象
        /// </summary>
        /// <param name="nodeClient"></param>
        public virtual void Add(INodeClient nodeClient)
        {
            lock (nodeClientListLockObj)
            {
                NodeClientList.Add(nodeClient);
            }
        }

        /// <summary>
        /// 从容器中移除指定NodeClient对象
        /// </summary>
        /// <param name="host">客户端地址</param>
        /// <param name="port">客户端端口</param>
        public virtual void Remove(string host, int port)
        {
            lock (nodeClientListLockObj)
            {
                var isRemove = false;
                var list = new List<INodeClient>();
                foreach (var client in NodeClientList)
                {
                    if (client.Host == host && client.Port == port)
                    {
                        client.CloseAsync().Wait();
                        isRemove = true;
                        continue;
                    }
                    list.Add(client);
                }
                if (isRemove)
                {
                    NodeClientList = list;
                }
            }
        }

        /// <summary>
        /// 关闭中容器中所有NodeClient连接
        /// </summary>
        /// <returns></returns>
        public virtual Task CloseAsync()
        {
            var taskList = new List<Task>();

            foreach (var client in NodeClientList)
            {
                taskList.Add(client.CloseAsync());
            }

            return Task.WhenAll((taskList));
        }

        /// <summary>
        /// 为容器中所有NodeClient执行连接操作
        /// </summary>
        /// <returns></returns>
        public virtual Task ConnectAsync()
        {
            var taskList = new List<Task>();

            foreach (var client in NodeClientList)
            {
                taskList.Add(client.ConnectAsync());
            }

            return Task.WhenAll(taskList).ContinueWith((t) =>
            {
                if (t.Exception != null && t.Exception.InnerExceptions.Count == taskList.Count)
                {
                    throw t.Exception.InnerException;
                }
            });
        }

        /// <summary>
        /// 获取NodeClient对象
        /// </summary>
        /// <param name="serviceId">服务Id</param>
        /// <param name="actionId">ActionId</param>
        /// <param name="paramList">Action参数列表</param>
        /// <param name="returnType">Action返回类型</param>
        /// <param name="Attachments">服务调用附加数据</param>
        public abstract INodeClient Get(int serviceId, int actionId, object[] paramList, Type returnType, IDictionary<string, byte[]> Attachments);

        /// <summary>
        /// 获取容器中所有NodeClient对象
        /// </summary>
        /// <returns></returns>
        public virtual IList<INodeClient> GetAll()
        {
            return NodeClientList;
        }
    }
}
