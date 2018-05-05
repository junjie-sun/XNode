// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Communication.ProtocolStack
{
    /// <summary>
    /// 消息头
    /// </summary>
    public sealed class MessageHeader
    {
        /// <summary>
        /// 校验码（32位）
        /// 第1-2字节为固定值abef表示消息为XNode消息
        /// 第3字节表示主版本号（0-255）
        /// 第4字节表示次版本号（0-255）
        /// </summary>
        public uint CrcCode { get; set; } = 0xabef0101;

        /// <summary>
        /// 消息长度（32位）
        /// 包括消息头与消息体
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 请求ID（64位）
        /// 由请求ID生成器生成
        /// </summary>
        public long RequestId { get; set; }

        /// <summary>
        /// 消息类型（8位）
        /// 0：业务请求消息
        /// 1：业务响应消息
        /// 2：业务ONE WAY消息（既是请求又是响应消息）
        /// 3：握手请求消息
        /// 4：握手应答消息
        /// 5：心跳请求消息
        /// 6：心跳应答消息
        /// </summary>
        public MessageType Type { get; set; }

        /// <summary>
        /// 消息优先级（8位）
        /// 0-255
        /// </summary>
        public byte Priority { get; set; }

        /// <summary>
        /// 可选字段（变长）
        /// 用于扩展消息头
        /// 如果Attachment长度为0，表示没有可选附件，则将长度编码设为0，ByteBuffer.WriteInt(0)；如果大于0，说明有附件需要编码，规则如下：
        /// 首先对附件的个数进行编码，ByteBuffer.WriteInt(Attachment.Count)
        /// 然后对Key进行编码（以UTF-8转为byte数组后，ByteBuffer.WriteInt(数组长度)与ByteBuffer.WriteBytes(数组)），再将Value长度与本身写入与ByteBuffer
        /// </summary>
        public IDictionary<string, byte[]> Attachments { get; set; }

        public override string ToString()
        {
            return $"MessageHeader [CrcCode={CrcCode}, Length={Length}, RequestId={RequestId}, Type={Type}, Priority={Priority}, Attachment={Attachments}]";
        }
    }
}
