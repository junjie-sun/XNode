// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Communication.ProtocolStack
{
    /// <summary>
    /// 消息定义
    /// </summary>
    public sealed class Message
    {
        /// <summary>
        /// 消息头
        /// </summary>
        public MessageHeader Header { get; set; }

        /// <summary>
        /// 消息体
        /// </summary>
        public byte[] Body { get; set; }

        public override string ToString()
        {
            return $"Message [{Header}]";
        }
    }
}
