// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XNode.Server;

namespace XNode.Security
{
    /// <summary>
    /// 默认服务授权验证
    /// </summary>
    public class DefaultServiceAuthorizer : IServiceAuthorizer
    {
        private ILogger logger;

        private IList<AuthorizeInfoCache> authorizeList;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configs">授权配置</param>
        /// <param name="loggerFactory">日志工厂</param>
        public DefaultServiceAuthorizer(IList<DefaultServiceAuthorizeConfig> configs, ILoggerFactory loggerFactory = null)
        {
            if (loggerFactory != null)
            {
                logger = loggerFactory.CreateLogger<DefaultServiceAuthorizer>();
            }

            LoadConfig(configs);
        }

        /// <summary>
        /// 授权验证
        /// </summary>
        /// <param name="context">服务上下文</param>
        /// <param name="serviceId">服务Id</param>
        /// <param name="actionId">ActionId</param>
        /// <param name="attachments">附加数据</param>
        public virtual Task Validate(ServiceContext context, int serviceId, int actionId, IDictionary<string, byte[]> attachments)
        {
            var authorizeInfoCache = authorizeList.Where((a => a.ServiceId == serviceId && a.ActionId == actionId)).FirstOrDefault();
            if (authorizeInfoCache == null)
            {
                return Task.CompletedTask;
            }

            if (!authorizeInfoCache.Authorizes.ContainsKey(context.Identity))
            {
                throw new ServiceAuthorizeException(ServiceAuthorizeExceptionType.NoAuthorize, "No Authorize.");
            }

            var authorizeInfo = authorizeInfoCache.Authorizes[context.Identity];

            ValidateDateLimit(authorizeInfo);
            ValidateTimeLimit(authorizeInfo);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 加载服务验证配置
        /// </summary>
        /// <param name="configs">配置列表</param>
        public virtual void LoadConfig(IList<DefaultServiceAuthorizeConfig> configs)
        {
            var list = new List<AuthorizeInfoCache>();

            if (configs != null)
            {
                foreach (var config in configs)
                {
                    if (config.Actions != null)
                    {
                        foreach (var actionConfig in config.Actions)
                        {
                            var cache = new AuthorizeInfoCache()
                            {
                                ServiceId = config.ServiceId,
                                ActionId = actionConfig.ActionId,
                                Authorizes = new Dictionary<string, DefaultServiceAuthorizeInfo>()
                            };
                            if (actionConfig.Authorizes != null && actionConfig.Authorizes.Count > 0)
                            {
                                foreach (var authorizeInfo in actionConfig.Authorizes)
                                {
                                    if (!cache.Authorizes.ContainsKey(authorizeInfo.Account))
                                    {
                                        cache.Authorizes.Add(authorizeInfo.Account, authorizeInfo);
                                    }
                                }
                                list.Add(cache);
                            }
                        }
                    }
                }
            }

            authorizeList = list;
            LogInformation("ServiceAuthorizer load config success.");
        }

        /// <summary>
        /// 验证日期限制
        /// </summary>
        /// <param name="authorizeInfo">验证配置</param>
        protected virtual void ValidateDateLimit(DefaultServiceAuthorizeInfo authorizeInfo)
        {
            var currentDate = DateTime.Now;
            if (authorizeInfo.DateLimitBegin != null && currentDate < authorizeInfo.DateLimitBegin.Value)
            {
                throw new ServiceAuthorizeException(ServiceAuthorizeExceptionType.DateLimit, "Date limit.");
            }
            if (authorizeInfo.DateLimitEnd != null && currentDate > authorizeInfo.DateLimitEnd.Value)
            {
                throw new ServiceAuthorizeException(ServiceAuthorizeExceptionType.DateLimit, "Date limit.");
            }
        }

        /// <summary>
        /// 验证时间限制
        /// </summary>
        /// <param name="authorizeInfo">验证配置</param>
        protected virtual void ValidateTimeLimit(DefaultServiceAuthorizeInfo authorizeInfo)
        {
            var currentDate = DateTime.Now;
            var currentTimeSpan = new TimeSpan(currentDate.Hour, currentDate.Minute, currentDate.Second);
            if (authorizeInfo.TimeLimitBegin != null && currentTimeSpan < authorizeInfo.TimeLimitBegin.Value)
            {
                throw new ServiceAuthorizeException(ServiceAuthorizeExceptionType.TimeLimit, "Time limit.");
            }
            if (authorizeInfo.TimeLimitEnd != null && currentTimeSpan > authorizeInfo.TimeLimitEnd.Value)
            {
                throw new ServiceAuthorizeException(ServiceAuthorizeExceptionType.TimeLimit, "Time limit.");
            }
        }

        /// <summary>
        /// 记录Debug日志
        /// </summary>
        /// <param name="log"></param>
        protected void LogDebug(string log)
        {
            if (logger != null)
            {
                logger.LogDebug(log);
            }
        }

        /// <summary>
        /// 记录Info日志
        /// </summary>
        /// <param name="log"></param>
        protected void LogInformation(string log)
        {
            if (logger != null)
            {
                logger.LogInformation(log);
            }
        }
    }

