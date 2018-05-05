// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Client;
using XNode.Server;

namespace XNode.Mix.Console.Services
{
    [Service("OrderService", 10002)]
    [ServiceProxy("OrderService", 10002)]
    public interface IOrderService
    {
        [Action("AddOrder", 1)]
        [ActionProxy("AddOrder", 1)]
        Task AddOrder(Order order);

        [Action("QueryOrder", 2)]
        [ActionProxy("QueryOrder", 2)]
        Task<List<Order>> QueryOrder(int customerId, string customerName);
    }
}
