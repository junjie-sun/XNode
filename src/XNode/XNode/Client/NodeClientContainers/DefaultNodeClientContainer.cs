// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace XNode.Client.NodeClientContainers
{
    /// <summary>
    /// 默认NodeClient容器
    /// </summary>
    public class DefaultNodeClientContainer : NodeClientContainerBase
    {
        private int nextIndex = 0;

        /// <summary>
        /// 轮循获取可用的NodeClient对象
        /// </summary>
        /// <param name="serviceId">服务Id</param>
        /// <param name="actionId">ActionId</param>
        /// <param name="paramList">Action参数列表</param>
        /// <param name="returnType">Action返回类型</param>
        /// <param name="Attachments">服务调用附加数据</param>
        /// <returns></returns>
        public override INodeClient Get(int serviceId, int actionId, object[] paramList, Type returnType, IDictionary<string, byte[]> Attachments)
        {
            var nodeClientList = NodeClientList;

            if (nodeClientList.Count == 0)
            {
                return null;
            }

            int i, j = nextIndex;

            for (var cnt = 0; cnt < nodeClientList.Count; cnt++)
            {
                do
                {
                    i = j;
                    j = Interlocked.CompareExchange(ref nextIndex, (i < nodeClientList.Count - 1) ? (i + 1) : 0, i);
                } while (i != j);

                var client = nodeClientList[j];
                if (client.IsConnected)
                {
                    return client;
                }
            }

            return null;
        }
    }
}
