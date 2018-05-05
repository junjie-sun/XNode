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
using XNode.Server.Route;

namespace XNode.Server
{
    /// <summary>
    /// XNode服务端默认实现
    /// </summary>
    public sealed class DefaultNodeServer : INodeServer
    {
        private ILogger logger;

        private NodeServerConfig config;

        private IProtocolStackFactory protocolStackFactory;

        private IServiceProvider serviceProvider;

        private IRouteManager routeManager;

        private IRouteDescriptor routeDescriptor;

        private ISerializer serializer;

        private IServiceInvoker serviceInvoker;

        private IServiceProcessor serviceProcessor;

        private ILoginValidator loginValidator;

        private IServer server;

        /// <summary>
        /// XNode服务器启动前触发
        /// </summary>
        public event NodeServerStartDelegate OnStarting;

        /// <summary>
        /// XNode服务器启动完成后触发
        /// </summary>
        public event NodeServerStartDelegate OnStarted;

        /// <summary>
        /// XNode服务器停止前触发
        /// </summary>
        public event NodeServerStopDelegate OnStopping;

        /// <summary>
        /// XNode服务器停止完成后触发
        /// </summary>
        public event NodeServerStopDelegate OnStopped;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">NodeServer配置</param>
        public DefaultNodeServer(NodeServerConfig config)
        {
            ValidateConfig(config);
            logger = LoggerManager.ServerLoggerFactory.CreateLogger<DefaultNodeServer>();
            server = config.Communication;
            protocolStackFactory = config.ProtocolStackFactory;
            serviceProvider = config.ServiceProvider;
            routeManager = config.RouteFactory.CreateRouteManager();
            routeDescriptor = config.RouteFactory.CreateRouteDescriptor();
            if (config.ServiceConfigs != null)
            {
                routeDescriptor.SetServiceConfig(config.ServiceConfigs);
            }
            serializer = config.Serializer;
            serviceInvoker = config.ServiceInvoker;
            serviceProcessor = config.ServiceProcessor;
            loginValidator = config.LoginValidator;
            this.config = config ?? throw new InvalidOperationException("Argument config is null.");
        }

        /// <summary>
        /// 启动NodeServer
        /// </summary>
        public Task StartAsync()
        {
            logger.LogInformation("Server is loading services.");

            LoadServices();

            logger.LogInformation("Server load services success.");

            server.OnRecieveLoginRequest += new RecieveLoginRequestDelegate(async (loginAuthInfo) =>
            {
                var loginInfo = new LoginRequestInfo()
                {
                    Body = loginAuthInfo.Body,
                    Attachments = loginAuthInfo.Attachments,
                    RemoteAddress = loginAuthInfo.RemoteAddress
                };
                var loginAuthResult = await loginValidator.Validate(loginInfo);
                return new LoginResponseData()
                {
                    AuthIdentity = loginAuthResult.AuthIdentity,
                    Attachments = loginAuthResult.Attachments,
                    AuthFailedMessage = loginAuthResult.AuthFailedMessage,
                    AuthResult = loginAuthResult.AuthResult,
                    AuthStatusCode = loginAuthResult.AuthStatusCode
                };
            });

            server.OnRecieveServiceRequest += new RecieveServiceRequestDelegate(async (byte[] message, IDictionary<string, byte[]> attachments, LoginState loginState) =>
            {
                var serviceProcessTimeBegin = DateTime.Now;
                var serviceRequest = await serializer.DeserializeAsync(protocolStackFactory.ServiceRequestType, message) as IServiceRequest;

                RouteDescription route = null;
                try
                {
                    route = routeManager.GetRoute(serviceRequest.ServiceId, serviceRequest.ActionId);
                }
                catch (RouteNotFoundException ex)
                {
                    logger.LogError(ex, $"RouteManager.GetRoute has error, route is not exist. ServiceId={serviceRequest.ServiceId}, ActionId={serviceRequest.ActionId}, ExceptionMessage={ex.Message}");
                    return await CreateServiceExceptionResponseDataAsync(ServiceExceptionKeys.SERVICE_NOT_EXIST_ERROR);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"RouteManager.GetRoute has error, ServiceId={serviceRequest.ServiceId}, ActionId={serviceRequest.ActionId}, ExceptionMessage={ex.Message}");
                    return await CreateSystemExceptionResponseDataAsync(SystemExceptionKeys.SYSTEM_ERROR);
                }

                logger.LogDebug($"Get route info. ServiceId={route.ServiceId}, ActionId={route.ActionId}, ServiceType={route.ServiceType}, ActionType={route.ActionType}");

                var context = new ServiceContext(config.Host, config.Port, loginState.Identity, loginState.RemoteAddress, route, serviceRequest.ParamList, attachments);

                var serviceProcessResult = await serviceProcessor.ProcessAsync(context);

                logger.LogInformation($"Service process used time: {DateTime.Now - serviceProcessTimeBegin}, ServiceId={serviceRequest.ServiceId}, ActionId={serviceRequest.ActionId}");

                return new ResponseData()
                {
                    Attachments = serviceProcessResult.Attachments,
                    Data = await serializer.SerializeAsync(serviceProcessResult.ServiceResponse)
                };
            });

