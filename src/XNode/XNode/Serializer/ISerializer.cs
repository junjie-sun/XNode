// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Serializer
{
    /// <summary>
    /// 序列化器接口
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// 序列化器名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 执行序列化操作
        /// </summary>
        /// <param name="obj">序列化操作的对象</param>
        /// <returns></returns>
        Task<byte[]> SerializeAsync(object obj);

        /// <summary>
        /// 执行反序列化操作
        /// </summary>
        /// <param name="type">反序列化的目标类型</param>
        /// <param name="data">反序列化操作的二进制数据</param>
        /// <returns></returns>
        Task<object> DeserializeAsync(Type type, byte[] data);
    }
}
