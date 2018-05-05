// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Client;
using XNode.Serializer;
using XNode.Server;
using Zipkin;

namespace XNode.Zipkin
{
    public class ZipkinCaller : ServiceCallerBase
    {
        private ISerializer serializer;

        public ZipkinCaller(ISerializer serializer)
        {
            this.serializer = serializer;
        }

        public async override Task<ServiceCallResult> CallAsync(INodeClientContainer nodeClientContainer, ServiceCallInfo info)
        {
            string name;
            if (!string.IsNullOrEmpty(info.ServiceName) && !string.IsNullOrEmpty(info.ActionName))
            {
                name = $"Proxy:{info.ServiceName}.{info.ActionName}({info.ServiceId.ToString()}.{info.ActionId.ToString()})";
            }
            else
            {
                name = $"Proxy:{info.ServiceId.ToString()}.{info.ActionId.ToString()}";
            }

            var crossProcessBag = new Dictionary<string, string>();

            if (ServiceContext.Current == null)
            {
                using (var trace = new StartClientTrace(name))
                {
                    TraceContextPropagation.PropagateTraceIdOnto(crossProcessBag);
                    return await CallNext(nodeClientContainer, info, crossProcessBag, trace);
                }
            }
            else
            {
                using (var trace = new LocalTrace(name))
                {
                    TraceContextPropagation.PropagateTraceIdOnto(crossProcessBag);
                    return await CallNext(nodeClientContainer, info, crossProcessBag, trace);
                }
            }
        }

        private async Task<ServiceCallResult> CallNext(INodeClientContainer nodeClientContainer, ServiceCallInfo info, Dictionary<string, string> crossProcessBag, ITrace trace)
        {
            if (info.Attachments == null)
            {
                info.Attachments = new Dictionary<string, byte[]>();
            }

            info.Attachments.Add(Constants.CrossProcessBagKey, await serializer.SerializeAsync(crossProcessBag));

            try
            {
                return await Next.CallAsync(nodeClientContainer, info);
            }
            catch (Exception ex)
            {
                trace.AnnotateWith(PredefinedTag.Error, ex.Message);
                throw ex;
            }
        }
    }
}
