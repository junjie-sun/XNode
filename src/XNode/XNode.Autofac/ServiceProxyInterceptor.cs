// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using XNode.Client;

namespace XNode.Autofac
{
    /// <summary>
    /// 服务代理拦截器，如果代理启用则调用远程服务，如果代理禁用则调用本地实现
    /// </summary>
    public class ServiceProxyInterceptor : IInterceptor
    {
        private IServiceProxyManager serviceProxyManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProxyManager">服务代理管理器</param>
        public ServiceProxyInterceptor(IServiceProxyManager serviceProxyManager)
        {
            this.serviceProxyManager = serviceProxyManager;
        }

        public void Intercept(IInvocation invocation)
        {
            //var serviceProxyType = invocation.TargetType;
            var serviceProxy = serviceProxyManager.GetServiceProxy(invocation.Method.DeclaringType);

            if (serviceProxy == null)
            {
                invocation.Proceed();
                return;
            }

            serviceProxy.Invoke(invocation.Method, invocation.Arguments,
                new Action<MethodInfo, object[]>((actionProxyType, paramList) =>
            {
                invocation.Proceed();
            }), new Action<MethodInfo, object, Exception>((actionProxyType, returnVal, ex) =>
            {
                if (ex != null)
                {
                    throw ex;
                }
                invocation.ReturnValue = returnVal;
            }));
        }
    }
}
