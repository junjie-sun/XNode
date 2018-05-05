// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Communication;
using XNode.Logging;
using XNode.Security;
using XNode.Serializer;
using XNode.ProtocolStack;

namespace XNode.Client
{
    /// <summary>
    /// NodeClient默认实现
    /// </summary>
    public class DefaultNodeClient : INodeClient
    {
        private ILogger logger;

        private IClient client;

        private ILoginHandler loginHandler;

        /// <summary>
        /// 服务地址
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// 本地地址
        /// </summary>
        public string LocalHost { get; }

        /// <summary>
        /// 本地商端口
        /// </summary>
        public int? LocalPort { get; }

        /// <summary>
        /// 序列化器
        /// </summary>
        public ISerializer Serializer { get; }

        /// <summary>
        /// 协议栈工厂
        /// </summary>
        public IProtocolStackFactory ProtocolStackFactory { get; }

        /// <summary>
        /// 是否与服务端连接成功
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return client.Status == ClientStatus.Connected;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="parameters">NodeClient参数信息</param>
        public DefaultNodeClient(NodeClientParameters parameters)
        {
            if (parameters == null)
            {
                throw new NodeClientException("NodeClientConfig is null.");
            }

            logger = LoggerManager.ClientLoggerFactory.CreateLogger<DefaultNodeClient>();
            Host = parameters.Host;
            Port = parameters.Port;
            LocalHost = parameters.LocalHost;
            LocalPort = parameters.LocalPort;
            Serializer = parameters.Serializer ?? throw new NodeClientException("Serializer is null.");
            ProtocolStackFactory = parameters.ProtocolStackFactory ?? new DefaultProtocolStackFactory();
            loginHandler = parameters.LoginHandler ?? new DefaultLoginHandler(null, Serializer, LoggerManager.ClientLoggerFactory);
            client = parameters.Communication;

            if (loginHandler != null)
            {
                client.OnSubmitLoginRequest += new SubmitLoginRequestDelegate(async () =>
                {
                    var loginInfo = await loginHandler.GetLoginInfo();
                    return new LoginRequestData()
                    {
                        Body = loginInfo.Body,
                        Attachments = loginInfo.Attachments
                    };
                });

                client.OnRecieveLoginResponse += new RecieveLoginResponseDelegate((body, attachments) =>
                {
                    return loginHandler.LoginResponseHandle(new LoginResponseInfo()
                    {
                        Body = body,
                        Attachments = attachments
                    });
                });
            }
        }

