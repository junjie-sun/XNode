// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace XNode.Communication.DotNetty
{
    /// <summary>
    /// 每个连接独立的状态数据
    /// </summary>
    public static class ChannelState
    {
        #region LoginState

        private static AttributeKey<LoginState> LoginStateKey = AttributeKey<LoginState>.NewInstance("LoginStateKey");

        /// <summary>
        /// 获取登录状态
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static LoginState GetLoginState(IChannelHandlerContext context)
        {
            return context.GetAttribute<LoginState>(LoginStateKey).Get();
        }

        /// <summary>
        /// 设置登录状态
        /// </summary>
        /// <param name="context"></param>
        /// <param name="isLoginSuccess"></param>
        public static void SetLoginState(IChannelHandlerContext context, bool isLoginSuccess)
        {
            context.GetAttribute<LoginState>(LoginStateKey).Set(new LoginState() { IsLoginSuccess = isLoginSuccess });
        }

        /// <summary>
        /// 移除登录状态
        /// </summary>
        /// <param name="context"></param>
        public static void RemoveLoginState(IChannelHandlerContext context)
        {
            context.GetAttribute<LoginState>(LoginStateKey).Remove();
        }

        #endregion
    }
}
