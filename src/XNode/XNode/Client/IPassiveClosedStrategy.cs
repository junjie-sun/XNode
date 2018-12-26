// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Communication;

namespace XNode.Client
{
    /// <summary>
    /// 客户端被动关闭处理策略接口
    /// </summary>
    public interface IPassiveClosedStrategy
    {
        /// <summary>
        /// 客户端被动关闭处理
        /// </summary>
        /// <param name="client">客户端通信对象</param>
        /// <returns></returns>
        Task Handle(IClient client);
    }
}
