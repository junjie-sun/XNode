// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Logging;
using XNode.Serializer;
using XNode.ProtocolStack;
using XNode.Server.Route;
using XNode.Security;
using XNode.Common;

namespace XNode.Server.Processors
{
    /// <summary>
    /// 服务授权验证处理器
    /// </summary>
    public class ServiceAuthorizeProcessor : ServiceProcessorBase
    {
        private ILogger logger;

        private IServiceAuthorizer serviceAuthorizer;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceAuthorizer">服务验证器</param>
        public ServiceAuthorizeProcessor(IServiceAuthorizer serviceAuthorizer)
        {
            logger = LoggerManager.ServerLoggerFactory.CreateLogger<ServiceAuthorizeProcessor>();
            this.serviceAuthorizer = serviceAuthorizer ?? throw new InvalidOperationException("serviceAuthorize is null.");
        }

        /// <summary>
        /// 服务授权验证
        /// </summary>
        /// <param name="context">服务上下文，每次服务调用共享一个实例</param>
        /// <returns></returns>
        public override async Task<ServiceProcessResult> ProcessAsync(ServiceContext context)
        {
            try
            {
                await serviceAuthorizer.Validate(context, context.Route.ServiceId, context.Route.ActionId, context.Attachments);
            }
            catch (ServiceAuthorizeException ex)
            {
                logger.LogError(ex, $"Service authorize failed. ServiceId={context.Route.ServiceId}, ActionId={context.Route.ActionId}, Identity={context.Identity}, Remote={context.RemoteAddress.Address.ToIPString()}, Message={ex.Message}");
                switch(ex.ServiceAuthorizeExceptionType)
                {
                    case ServiceAuthorizeExceptionType.NoAuthorize:
                        return ServiceProcessorUtils.CreateServiceExceptionResult(ServiceExceptionKeys.SERVICE_NO_AUTHORIZE, ProtocolStackFactory);
                    case ServiceAuthorizeExceptionType.DateLimit:
                        return ServiceProcessorUtils.CreateServiceExceptionResult(ServiceExceptionKeys.SERVICE_DATE_LIMIT, ProtocolStackFactory);
                    case ServiceAuthorizeExceptionType.TimeLimit:
                        return ServiceProcessorUtils.CreateServiceExceptionResult(ServiceExceptionKeys.SERVICE_TIME_LIMIT, ProtocolStackFactory);
                    default:
                        return ServiceProcessorUtils.CreateServiceExceptionResult(ServiceExceptionKeys.SERVICE_NO_AUTHORIZE, ProtocolStackFactory);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Service authorize has error. ServiceId={context.Route.ServiceId}, ActionId={context.Route.ActionId}, Identity={context.Identity}, Remote={context.RemoteAddress.Address.ToIPString()}, ExceptionMessage={ex.Message}");
                return ServiceProcessorUtils.CreateSystemExceptionResult(SystemExceptionKeys.SYSTEM_ERROR, ProtocolStackFactory);
            }

            if (Next != null)
            {
                return await Next.ProcessAsync(context);
            }
            return new ServiceProcessResult()
            {
                ServiceResponse = ProtocolStackFactory.CreateServiceResponse()
            };
        }
    }
}
