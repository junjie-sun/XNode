// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using XNode.Communication.ProtocolStack;
using XNode.Logging;

namespace XNode.Communication.DotNetty
{
    /// <summary>
    /// 消息编码器
    /// </summary>
    public sealed class MessageEncoder : MessageToByteEncoder<Message>
    {
        private ILogger logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        public MessageEncoder(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<MessageEncoder>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="output"></param>
        protected override void Encode(IChannelHandlerContext context, Message message, IByteBuffer output)
        {
            if (message == null || message.Header == null)
            {
                throw new Exception("The encode message is null");
            }

            logger.LogDebug($"Encode message beginning.");

            //var sendBuf = Unpooled.Buffer();
            var sendBuf = output;
            sendBuf.SetUnsignedInt(0, message.Header.CrcCode);
            sendBuf.SetWriterIndex(4);
            sendBuf.WriteInt(message.Header.Length);
            sendBuf.WriteLong(message.Header.RequestId);
            sendBuf.WriteByte((byte)message.Header.Type);
            sendBuf.WriteByte(message.Header.Priority);
            sendBuf.WriteInt(message.Header.Attachments == null ? 0 : message.Header.Attachments.Count);      //编码附件长度

            //对附件进行编码
            if (message.Header.Attachments != null)
            {
                foreach (var key in message.Header.Attachments.Keys)
                {
                    //编码Key
                    var keyArray = Encoding.UTF8.GetBytes(key);
                    sendBuf.WriteInt(keyArray.Length);              //写入Key长度
                    sendBuf.WriteBytes(keyArray);                   //写入Key本身
                    //编码Value
                    var value = message.Header.Attachments[key];
                    sendBuf.WriteInt(value.Length);                 //写入Value长度
                    sendBuf.WriteBytes(value);                      //写入Value本身
                }
            }

            //写入Body
            if (message.Body != null && message.Body.Length > 0)
            {
                sendBuf.WriteInt(message.Body.Length);      //body长度
                sendBuf.WriteBytes(message.Body);           //body本身
            }
            else
            {
                sendBuf.WriteInt(0);
            }
            sendBuf.SetInt(4, sendBuf.ReadableBytes);       //修改消息头中的Length字段

            //output.Add(sendBuf);

            logger.LogDebug($"Encode message beginning. ReadableBytes={sendBuf.ReadableBytes}");
        }
    }
}
