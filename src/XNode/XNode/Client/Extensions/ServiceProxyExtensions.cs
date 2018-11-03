// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace XNode.Client
{
    /// <summary>
    /// ServiceProxy扩展方法
    /// </summary>
    public static class ServiceProxyExtensions
    {
        #region Invoke

        private static MethodInfo GetResultMethodInfo = typeof(ServiceProxyExtensions).GetMethod("GetResult", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// 根据服务代理配置决定调用远程服务或本地服务
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="actionProxyType">Action代理类型</param>
        /// <param name="paramList">服务调用参数列表</param>
        /// <param name="localServiceInvoker">本地服务调用委托，当服务代理配置为本地服务时会被调用，参数：MethodInfo=Action代理类型，object[]=服务调用参数列表</param>
        /// <param name="remoteServiceInvokeCallback">远程服务执行完成回调，当服务代理配置为远程服务时会被调用，参数：MethodInfo=Action代理类型，object=服务返回值</param>
        public static void Invoke(this IServiceProxy serviceProxy, MethodInfo actionProxyType, object[] paramList, Action<MethodInfo, object[]> localServiceInvoker, Action<MethodInfo, object, Exception> remoteServiceInvokeCallback)
        {
            var serviceProxyInfo = serviceProxy.GetServiceProxyInfo(actionProxyType);
            if (serviceProxyInfo != null && serviceProxyInfo.Enabled)
            {
                var task = serviceProxy.CallRemoteServiceAsync(actionProxyType, paramList);
                //服务返回值为Task或Task的子类
                if (typeof(Task).IsAssignableFrom(actionProxyType.ReturnType))
                {
                    //服务返回值为Task<T>
                    if (actionProxyType.ReturnType.GetTypeInfo().IsGenericType)
                    {
                        //Task<object>无法转换为Task<T>，因此通过反射创建Task<T>对象
                        var realType = actionProxyType.ReturnType.GenericTypeArguments[0];
                        //创建Task<T>构造函数所需的第一个参数Func<,>
                        Type funcType = typeof(Func<,>).MakeGenericType(typeof(object), realType);
                        //为Func<,>创建实现方法
                        MethodInfo method = GetResultMethodInfo.MakeGenericMethod(typeof(object), realType);
                        //创建Func<,>委托
                        var handler = Delegate.CreateDelegate(funcType, method);
                        //创建Task<T>实例
                        var newTask = (Task)Activator.CreateInstance(actionProxyType.ReturnType, handler, task);
                        //在Task<T>中同步调用原始task
                        newTask.Start();

                        remoteServiceInvokeCallback?.Invoke(actionProxyType, newTask, null);
                    }
                    //服务返回值为Task
                    else
                    {
                        remoteServiceInvokeCallback?.Invoke(actionProxyType, task, null);
                    }
                }
                //服务返回值为非Task类型
                else if (actionProxyType.ReturnType != typeof(void))
                {
                    try
                    {
                        task.Wait();
                        remoteServiceInvokeCallback?.Invoke(actionProxyType, task.Result, null);
                    }
                    catch (AggregateException ex)
                    {
                        remoteServiceInvokeCallback?.Invoke(actionProxyType, null, ex.InnerException);
                    }
                    catch (Exception ex)
                    {
                        remoteServiceInvokeCallback?.Invoke(actionProxyType, null, ex);
                    }
                }
                else
                {
                    try
                    {
                        task.Wait();
                        remoteServiceInvokeCallback?.Invoke(actionProxyType, null, null);
                    }
                    catch (AggregateException ex)
                    {
                        remoteServiceInvokeCallback?.Invoke(actionProxyType, null, ex.InnerException);
                    }
                    catch (Exception ex)
                    {
                        remoteServiceInvokeCallback?.Invoke(actionProxyType, null, ex);
                    }
                }
            }
            else
            {
                localServiceInvoker?.Invoke(actionProxyType, paramList);
            }
        }

        private static T2 GetResult<T1, T2>(T1 state)
        {
            var task = state as Task<object>;
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
            return (T2)task.Result;
        }

        #endregion

        /// <summary>
        /// 添加多个Client
        /// </summary>
        /// <param name="serviceProxy"></param>
        /// <param name="nodeClientList">NodeClient列表</param>
        public static IServiceProxy AddClients(this IServiceProxy serviceProxy, IList<INodeClient> nodeClientList)
        {
            foreach (var nodeClient in nodeClientList)
            {
                serviceProxy.AddClient(nodeClient);
            }
            return serviceProxy;
        }

        /// <summary>
        /// 添加Service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProxy"></param>
        /// <returns></returns>
        public static IServiceProxy AddService<T>(this IServiceProxy serviceProxy) where T : class
        {
            serviceProxy.AddService(typeof(T));
            return serviceProxy;
        }

        /// <summary>
        /// 根据类型名列表添加Service
        /// </summary>
        /// <param name="serviceProxy"></param>
        /// <param name="typeNameList"></param>
        /// <returns></returns>
        public static IServiceProxy AddServices(this IServiceProxy serviceProxy, IList<string> typeNameList)
        {
            foreach (var typeName in typeNameList)
            {
                var type = Type.GetType(typeName);
                if (!type.IsServiceProxy())
                {
                    throw new InvalidOperationException($"{type.FullName} is not service proxy.");
                }
                serviceProxy.AddService(type);
            }
            return serviceProxy;
        }

        /// <summary>
        /// 将代理中的所有服务设置为启用
        /// 注意：在调用此方法后添加的服务将不会被此方法的设置所影响
        /// </summary>
        /// <param name="serviceProxy"></param>
        /// <returns></returns>
        public static IServiceProxy EnableAll(this IServiceProxy serviceProxy)
        {
            foreach (var info in serviceProxy.ServiceProxyInfos)
            {
                info.Enabled = true;
            }
            return serviceProxy;
        }

        /// <summary>
        /// 将代理中的所有服务设置为禁用
        /// 注意：在调用此方法后添加的服务将不会被此方法的设置所影响
        /// </summary>
        /// <param name="serviceProxy"></param>
        /// <returns></returns>
        public static IServiceProxy DisableAll(this IServiceProxy serviceProxy)
        {
            foreach (var info in serviceProxy.ServiceProxyInfos)
            {
                info.Enabled = false;
            }
            return serviceProxy;
        }

        /// <summary>
        /// 将代理中的指定服务设置为启用
        /// 注意：在调用此方法后添加的服务将不会被此方法的设置所影响
        /// </summary>
        /// <param name="serviceProxy"></param>
        /// <param name="serviceId"></param>
        /// <param name="actionId"></param>
        /// <returns></returns>
        public static IServiceProxy Enable(this IServiceProxy serviceProxy, int serviceId, int? actionId = null)
        {
            if (actionId != null)
            {
                var info = serviceProxy.ServiceProxyInfos.Where(p => p.ServiceId == serviceId && p.ActionId == actionId.Value).SingleOrDefault();
                if (info != null)
                {
                    info.Enabled = true;
                }
            }
            else
            {
                var infoList = serviceProxy.ServiceProxyInfos.Where(p => p.ServiceId == serviceId);
                foreach (var info in infoList)
                {
                    info.Enabled = true;
                }
            }
            return serviceProxy;
        }

        /// <summary>
        /// 将代理中的指定服务设置为禁用
        /// 注意：在调用此方法后添加的服务将不会被此方法的设置所影响
        /// </summary>
        /// <param name="serviceProxy"></param>
        /// <param name="serviceId"></param>
        /// <param name="actionId"></param>
        /// <returns></returns>
        public static IServiceProxy Disable(this IServiceProxy serviceProxy, int serviceId, int? actionId = null)
        {
            if (actionId != null)
            {
                var info = serviceProxy.ServiceProxyInfos.Where(p => p.ServiceId == serviceId && p.ActionId == actionId.Value).SingleOrDefault();
                if (info != null)
                {
                    info.Enabled = false;
                }
            }
            else
            {
                var infoList = serviceProxy.ServiceProxyInfos.Where(p => p.ServiceId == serviceId);
                foreach (var info in infoList)
                {
                    info.Enabled = false;
                }
            }
            return serviceProxy;
        }

        /// <summary>
        /// 判断指定类型是否为服务代理
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsServiceProxy(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.GetCustomAttribute<ServiceProxyAttribute>() == null)
            {
                return false;
            }
            return true;
        }
    }
}
