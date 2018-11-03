// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.ProtocolStack;
using XNode.Serializer;
using XNode.Server;
using Zipkin;

namespace XNode.Zipkin
{
    public class ZipkinProcessor : ServiceProcessorBase
    {
        /// <summary>
        /// 服务追踪
        /// </summary>
        /// <param name="context">服务上下文，每次服务调用共享一个实例</param>
        /// <returns></returns>
        public override async Task<ServiceProcessResult> ProcessAsync(ServiceContext context)
        {
            if (context.Attachments == null || !context.Attachments.ContainsKey(Constants.CrossProcessBagKey))
            {
                return await CallNext(context);
            }

            string name;
            if (!string.IsNullOrEmpty(context.Route.ServiceName) && !string.IsNullOrEmpty(context.Route.ActionName))
            {
                name = $"{context.Route.ServiceName}.{context.Route.ActionName}({context.Route.ServiceId.ToString()}.{context.Route.ActionId.ToString()})";
            }
            else
            {
                name = $"{context.Route.ServiceId.ToString()}.{context.Route.ActionId.ToString()}";
            }

            var crossProcessBag = (Dictionary<string, string>)await Serializer.DeserializeAsync(typeof(Dictionary<string, string>), context.Attachments[Constants.CrossProcessBagKey]);

            using (var trace = new StartServerTrace(name, crossProcessBag))
            {
                try
                {
                    return await CallNext(context);
                }
                catch (Exception ex)
                {
                    trace.AnnotateWith(PredefinedTag.Error, ex.Message);
                    throw ex;
                }
            }
        }

        private async Task<ServiceProcessResult> CallNext(ServiceContext context)
        {
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
