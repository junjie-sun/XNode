// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Communication.ProtocolStack
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType : byte
    {
        /// <summary>
        /// 0：业务请求消息
        /// </summary>
        SERVICE_REQ = 0,

        /// <summary>
        /// 1：业务响应消息
        /// </summary>
        SERVICE_RESP = 1,

        /// <summary>
        /// 2：业务ONE WAY消息（既是请求又是响应消息）
        /// </summary>
        ONE_WAY = 2,

        /// <summary>
        /// 3：握手请求消息
        /// </summary>
        LOGIN_REQ = 3,

        /// <summary>
        /// 4：握手应答消息
        /// </summary>
        LOGIN_RESP = 4,

        /// <summary>
        /// 5：心跳请求消息
        /// </summary>
        HEARTBEAT_REQ = 5,

        /// <summary>
        /// 6：心跳应答消息
        /// </summary>
        HEARTBEAT_RESP = 6
    }
}
