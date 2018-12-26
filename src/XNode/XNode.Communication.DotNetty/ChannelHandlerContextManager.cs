// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Communication.DotNetty
{
    /// <summary>
    /// ChannelHandlerContext管理器
    /// </summary>
    public class ChannelHandlerContextManager
    {
        private IList<IChannelHandlerContext> channelHandlerContexts = new List<IChannelHandlerContext>();

        private readonly object lockObj = new object();

        /// <summary>
        /// 注册ChannelHandlerContext
        /// </summary>
        /// <param name="context"></param>
        public void Regist(IChannelHandlerContext context)
        {
            lock (lockObj)
            {
                if (!channelHandlerContexts.Contains(context))
                {
                    channelHandlerContexts.Add(context);
                }
            }
        }

        /// <summary>
        /// 注销ChannelHandlerContext
        /// </summary>
        /// <param name="context"></param>
        public void Unregist(IChannelHandlerContext context)
        {
            lock (lockObj)
            {
                if (channelHandlerContexts.Contains(context))
                {
                    channelHandlerContexts.Remove(context);
                }
            }
        }

        /// <summary>
        /// 关闭所有注册的ChannelHandlerContext
        /// </summary>
        /// <returns></returns>
        public Task CloseAllAsync()
        {
            var taskList = new List<Task>();

            lock (lockObj)
            {
                if (channelHandlerContexts.Count > 0)
                {
                    foreach (var context in channelHandlerContexts)
                    {
                        taskList.Add(context.CloseAsync());
                    }
                }
            }

            if (taskList.Count > 0)
            {
                return Task.WhenAll(taskList);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }
}
