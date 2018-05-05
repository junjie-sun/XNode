// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XNode.Communication.ProtocolStack;
using XNode.Logging;

namespace XNode.Communication
{
    /// <summary>
    /// 请求管理器
    /// </summary>
    public class RequestManager
    {
        private ILogger logger;

        private long nextRequestId = 1;

        private ConcurrentDictionary<long, RequestInfo> requestList = new ConcurrentDictionary<long, RequestInfo>();

        public RequestManager(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<RequestManager>();
        }

        /// <summary>
        /// 生成新的RequestId，RequestId从long.MinValue到long.MaxValue循环使用
        /// </summary>
        /// <param name="currentId">最新可使用的RequestId</param>
        /// <returns></returns>
        private long GetNewRequestId(ref long currentId)
        {
            long i, j = currentId;

            do
            {
                i = j;
                j = Interlocked.CompareExchange(ref currentId, (i < long.MaxValue) ? (i + 1) : long.MinValue, i);
            } while (i != j);

            return j;
        }

        /// <summary>
        /// 创建请求信息对象
        /// </summary>
        /// <param name="timeout">请求超时时长（毫秒）</param>
        /// <returns></returns>
        public RequestInfo CreateRequest(int timeout)
        {
            var request = new RequestInfo();

            do
            {
                request.Id = GetNewRequestId(ref nextRequestId);
            } while (!requestList.TryAdd(request.Id, request));

            var tcs = new TaskCompletionSource<Message>();
            var cts = new CancellationTokenSource(timeout);

            var token = cts.Token;
            token.Register(() =>
            {
                if (!requestList.TryRemove(request.Id, out request))
                {
                    return;
                }

                try
                {
                    tcs.SetException(new RequestTimeoutExcption("Request timeout.", request));
                }
                catch { }
            });

            request.Id = request.Id;
            request.TaskCompletionSource = tcs;
            request.CancellationTokenSource = cts;

            logger.LogDebug($"Create request. RequestId={request.Id}");

            return request;
        }

        /// <summary>
        /// 请求完成
        /// </summary>
        /// <param name="requestId">RequestId</param>
        /// <param name="result">承载请求响应的消息对象</param>
        public void CompleteRequest(long requestId, Message result)
        {
            if (!requestList.TryRemove(requestId, out RequestInfo request))
            {
                return;
            }

            try
            {
                request.CancellationTokenSource.Dispose();
                request.TaskCompletionSource.SetResult(result);
                logger.LogDebug($"Request completed. Request={request.Id}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Request has error. ExceptionMessage={ex.Message}");
            }
        }
    }

    /// <summary>
    /// 请求信息对象
    /// </summary>
    public class RequestInfo
    {
        /// <summary>
        /// RequestId
        /// </summary>
        public long Id { get; set; }

        public TaskCompletionSource<Message> TaskCompletionSource { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public Task<Message> Task { get { return TaskCompletionSource.Task; } }
    }

    /// <summary>
    /// 请求超时异常
    /// </summary>
    public class RequestTimeoutExcption : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">异常信息</param>
        /// <param name="request">请求信息对象</param>
        public RequestTimeoutExcption(string message, RequestInfo request) : base(message)
        {
            this.Request = request;
        }

        /// <summary>
        /// 请求信息对象
        /// </summary>
        public RequestInfo Request { get; }
    }
}
