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

    /// <summary>
    /// 连接断开委托
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    public delegate Task InactiveDelegate(IClient client);

    /// <summary>
    /// 客服端通信接口
    /// </summary>
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
        /// 连接断开事件
        /// </summary>
        event InactiveDelegate OnInactive;

        /// <summary>
        /// 客户端状态
        /// </summary>
        ClientStatus Status { get; }

        /// <summary>
        /// 服务地址
        /// </summary>
        string Host { get; }

        /// <summary>
        /// 服务端口
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 本地地址
        /// </summary>
        string LocalHost { get; }

        /// <summary>
        /// 本地端口
        /// </summary>
        int? LocalPort { get; }

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
        /// 主动关闭
        /// </summary>
        Closed = 1,

        /// <summary>
        /// 连接中
        /// </summary>
        Connecting = 2,

        /// <summary>
        /// 已连接
        /// </summary>
        Connected = 3,

        /// <summary>
        /// 被动关闭
        /// </summary>
        PassiveClosed = 4
    }
}
