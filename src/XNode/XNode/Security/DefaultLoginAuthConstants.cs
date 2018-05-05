// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Security
{
    /// <summary>
    /// 默认登录验证所使用的常量
    /// </summary>
    public static class DefaultLoginAuthConstants
    {
        /// <summary>
        /// 时间戳字典Key
        /// </summary>
        public const string TimestampKey = "timestamp";

        /// <summary>
        /// 随机字符串字典Key
        /// </summary>
        public const string NoncestrKey = "noncestr";

        /// <summary>
        /// 签名字典Key
        /// </summary>
        public const string SignatureKey = "signature";

        /// <summary>
        /// 账号名称Key
        /// </summary>
        public const string AccountNameKey = "accountName";
    }
}
