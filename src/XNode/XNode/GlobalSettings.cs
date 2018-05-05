// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace XNode
{
    /// <summary>
    /// 全局设置
    /// </summary>
    public static class GlobalSettings
    {
        /// <summary>
        /// 应用设置
        /// </summary>
        /// <param name="config"></param>
        public static void Apply(GlobalConfig config)
        {
            if (config == null)
            {
                return;
            }

            SetMinThreads(config);

            SetMaxThreads(config);
        }

        private static void SetMinThreads(GlobalConfig config)
        {
            if (config.MinWorkThreads == null && config.MinCompletionPortThreads == null)
            {
                return;
            }

            ThreadPool.GetMinThreads(out int minWorkThreads, out int minCompletionPortThreads);
            ThreadPool.SetMinThreads(
                config.MinWorkThreads != null ? config.MinWorkThreads.Value : minWorkThreads,
                config.MinCompletionPortThreads != null ? config.MinCompletionPortThreads.Value : minCompletionPortThreads
            );
        }

        private static void SetMaxThreads(GlobalConfig config)
        {
            if (config.MaxWorkThreads == null && config.MaxCompletionPortThreads == null)
            {
                return;
            }

            ThreadPool.GetMaxThreads(out int maxWorkThreads, out int maxCompletionPortThreads);
            ThreadPool.SetMaxThreads(
                config.MaxWorkThreads != null ? config.MaxWorkThreads.Value : maxWorkThreads,
                config.MaxCompletionPortThreads != null ? config.MaxCompletionPortThreads.Value : maxCompletionPortThreads
            );
        }
    }

    /// <summary>
    /// 全局配置
    /// </summary>
    public class GlobalConfig
    {
        /// <summary>
        /// 最小工作线程数
        /// </summary>
        public int? MinWorkThreads { get; set; }

        /// <summary>
        /// 最大工作线程数
        /// </summary>
        public int? MaxWorkThreads { get; set; }

        /// <summary>
        /// 最小I/O线程数
        /// </summary>
        public int? MinCompletionPortThreads { get; set; }

        /// <summary>
        /// 最大I/O线程数
        /// </summary>
        public int? MaxCompletionPortThreads { get; set; }
    }

    /// <summary>
    /// 配置扩展方法
    /// </summary>
    public static class ConfigurationExtensions
    {
        public static GlobalConfig GetGlobalConfig(this IConfigurationRoot config)
        {
            var globalConfig = new GlobalConfig();
            config.GetSection("xnode:global")
                .Bind(globalConfig);
            return globalConfig;
        }
    }
}
