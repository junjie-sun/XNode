using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace XNode.Communication
{
    /// <summary>
    /// 登录状态
    /// </summary>
    public class LoginState
    {
        /// <summary>
        /// 是否登录成功
        /// </summary>
        public bool IsLoginSuccess { get; set; }

        /// <summary>
        /// 身份标识
        /// </summary>
        public string Identity { get; set; }

        /// <summary>
        /// 客户端地址
        /// </summary>
        public IPEndPoint RemoteAddress { get; set; }
    }
}
