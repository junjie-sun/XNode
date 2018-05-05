// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Autofac;
using XNode.Server;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using XNode.Logging;

namespace XNode.Autofac
{
    /// <summary>
    /// Autofac服务提供器
    /// </summary>
    public class AutofacServiceProvider : ServiceProviderBase
    {
        private ILogger logger;

        private IContainer container;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="container">Autofac容器</param>
        public AutofacServiceProvider(IContainer container)
        {
            logger = LoggerManager.ServerLoggerFactory.CreateLogger<AutofacServiceProvider>();
            this.container = container ?? throw new InvalidOperationException("Container is null");
        }

        #region 接口实现

        public override object GetNodeServiceInstance(Type serviceType)
        {
            logger.LogDebug($"Get node service instance from Autofac. ServiceType={serviceType}");
            return container.Resolve(serviceType);
        }

        protected override IList<Type> GetServiceTypes()
        {
            logger.LogDebug($"Get node service types from Autofac.");

            var list = new List<Type>();

            foreach (var reg in container.ComponentRegistry.Registrations)
            {
                var e = reg.Services.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current is TypedService t && IsNodeService(t.ServiceType))
                    {
                        list.Add(t.ServiceType);
                    }
                }
            }

            return list;
        }

        #endregion
    }
}
