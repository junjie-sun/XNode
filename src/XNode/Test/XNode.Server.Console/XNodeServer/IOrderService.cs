// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Server.Console.XNodeServer
{
    [Service("OrderService", 10002)]
    public interface IOrderService
    {
        [Action("AddOrder", 1)]
        Task AddOrder(Order order);

        [Action("QueryOrder", 2)]
        Task<List<Order>> QueryOrder(int customerId, string customerName);
    }
}
