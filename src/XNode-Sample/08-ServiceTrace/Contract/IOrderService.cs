// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XNode.Client;
using XNode.Server;

namespace Contract
{
    [Service("OrderService", 20001, true)]
    [ServiceProxy("OrderService", 20001)]
    public interface IOrderService
    {
        [Action("GetOrders", 1)]
        [ActionProxy("GetOrders", 1)]
        Task<List<Order>> GetOrders(int customerId);
    }
}
