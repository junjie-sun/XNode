// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Client;
using XNode.Server;

namespace Contract.Service
{
    [Service("OrderService", 10002)]
    [ServiceProxy("OrderService", 10002)]
    public interface IOrderService
    {
        [Action("GetOrders", 1)]
        [ActionProxy("GetOrders", 1)]
        Task<List<Order>> GetOrders(int customerId);
    }
}
