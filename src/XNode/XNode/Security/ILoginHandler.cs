// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Security
{
    /// <summary>
    /// 登录处理器
    /// </summary>
    public interface ILoginHandler
    {
        /// <summary>
        /// 获取登录信息
        /// </summary>
        /// <returns></returns>
        Task<LoginInfo> GetLoginInfo();

        /// <summary>
        /// 登录验证响应处理
        /// </summary>
        /// <param name="loginResponseInfo">登录验证响应信息</param>
        /// <returns>登录验证状态码（非0表示验证失败，1-30为XNode保留状态码）</returns>
        Task<byte> LoginResponseHandle(LoginResponseInfo loginResponseInfo);
    }
}
