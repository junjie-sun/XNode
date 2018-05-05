// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Security
{
    /// <summary>
    /// 登录响应信息
    /// </summary>
    public class LoginResponseInfo
    {
        /// <summary>
        /// 服务端回复的主体数据
        /// </summary>
        public byte[] Body { get; set; }

        /// <summary>
        /// 服务端回复的附加数据
        /// </summary>
        public IDictionary<string, byte[]> Attachments { get; set; }
    }
}
