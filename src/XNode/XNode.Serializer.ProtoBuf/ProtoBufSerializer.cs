// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XNode.Logging;
using ProtoBufNet = ProtoBuf;

namespace XNode.Serializer.ProtoBuf
{
    /// <summary>
    /// 基于ProtoBuf的序列化器
    /// </summary>
    public class ProtoBufSerializer : ISerializer
    {
        private ILogger logger;

        public string Name
        {
            get
            {
                return "ProtoBuf";
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggerFactory">日志工厂</param>
        public ProtoBufSerializer(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<ProtoBufSerializer>();
        }

        public Task<object> DeserializeAsync(Type type, byte[] data)
        {
            if (type == null)
            {
                throw new InvalidOperationException("无法进行反序列化操作，type不能为null");
            }

            if (data == null || data.Length == 0)
            {
                logger.LogDebug($"Deserialize data is null.");
                return Task.FromResult<object>(null);
            }

            logger.LogDebug($"Do deserialize.");
            object result;
            using (var stream = new MemoryStream(data))
            {
                result = ProtoBufNet.Serializer.Deserialize(type, stream);
            }

            return Task.FromResult(result);
        }

        public Task<byte[]> SerializeAsync(object obj)
        {
            if (obj == null)
            {
                logger.LogDebug($"Serialize data is null.");
                return Task.FromResult(new byte[0]);
            }

            logger.LogDebug($"Do serialize.");
            byte[] result;
            using (var stream = new MemoryStream())
            {
                ProtoBufNet.Serializer.Serialize(stream, obj);
                result = stream.ToArray();
            }

            return Task.FromResult(result);
        }
    }
}
