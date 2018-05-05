// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Autofac.Extras.DynamicProxy;
using Contract.Repository;
using Contract.Service;
using Repository;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XNode.Autofac;
using XNode.Client;

namespace Web.Common
{
    public class AutofacModule : Module
    {
        private IServiceProxyManager _serviceProxyManager;

        public AutofacModule(IServiceProxyManager serviceProxyManager)
        {
            _serviceProxyManager = serviceProxyManager;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new ServiceProxyInterceptor(_serviceProxyManager))
                .SingleInstance();

            builder.RegisterType<CustomerRepository>()
                .As<ICustomerRepository>()
                .SingleInstance();
            builder.RegisterType<GoodsRepository>()
                .As<IGoodsRepository>()
                .SingleInstance();
            builder.RegisterType<OrderRepository>()
                .As<IOrderRepository>()
                .SingleInstance();

            builder.RegisterType<CustomerService>()
                .As<ICustomerService>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(ServiceProxyInterceptor))
                .SingleInstance();
            builder.RegisterType<GoodsService>()
                .As<IGoodsService>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(ServiceProxyInterceptor))
                .SingleInstance();
            builder.RegisterType<OrderService>()
                .As<IOrderService>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(ServiceProxyInterceptor))
                .SingleInstance();
        }
    }
}
