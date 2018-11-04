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
    /// 消息解码器
    /// </summary>
    public class MessageDecoder : LengthFieldBasedFrameDecoder
    {
        private ILogger logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        /// <param name="maxFrameLength">最大允许消息长度</param>
        /// <param name="lengthFieldOffset">消息中的长度字段起始位置（从0开始计算，本例第4位为长度字段）</param>
        /// <param name="lengthFieldLength">消息中的长度字段的长度，本例长度字段为4个字节</param>
        /// <param name="lengthAdjustment">长度字段中的值表示长度字段之后需要读取的字节长度值，例如lengthAdjustment=12表示长度字段之后需要读取12个字节，本例长度字段的值为消息头与消息体的总长度（包括CrcCode与Length字段），因此读取长度应该调整为-8（长度字段之后需要读取的字节数=总长度[长度字段值]-4[CrcCode字段]-4[Length字段]）</param>
        /// <param name="initialBytesToStrip">启始读取字节，本例所有字段都需要读取，因此为0</param>
        public MessageDecoder(ILoggerFactory loggerFactory, int maxFrameLength, int lengthFieldOffset, int lengthFieldLength, int lengthAdjustment, int initialBytesToStrip) : base(maxFrameLength, lengthFieldOffset, lengthFieldLength, lengthAdjustment, initialBytesToStrip)
        {
            logger = loggerFactory.CreateLogger<MessageDecoder>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            var frame = base.Decode(context, input);
            if (frame == null)
            {
                return;
            }

            var input2 = frame as IByteBuffer;

            logger.LogDebug($"Decode message beginning. ReadableBytes={input2.ReadableBytes}");

            var message = new Message();
            var header = new MessageHeader();
            header.CrcCode = input2.ReadUnsignedInt();
            header.Length = input2.ReadInt();
            header.RequestId = input2.ReadLong();
            header.Type = (MessageType)input2.ReadByte();
            header.Priority = input2.ReadByte();

            //解码附件
            var size = input2.ReadInt();     //附件个数
            if (size > 0)
            {
                header.Attachments = new Dictionary<string, byte[]>(size);
                for (var i = 0; i < size; i++)
                {
                    var keySize = input2.ReadInt();                  //附件Key长度
                    var keyArray = new byte[keySize];
                    input2.ReadBytes(keyArray);
                    var key = Encoding.UTF8.GetString(keyArray);    //附件Key
                    var valueLength = input2.ReadInt();              //附件Value长度
                    var value = new byte[valueLength];              //附件Value
                    input2.ReadBytes(value);
                    header.Attachments.Add(key, value);
                }
            }

            message.Header = header;

            //Body
            if (input2.ReadableBytes >= 4)
            {
                var bodyLength = input2.ReadInt();       //body长度
                var body = new byte[bodyLength];        //body
                input2.ReadBytes(body);
                message.Body = body;
            }

            output.Add(message);

            logger.LogDebug($"Decode message finished.");
        }
    }
}
