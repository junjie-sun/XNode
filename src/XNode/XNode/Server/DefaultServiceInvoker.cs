// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XNode.Logging;
using XNode.Serializer;
using XNode.Server.Route;

namespace XNode.Server
{
    /// <summary>
    /// 服务调用器默认实现
    /// </summary>
    public class DefaultServiceInvoker : IServiceInvoker
    {
        private ILogger logger = LoggerManager.ServerLoggerFactory.CreateLogger<DefaultServiceInvoker>();

        private IDictionary<MethodInfo, ActionInfo> actionInfoCache = new Dictionary<MethodInfo, ActionInfo>();

        private object actionInfoCacheLockObj = new object();

        /// <summary>
        /// 服务提供器
        /// </summary>
        public IServiceProvider ServiceProvider { private get; set; }

        /// <summary>
        /// 调用服务
        /// </summary>
        /// <param name="serializer">序列化器</param>
        /// <param name="route">路由信息</param>
        /// <param name="paramList">服务参数列表</param>
        /// <returns></returns>
        public async Task<byte[]> Invoke(ISerializer serializer, RouteDescription route, IList<byte[]> paramList)
        {
            var actionType = route.ActionType;

            var paramArr = await GetParams(serializer, actionType, paramList);

            return await Invoke(serializer, route.ServiceType, actionType, paramArr);
        }

        private async Task<object[]> GetParams(ISerializer serializer, MethodInfo actionType, IList<byte[]> paramList)
        {
            var actionInfo = GetActionInfo(actionType);
            var paramInfoArr = actionInfo.ParameterInfoList;
            var result = new object[paramInfoArr.Length];
            logger.LogDebug($"GetParams: Get action info. MethodInfo={actionInfo.MethodInfo}");

            for (var i = 0; i < paramInfoArr.Length; i++)
            {
                var paramInfo = paramInfoArr[i];
                if (paramList != null && i < paramList.Count)
                {
                    try
                    {
                        result[i] = await serializer.DeserializeAsync(paramInfo.ParameterType, paramList[i]);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"GetParams deserialize parameter error. MethodInfo={actionInfo.MethodInfo}, ParameterType={paramInfo.ParameterType}");
                        throw ex;
                    }
                }
                else
                {
                    result[i] = paramInfo.HasDefaultValue ? paramInfo.DefaultValue : null;
                }
            }

            return result;
        }

        private async Task<byte[]> Invoke(ISerializer serializer, Type serviceType, MethodInfo methodInfo, object[] paramList)
        {
            var returnType = methodInfo.ReturnType;
            var serviceObj = ServiceProvider.GetNodeServiceInstance(serviceType);

            if (serviceObj == null)
            {
                throw new ServiceInvokeException($"Not found service instance. ServiceType={serviceType.FullName}");
            }

            logger.LogInformation($"Service invoke. ServiceType={serviceType}, methodInfo={methodInfo}");

            //服务返回值为Task或Task的子类
            if (typeof(Task).IsAssignableFrom(returnType))
            {
                var returnTypeInfo = returnType.GetTypeInfo();
                var returnVal = methodInfo.Invoke(serviceObj, paramList);
                //服务返回值为Task<T>
                if (returnTypeInfo.IsGenericType)
                {
                    await ((Task)returnVal);
                    var resultProp = returnType.GetProperty("Result");
                    var result = resultProp.GetValue(returnVal);
                    logger.LogDebug($"Service invoke: return type is Task<T>. ServiceType={serviceType}, methodInfo={methodInfo}, Result={result}");
                    return await serializer.SerializeAsync(result);
                }
                //服务返回值为Task
                else
                {
                    await ((Task)returnVal);
                    logger.LogDebug($"Service invoke: return type is Task. ServiceType={serviceType}, methodInfo={methodInfo}");
                    return null;
                }
            }
            //服务返回值为非Task类型
            else
            {
                var returnVal = methodInfo.Invoke(serviceObj, paramList);
                //服务返回值为void
                if (returnType == typeof(void))
                {
                    logger.LogDebug($"Service invoke: return type is void. ServiceType={serviceType}, methodInfo={methodInfo}");
                    return null;
                }
                //服务返回值为其它类型
                else
                {
                    logger.LogDebug($"Service invoke: return type is not Task and not void. ServiceType={serviceType}, methodInfo={methodInfo}, Result={returnVal}");
                    return await serializer.SerializeAsync(returnVal);
                }
            }
        }

        private ActionInfo GetActionInfo(MethodInfo actionType)
        {
            if (!actionInfoCache.ContainsKey(actionType))
            {
                lock(actionInfoCacheLockObj)
                {
                    if (!actionInfoCache.ContainsKey(actionType))
                    {
                        actionInfoCache.Add(actionType, new ActionInfo()
                        {
                            MethodInfo = actionType,
                            ParameterInfoList = actionType.GetParameters()
                        });
                    }   
                }
            }
            return actionInfoCache[actionType];
        }
    }

    internal class ActionInfo
    {
        internal MethodInfo MethodInfo { get; set; }

        internal ParameterInfo[] ParameterInfoList { get; set; }
    }
}
