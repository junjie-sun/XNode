// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Security
{
    public class LoginAuthResult
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
}
