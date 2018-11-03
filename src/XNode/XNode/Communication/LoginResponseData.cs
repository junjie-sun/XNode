// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Communication
{
    /// <summary>
    /// 登录响应数据
    /// </summary>
    public class LoginResponseData
    {
        /// <summary>
        /// 登录验证的身份标识
        /// </summary>
        public string AuthIdentity { get; set; }

        /// <summary>
        /// 登录验证是否成功
        /// </summary>
        public bool AuthResult { get; set; }

        /// <summary>
        /// 登录验证状态码（非0表示验证失败，1-30为XNode保留状态码）
        /// </summary>
        public byte AuthStatusCode { get; set; }

        /// <summary>
        /// 登录验证失败错误信息
        /// </summary>
        public string AuthFailedMessage { get; set; }

        /// <summary>
        /// 附加数据
        /// </summary>
        public IDictionary<string, byte[]> Attachments { get; set; }
    }

    /// <summary>
    /// XNode保留登录验证状态码枚举
    /// </summary>
    public enum AuthStatusCodes : byte
    {
        /// <summary>
        /// 未配置登录验证Handler
        /// </summary>
        LoginRecieveHandlerIsNull = 1,

        /// <summary>
        /// 调用登录验证Handler返回的LoginResponseData对象为Null
        /// </summary>
        LoginResponseDataIsNull = 2,

        /// <summary>
        /// 未登录
        /// </summary>
        NoLogin = 3,

        /// <summary>
        /// 等待登录响应超时
        /// </summary>
        WaitLoginResponseTimeout = 4,

        /// <summary>
        /// 解析登录响应数据时发生错误
        /// </summary>
        ParseLoginResponseDataError = 5
    }
}