            logger.LogInformation("Server is binding.");

            OnStarting?.Invoke(new NodeServerStartEventArg(config.Host, config.Port, routeManager.GetAllRoutes()));

            return server.StartAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    logger.LogError(task.Exception, $"Server start has error. Host={config.Host}, Port={config.Port}, ExceptionMessage={task.Exception.InnerException.Message}, ExceptionStackTrace={task.Exception.InnerException.StackTrace}");
                    return;
                }
                logger.LogInformation($"Server listen port {config.Port}");
                OnStarted?.Invoke(new NodeServerStartEventArg(config.Host, config.Port, routeManager.GetAllRoutes()));
            });
        }

        /// <summary>
        /// 停止NodeServer
        /// </summary>
        public Task StopAsync()
        {
            if (server == null)
            {
                logger.LogDebug("Server instance is not start.");
                return Task.CompletedTask;
            }

            OnStopping?.Invoke(new NodeServerStopEventArg(config.Host, config.Port, routeManager.GetAllRoutes()));

            return server.CloseAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    logger.LogError(task.Exception, $"Server closing has error. Host={config.Host}, Port={config.Port}, ExceptionMessage={task.Exception.InnerException.Message}, ExceptionStackTrace={task.Exception.InnerException.StackTrace}");
                    return;
                }
                logger.LogInformation("Server closed");
                OnStopped?.Invoke(new NodeServerStopEventArg(config.Host, config.Port, routeManager.GetAllRoutes()));
            });
        }

        #region 私有方法

        /// <summary>
        /// 加载注册为XNode的服务对象
        /// </summary>
        private void LoadServices()
        {
            var typeList = serviceProvider.GetNodeServiceTypes();

            foreach(var type in typeList)
            {
                var routeList = routeDescriptor.CreateRouteDescription(type);
                foreach (var route in routeList)
                {
                    routeManager.AddRoute(route);
                }
            }
        }

        /// <summary>
        /// 验证配置对象
        /// </summary>
        /// <param name="config"></param>
        private void ValidateConfig(NodeServerConfig config)
        {
            if (config.ServiceProvider == null)
            {
                throw new InvalidOperationException("ServiceProvider config is null.");
            }
            if (config.RouteFactory == null)
            {
                throw new InvalidOperationException("RouteFactory config is null.");
            }
            if (config.Serializer == null)
            {
                throw new InvalidOperationException("Serializer config is null.");
            }
            if (config.ServiceInvoker == null)
            {
                throw new InvalidOperationException("ServiceInvoker config is null.");
            }
            if(config.LoginValidator == null)
            {
                throw new InvalidOperationException("LoginValidator config is null.");
            }
        }

        /// <summary>
        /// 创建ResponseData
        /// </summary>
        /// <param name="response"></param>
        /// <param name="attachments"></param>
        /// <returns></returns>
        private async Task<ResponseData> CreateResponseDataAsync(IServiceResponse response, IDictionary<string, byte[]> attachments)
        {
            return new ResponseData()
            {
                Attachments = attachments,
                Data = await serializer.SerializeAsync(response)
            };
        }

        /// <summary>
        /// 创建异常ResponseData
        /// </summary>
        /// <param name="exceptionId"></param>
        /// <returns></returns>
        private async Task<ResponseData> CreateServiceExceptionResponseDataAsync(int exceptionId)
        {
            var response = protocolStackFactory.CreateServiceResponse();
            response.HasException = true;
            response.ExceptionId = exceptionId;
            response.ExceptionMessage = ExceptionMap.ServiceExceptions[exceptionId];

            return new ResponseData()
            {
                Data = await serializer.SerializeAsync(response)
            };
        }

        /// <summary>
        /// 创建异常ResponseData
        /// </summary>
        /// <param name="exceptionId"></param>
        /// <returns></returns>
        private async Task<ResponseData> CreateSystemExceptionResponseDataAsync(int exceptionId)
        {
            var response = protocolStackFactory.CreateServiceResponse();
            response.HasException = true;
            response.ExceptionId = exceptionId;
            response.ExceptionMessage = ExceptionMap.SystemExceptions[exceptionId];

            return new ResponseData()
            {
                Data = await serializer.SerializeAsync(response)
            };
        }

        #endregion
    }
}