        /// <summary>
        /// 与服务端创建连接
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            logger.LogInformation($"NodeClient start connect. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}");
            try
            {
                await client.ConnectAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"NodeClient connect has error. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}, ExceptionMessage={ex.Message}");
                throw ex;
            }
            logger.LogInformation($"NodeClient connected. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}");
        }

        /// <summary>
        /// 与服务端断开连接
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            logger.LogInformation($"NodeClient start close. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}");
            try
            {
                await client.CloseAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"NodeClient close has error. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}, ExceptionMessage={ex.Message}");
                throw ex;
            }

            logger.LogInformation($"NodeClient closed. Host={Host}, Port={Port}, LocalHost={LocalHost}, LocalPort={LocalPort}");
        }

        /// <summary>
        /// 调用服务
        /// </summary>
        /// <param name="serviceId">服务Id</param>
        /// <param name="actionId">ActionId</param>
        /// <param name="paramList">Action参数列表</param>
        /// <param name="returnType">Action返回类型</param>
        /// <param name="timeout">Action调用超时时长（毫秒）</param>
        /// <param name="attachments">Action调用的附加数据</param>
        /// <returns></returns>
        public async Task<ServiceCallResult> CallServiceAsync(int serviceId, int actionId, object[] paramList, Type returnType, int timeout, IDictionary<string, byte[]> attachments)
        {
            logger.LogDebug($"Call service beginning. Host={Host}, Port={Port}, ServiceId={serviceId}, ActionId={actionId}");

            logger.LogDebug($"Service detail info. Host={Host}, Port={Port}, ServiceId={serviceId}, ActionId={actionId}, ParamList={(paramList == null || paramList.Length == 0 ? string.Empty : string.Join("|", paramList))}");

            var request = await CreateServiceRequestAsync(serviceId, actionId, paramList);

            RequestResult result = null;

            byte[] requestBytes = null;

            try
            {
                requestBytes = await Serializer.SerializeAsync(request);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"NodeClient serialize request data has error. Host={Host}, Port={Port}, ServiceId={serviceId}, ActionId={actionId}, ExceptionMessage={ex.Message}");
                throw ex;
            }

            try
            {
                logger.LogDebug($"Send request data. Host={Host}, Port={Port}, ServiceId={serviceId}, ActionId={actionId}");
                result = await client.SendAsync(requestBytes, timeout, attachments);
            }
            catch (RequestTimeoutExcption ex)
            {
                logger.LogError(ex, $"NodeClient call service timeout. Host={Host}, Port={Port}, ServiceId={serviceId}, ActionId={actionId}, RequestId={ex.Request.Id}");
                throw ex;
            }
            catch (NetworkException ex)
            {
                logger.LogError(ex, $"NodeClient call service has network error. Host={Host}, Port={Port}, ServiceId={serviceId}, ActionId={actionId}, ExceptionMessage={ex.Message}");
                throw ex;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"NodeClient call service has error. Host={Host}, Port={Port}, ServiceId={serviceId}, ActionId={actionId}, ExceptionMessage={ex.Message}");
                throw ex;
            }

            IServiceResponse response = null;

            try
            {
                logger.LogDebug($"NodeClient deserialize response data. Host={Host}, Port={Port}, ServiceId={serviceId}, ActionId={actionId}");
                response = (IServiceResponse)await Serializer.DeserializeAsync(ProtocolStackFactory.ServiceResponseType, result.Data);
                if (response == null)
                {
                    response = ProtocolStackFactory.CreateServiceResponse();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"NodeClient deserialize response data has error: Host={Host}, Port={Port}, ServiceId={serviceId}, ActionId={actionId}, ExceptionMessage={ex.Message}");
                throw ex;
            }
            
            if (response.HasException)
            {
                logger.LogError($"Node server has an error, Host={Host}, Port={Port}, ServiceId={serviceId}, ActionId={actionId}, ExceptionId={response.ExceptionId}, ExceptionMessage={response.ExceptionMessage}");
                throw new ServiceCallException(serviceId, actionId, response.ExceptionId, response.ExceptionMessage);
            }
            else
            {
                var serviceCallResult = new ServiceCallResult()
                {
                    Attachments = result.Attachments
                };
                if (returnType != null && returnType != typeof(void))
                {
                    try
                    {
                        logger.LogDebug($"NodeClient deserialize return value. Host={Host}, Port={Port}, ServiceId={serviceId}, ActionId={actionId}");
                        serviceCallResult.ReturnVal = await Serializer.DeserializeAsync(returnType, response.ReturnValue);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"NodeClient deserialize return value has error. Host={Host}, Port={Port}, ServiceId={serviceId}, ActionId={actionId}, ExceptionMessage={ex.Message}");
                        throw ex;
                    }
                }
                logger.LogDebug($"Call service finished. Host={Host}, Port={Port}, ServiceId={serviceId}, ActionId={actionId}");
                return serviceCallResult;
            }
        }

        private async Task<IServiceRequest> CreateServiceRequestAsync(int serviceId, int actionId, object[] paramList)
        {
            var request = ProtocolStackFactory.CreateServiceRequest();
            request.ServiceId = serviceId;
            request.ActionId = actionId;
            request.ParamList = new List<byte[]>();

            if (paramList != null)
            {
                foreach (var param in paramList)
                {
                    try
                    {
                        request.ParamList.Add(await Serializer.SerializeAsync(param));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"NodeClient serialize parameters has error. Host={Host}, Port={Port}, ServiceId={serviceId}, ActionId={actionId}, ExceptionMessage={ex.Message}");
                        throw ex;
                    }
                }
            }

            return request;
        }
    }
}
