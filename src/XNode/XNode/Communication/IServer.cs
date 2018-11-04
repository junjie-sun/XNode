using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Communication
{
    /// <summary>
    /// 接收登录请求委托
    /// </summary>
    /// <returns></returns>
    public delegate Task<LoginResponseData> RecieveLoginRequestDelegate(LoginAuthInfo loginAuthInfo);

    /// <summary>
    /// 接收服务请求委托
    /// </summary>
    /// <returns></returns>
    public delegate Task<ResponseData> RecieveServiceRequestDelegate(byte[] message, IDictionary<string, byte[]> attachments, LoginState loginState);

    /// <summary>
    /// 服务端通信接口
    /// </summary>
    public interface IServer
    {
        /// <summary>
        /// 接收登录请求事件
        /// </summary>
        event RecieveLoginRequestDelegate OnRecieveLoginRequest;

        /// <summary>
        /// 接收服务请求事件
        /// </summary>
        event RecieveServiceRequestDelegate OnRecieveServiceRequest;

        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        /// 关闭服务器
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();
    }
}
