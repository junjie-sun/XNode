// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Serializer;

namespace XNode.Security
{
    /// <summary>
    /// 默认登录处理器
    /// </summary>
    public class DefaultLoginHandler : ILoginHandler
    {
        private ILogger logger;

        private DefaultLoginHandlerConfig config;

        private ISerializer serializer;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">配置信息</param>
        /// <param name="serializer">序列化器</param>
        /// <param name="loggerFactory">日志工厂</param>
        public DefaultLoginHandler(DefaultLoginHandlerConfig config, ISerializer serializer, ILoggerFactory loggerFactory = null)
        {
            this.config = config;
            this.serializer = serializer;
            if (loggerFactory != null)
            {
                logger = loggerFactory.CreateLogger<DefaultLoginHandler>();
            }
        }

        /// <summary>
        /// 获取登录信息
        /// </summary>
        /// <returns></returns>
        public async Task<LoginInfo> GetLoginInfo()
        {
            if (config == null)
            {
                return new LoginInfo();
            }

            var noncestr = CryptographyUtils.getNoncestr(false, 8);
            var timestamp = ((long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString();
            var signature = CryptographyUtils.GetDefaultSignature(config.AccountName, noncestr, timestamp, config.AccountKey);

            var r = await Task.WhenAll(
                serializer.SerializeAsync(config.AccountName),
                serializer.SerializeAsync(noncestr),
                serializer.SerializeAsync(timestamp),
                serializer.SerializeAsync(signature)
            );

            var loginInfo = new LoginInfo()
            {
                Attachments = new Dictionary<string, byte[]>()
            };
            loginInfo.Attachments.Add(DefaultLoginAuthConstants.AccountNameKey, r[0]);
            loginInfo.Attachments.Add(DefaultLoginAuthConstants.NoncestrKey, r[1]);
            loginInfo.Attachments.Add(DefaultLoginAuthConstants.TimestampKey, r[2]);
            loginInfo.Attachments.Add(DefaultLoginAuthConstants.SignatureKey, r[3]);

            return loginInfo;
        }

        /// <summary>
        /// 登录验证响应处理
        /// </summary>
        /// <param name="loginResponseInfo">登录验证响应信息</param>
        /// <returns>登录验证状态码（非0表示验证失败，1-30为XNode保留状态码）</returns>
        public Task<byte> LoginResponseHandle(LoginResponseInfo loginResponseInfo)
        {
            LogInformation("Login response is handling.");

            if (loginResponseInfo.Body == null || loginResponseInfo.Body.Length == 0)
            {
                LogDebug("Login response has not body.");
                throw new Exception("Login response has not body.");
            }

            LogInformation("Login response handle completed.");
            return Task.FromResult(loginResponseInfo.Body[0]);
        }

        private void LogDebug(string log)
        {
            if (logger != null)
            {
                logger.LogDebug(log);
            }
        }

        private void LogInformation(string log)
        {
            if (logger != null)
            {
                logger.LogInformation(log);
            }
        }
    }

    /// <summary>
    /// 默认登录处理器配置
    /// </summary>
    public class DefaultLoginHandlerConfig
    {
        /// <summary>
        /// 账号名称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 账号密钥
        /// </summary>
        public string AccountKey { get; set; }
    }
}
