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

namespace XNode.Server.Processors
{
    /// <summary>
    /// 服务处理器默认实现
    /// </summary>
    public class DefaultServiceProcessor : ServiceProcessorBase
    {
        private ILogger logger = LoggerManager.ServerLoggerFactory.CreateLogger<DefaultServiceProcessor>();

        /// <summary>
        /// 对服务请求进行处理，根据路由信息调用合适的服务实现
        /// </summary>
        /// <param name="context">服务上下文，每次服务调用共享一个实例</param>
        /// <returns></returns>
        public override async Task<ServiceProcessResult> ProcessAsync(ServiceContext context)
        {
            logger.LogInformation($"Service request process beginning. ServiceId={context.Route.ServiceId}, ActionId={context.Route.ActionId}");

            if (!context.Route.Enabled)
            {
                logger.LogError($"Route is disabled. ServiceId={context.Route.ServiceId}, ActionId={context.Route.ActionId}");
                return ServiceProcessorUtils.CreateServiceExceptionResult(ServiceExceptionKeys.SERVICE_DISABLED_ERROR, ProtocolStackFactory);
            }

            if (Serializer == null || ServiceInvoker == null)
            {
                logger.LogError($"Service has configuration error. ServiceId={context.Route.ServiceId}, ActionId={context.Route.ActionId}");
                return ServiceProcessorUtils.CreateServiceExceptionResult(ServiceExceptionKeys.SERVER_CONFIG_ERROR, ProtocolStackFactory);
            }

            var result = new ServiceProcessResult()
            {
                ServiceResponse = ProtocolStackFactory.CreateServiceResponse()
            };

            try
            {
                result.ServiceResponse.ReturnValue = await ServiceInvoker.Invoke(Serializer, context.Route, context.ActionParamList);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ServiceInvoker.Invoke has error, ServiceId={context.Route.ServiceId}, ActionId={context.Route.ActionId}, ExceptionMessage={ex.Message}");
                return ServiceProcessorUtils.CreateServiceExceptionResult(ServiceExceptionKeys.SERVICE_INVOKE_ERROR, ProtocolStackFactory);
            }

            logger.LogInformation($"Service request process finished. ServiceId={context.Route.ServiceId}, ActionId={context.Route.ActionId}");

            return result;
        }
    }
}
