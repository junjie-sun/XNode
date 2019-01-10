// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using XNode.Serializer;
using System.Net;
using Microsoft.Extensions.Logging;
using XNode.Logging;
using XNode.Common;

namespace XNode.Security
{
    /// <summary>
    /// 默认登录验证器
    /// </summary>
    public class DefaultLoginValidator : ILoginValidator
    {
        private ILogger logger;

        private DefaultLoginValidatorConfig validatorConfig;

        /// <summary>
        /// 序列化器
        /// </summary>
        public ISerializer Serializer { private get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">配置信息</param>
        /// <param name="loggerFactory">日志工厂</param>
        public DefaultLoginValidator(DefaultLoginValidatorConfig config = null, ILoggerFactory loggerFactory = null)
        {
            if (loggerFactory != null)
            {
                logger = loggerFactory.CreateLogger<DefaultLoginValidator>();
            }

            LoadConfig(config);
        }

        /// <summary>
        /// 加载日志信息
        /// </summary>
        /// <param name="config"></param>
        public void LoadConfig(DefaultLoginValidatorConfig config)
        {
            validatorConfig = config;
            LogInformation("LoginValidator load config success.");
        }

        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="loginInfo">登录信息</param>
        /// <returns></returns>
        public async Task<LoginAuthResult> Validate(LoginRequestInfo loginInfo)
        {
            var config = validatorConfig;

            if (config == null || config.Accounts == null)
            {
                LogInformation("Login validate completed. Result pass, use guest because config is null.");
                return CreateSuccessLoginAuthResult("Guest");
            }

            var paramsResult = ValidateParams(loginInfo);
            if (paramsResult != null)
            {
                LogInformation("Login validate completed. Result reject, params failed validation.");
                return paramsResult;
            }
            
            //验证账号是否存在
            var accountName = (string)await Serializer.DeserializeAsync(typeof(string), loginInfo.Attachments[DefaultLoginAuthConstants.AccountNameKey]);
            var accountInfo = config.Accounts.Where((info) => info.AccountName == accountName).FirstOrDefault();
            if (accountInfo == null)
            {
                LogInformation($"Login validate completed. Result reject, account is not exist. AccountName={accountName}");
                return CreateFailedLoginAuthResult(accountName, 110, $"Account is not exist. AccountName={accountName}");
            }

            //验证IP
            if (!ValidateIP(loginInfo.RemoteAddress, accountInfo))
            {
                LogInformation($"Login validate completed. Result reject, IP rejected. AccountName={accountInfo.AccountName} IP={loginInfo.RemoteAddress.Address.ToIPString()}");
                return CreateFailedLoginAuthResult(accountInfo.AccountName, 111, $"IP rejected. IP={loginInfo.RemoteAddress.Address.ToIPString()}");
            }

            //时间戳：1970年至今的毫秒数
            var timestamp = (string)await Serializer.DeserializeAsync(typeof(string), loginInfo.Attachments[DefaultLoginAuthConstants.TimestampKey]);
            //随机字符串
            var noncestr = (string)await Serializer.DeserializeAsync(typeof(string), loginInfo.Attachments[DefaultLoginAuthConstants.NoncestrKey]);
            //签名
            var signature = (string)await Serializer.DeserializeAsync(typeof(string), loginInfo.Attachments[DefaultLoginAuthConstants.SignatureKey]);
            //验证签名
            if (!ValidateSignature(accountInfo.AccountName, timestamp, noncestr, signature, accountInfo))
            {
                LogInformation("Login validate completed. Result reject, signature invalid.");
                return CreateFailedLoginAuthResult(accountInfo.AccountName, 112, "Signature invalid.");
            }

            LogInformation($"Login validate completed. Result pass. AccountName={accountInfo.AccountName}");
            return CreateSuccessLoginAuthResult(accountInfo.AccountName);
        }

        private LoginAuthResult ValidateParams(LoginRequestInfo loginInfo)
        {
            if (loginInfo.Attachments == null || loginInfo.Attachments.Count == 0)
            {
                LogDebug("Login validate: no login infomation.");
                return CreateFailedLoginAuthResult(null, 100, "No login infomation.");
            }

            if (!loginInfo.Attachments.ContainsKey(DefaultLoginAuthConstants.TimestampKey))
            {
                LogDebug("Login validate: no timestamp.");
                return CreateFailedLoginAuthResult(null, 101, "No timestamp.");
            }

            if (!loginInfo.Attachments.ContainsKey(DefaultLoginAuthConstants.NoncestrKey))
            {
                LogDebug("Login validate: no noncestr.");
                return CreateFailedLoginAuthResult(null, 102, "No noncestr.");
            }

            if (!loginInfo.Attachments.ContainsKey(DefaultLoginAuthConstants.SignatureKey))
            {
                LogDebug("Login validate: no signature.");
                return CreateFailedLoginAuthResult(null, 103, "No signature.");
            }

            if (!loginInfo.Attachments.ContainsKey(DefaultLoginAuthConstants.AccountNameKey))
            {
                LogDebug("Login validate: no accountName.");
                return CreateFailedLoginAuthResult(null, 104, "No accountName.");
            }

            return null;
        }

        private bool ValidateIP(IPEndPoint remoteAddress, DefaultLoginValidatorAccountInfo accountInfo)
        {
            if (accountInfo.IPWhiteList == null || accountInfo.IPWhiteList.Count == 0)
            {
                return true;
            }

            var ip = remoteAddress.Address.ToIPString();

            foreach (var allowedIP in accountInfo.IPWhiteList)
            {
                if (ip == allowedIP)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ValidateSignature(string accountName, string timestamp, string noncestr, string signature, DefaultLoginValidatorAccountInfo accountInfo)
        {
            //签名算法：MD5(accountName=XXX&noncestr=XXX&timestamp=XXX&accountKey=XXX)
            var calcSignature = CryptographyUtils.GetDefaultSignature(accountName, noncestr, timestamp, accountInfo.AccountKey);
            return calcSignature == signature;
        }

        private LoginAuthResult CreateFailedLoginAuthResult(string identity, byte authStatusCode, string authFailedMessage = null)
        {
            return new LoginAuthResult()
            {
                AuthIdentity = identity,
                AuthResult = false,
                AuthStatusCode = authStatusCode,
                AuthFailedMessage = authFailedMessage
            };
        }

        private LoginAuthResult CreateSuccessLoginAuthResult(string identity)
        {
            return new LoginAuthResult()
            {
                AuthIdentity = identity,
                AuthResult = true
            };
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
    /// 默认登录验证器配置
    /// </summary>
    public class DefaultLoginValidatorConfig
    {
        /// <summary>
        /// 账号列表
        /// </summary>
        public List<DefaultLoginValidatorAccountInfo> Accounts { get; set; }
    }

    /// <summary>
    /// 默认登录验证器账号信息
    /// </summary>
    public class DefaultLoginValidatorAccountInfo
    {
        /// <summary>
        /// 账号名称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 账号密钥
        /// </summary>
        public string AccountKey { get; set; }

        /// <summary>
        /// IP白名单
        /// </summary>
        public List<string> IPWhiteList { get; set; }
    }
}
