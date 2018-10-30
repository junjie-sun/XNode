// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MessagePack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XNode.Logging;

namespace XNode.Serializer.MsgPack
{
    /// <summary>
    /// 基于MsgPack的序列化器
    /// </summary>
    public class MsgPackSerializer : ISerializer
    {
        private ILogger logger;

        public string Name
        {
            get
            {
                return "MsgPack";
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        public MsgPackSerializer(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<MsgPackSerializer>();
        }

        public Task<object> DeserializeAsync(Type type, byte[] data)
        {
            if (type == null)
            {
                throw new InvalidOperationException("Deserialize failed, type is null.");
            }

            if (data == null || data.Length == 0)
            {
                logger.LogDebug($"Deserialize data is null.");
                return Task.FromResult<object>(null);
            }

            logger.LogDebug($"Do deserialize.");
            return Task.FromResult(MessagePackSerializer.NonGeneric.Deserialize(type, data));
        }

        public Task<byte[]> SerializeAsync(object obj)
        {
            if (obj == null)
            {
                logger.LogDebug($"Serialize data is null.");
                return Task.FromResult<byte[]>(null);
            }

            logger.LogDebug($"Do serialize.");
            return Task.FromResult(MessagePackSerializer.NonGeneric.Serialize(obj.GetType(), obj));
        }
    }
}