    /// <summary>
    /// 默认服务授权验证配置
    /// </summary>
    public class DefaultServiceAuthorizeConfig
    {
        /// <summary>
        /// 服务Id
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// Action列表
        /// </summary>
        public List<DefaultServiceAuthorizeAction> Actions { get; set; }
    }

    /// <summary>
    /// 默认服务授权验证Action配置
    /// </summary>
    public class DefaultServiceAuthorizeAction
    {
        /// <summary>
        /// ActionId
        /// </summary>
        public int ActionId { get; set; }

        /// <summary>
        /// 授权详细列表
        /// </summary>
        public List<DefaultServiceAuthorizeInfo> Authorizes { get; set; }
    }

    /// <summary>
    /// 授权详细信息
    /// </summary>
    public class DefaultServiceAuthorizeInfo
    {
        /// <summary>
        /// 账号名称
        /// </summary>
        public string Account { get; set; }

        #region 日期限制

        private string dateLimit;

        private DateTime? dateLimitBegin;

        private DateTime? dateLimitEnd;

        /// <summary>
        /// 日期限制
        /// </summary>
        public string DateLimit
        {
            get
            {
                return dateLimit;
            }
            set
            {
                dateLimit = value;
                if (string.IsNullOrEmpty(dateLimit))
                {
                    return;
                }
                var dateLimitArr = dateLimit.Split('~');
                if (dateLimitArr.Length > 0 && !string.IsNullOrEmpty(dateLimitArr[0]))
                {
                    dateLimitBegin = Convert.ToDateTime(dateLimitArr[0]);
                }
                if (dateLimitArr.Length > 1 && !string.IsNullOrEmpty(dateLimitArr[1]))
                {
                    dateLimitEnd = Convert.ToDateTime(dateLimitArr[1]).AddDays(1).AddMilliseconds(-1);
                }
                
            }
        }

        /// <summary>
        /// 日期限制开始
        /// </summary>
        public DateTime? DateLimitBegin
        {
            get
            {
                return dateLimitBegin;
            }
        }

        /// <summary>
        /// 日期限制结束
        /// </summary>
        public DateTime? DateLimitEnd
        {
            get
            {
                return dateLimitEnd;
            }
        }

        #endregion 时间限制

        #region 时间限制

        private string timeLimit;

        private TimeSpan? timeLimitBegin;

        private TimeSpan? timeLimitEnd;

        /// <summary>
        /// 日期限制
        /// </summary>
        public string TimeLimit
        {
            get
            {
                return timeLimit;
            }
            set
            {
                timeLimit = value;
                if (string.IsNullOrEmpty(timeLimit))
                {
                    return;
                }
                var timeLimitArr = timeLimit.Split('~');
                if (timeLimitArr.Length > 0 && !string.IsNullOrEmpty(timeLimitArr[0]))
                {
                    timeLimitBegin = TimeSpan.Parse(timeLimitArr[0]);
                }
                if (timeLimitArr.Length > 1 && !string.IsNullOrEmpty(timeLimitArr[1]))
                {
                    timeLimitEnd = TimeSpan.Parse(timeLimitArr[1]);
                }

            }
        }

        /// <summary>
        /// 日期限制开始
        /// </summary>
        public TimeSpan? TimeLimitBegin
        {
            get
            {
                return timeLimitBegin;
            }
        }

        /// <summary>
        /// 日期限制结束
        /// </summary>
        public TimeSpan? TimeLimitEnd
        {
            get
            {
                return timeLimitEnd;
            }
        }

        #endregion 时间限制
    }

    internal class AuthorizeInfoCache
    {
        internal int ServiceId { get; set; }

        internal int ActionId { get; set; }

        internal IDictionary<string, DefaultServiceAuthorizeInfo> Authorizes { get; set; }
    }
}
