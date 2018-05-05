// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Communication
{
    /// <summary>
    /// 登录请求数据
    /// </summary>
    public class LoginRequestData
    {
        /// <summary>
        /// 客户端提交的主体数据
        /// </summary>
        public byte[] Body { get; set; }

        /// <summary>
        /// 客户端提交的附加数据
        /// </summary>
        public IDictionary<string, byte[]> Attachments { get; set; }
    }
}
