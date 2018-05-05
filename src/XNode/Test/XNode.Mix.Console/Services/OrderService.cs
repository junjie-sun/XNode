// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XNode.Mix.Console.Services
{
    public class OrderService : IOrderService
    {
        private static ConcurrentBag<Order> orders = new ConcurrentBag<Order>();

        public Task AddOrder(Order order)
        {
            return Task.Run(() =>
            {
                orders.Add(order);
            });
        }

        public Task<List<Order>> QueryOrder(int customerId, string customerName)
        {
            return Task.Run<List<Order>>(() =>
            {
                return orders.Where(o => o.CustomerId == customerId && o.CustomerName == customerName)
                    .OrderBy((c) => c.Id).ToList();
            });
        }
    }
}
