// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Serializer;

namespace XNode.Security
{
    /// <summary>
    /// 登录验证接口
    /// </summary>
    public interface ILoginValidator
    {
        /// <summary>
        /// 序列化器
        /// </summary>
        ISerializer Serializer { set; }

        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="loginInfo">登录信息</param>
        /// <returns></returns>
        Task<LoginAuthResult> Validate(LoginRequestInfo loginInfo);
    }
}
