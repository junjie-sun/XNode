// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Communication
{
    /// <summary>
    /// 登录验证异常
    /// </summary>
    public class LoginAuthException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loginResult">登录验证结果</param>
        /// <param name="message">异常信息</param>
        public LoginAuthException(byte loginResult, string message) : base(message)
        {
            LoginResult = loginResult;
        }

        /// <summary>
        /// 登录验证结果
        /// </summary>
        public byte LoginResult { get; }
    }
}
