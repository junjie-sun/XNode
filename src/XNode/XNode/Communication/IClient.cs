using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Communication
{
    /// <summary>
    /// 提交登录请求委托
    /// </summary>
    /// <returns></returns>
    public delegate Task<LoginRequestData> SubmitLoginRequestDelegate();

    /// <summary>
    /// 接收登录响应委托
    /// </summary>
    /// <returns>登录验证状态码（非0表示验证失败，1-30为XNode保留状态码）</returns>
    public delegate Task<byte> RecieveLoginResponseDelegate(byte[] message, IDictionary<string, byte[]> attachments);

    public interface IClient
    {
        /// <summary>
        /// 提交登录请求事件
        /// </summary>
        event SubmitLoginRequestDelegate OnSubmitLoginRequest;

        /// <summary>
        /// 接收登录响应事件
        /// </summary>
        event RecieveLoginResponseDelegate OnRecieveLoginResponse;

        /// <summary>
        /// 客户端状态
        /// </summary>
        ClientStatus Status { get; }

        /// <summary>
        /// 向服务端发起连接
        /// </summary>
        /// <returns></returns>
        Task ConnectAsync();

        /// <summary>
        /// 单向发送消息到服务器
        /// </summary>
        /// <param name="msg">消息数据</param>
        /// <param name="timeout">超时时长（毫秒）</param>
        /// <param name="attachments">消息附加数据</param>
        /// <returns></returns>
        Task SendOneWayAsync(byte[] msg, int timeout = 30000, IDictionary<string, byte[]> attachments = null);

        /// <summary>
        /// 发送消息到服务器
        /// </summary>
        /// <param name="msg">消息数据</param>
        /// <param name="timeout">超时时长（毫秒）</param>
        /// <param name="attachments">消息附加数据</param>
        /// <returns></returns>
        Task<RequestResult> SendAsync(byte[] msg, int timeout = 30000, IDictionary<string, byte[]> attachments = null);

        /// <summary>
        /// 关闭与服务端的连接
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();
    }

    /// <summary>
    /// 客户端状态
    /// </summary>
    public enum ClientStatus
    {
        /// <summary>
        /// 连接关闭
        /// </summary>
        Closed = 1,

        /// <summary>
        /// 连接中
        /// </summary>
        Connecting = 2,

        /// <summary>
        /// 已连接
        /// </summary>
        Connected = 3
    }
}
