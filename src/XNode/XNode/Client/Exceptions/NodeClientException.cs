// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Client
{
    /// <summary>
    /// NodeClient异常
    /// </summary>
    public class NodeClientException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">异常信息</param>
        public NodeClientException(string message) : base(message) { }
    }
}
