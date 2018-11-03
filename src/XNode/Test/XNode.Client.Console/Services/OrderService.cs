// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Client.Console.Services
{
    [ServiceProxy("OrderService", 10002)]
    public class OrderService : IOrderService
    {
        [ActionProxy("AddOrder", 1)]
        public virtual Task AddOrder(Order order)
        {
            return Task.Run(() =>
            {
                System.Console.WriteLine("AddOrder local call");
            });
        }

        [ActionProxy("QueryOrder", 2)]
        public virtual Task<List<Order>> QueryOrder(int customerId, string customerName)
        {
            return Task.Run<List<Order>>(() =>
            {
                System.Console.WriteLine("QueryOrder local call");
                return new List<Order>();
            });
        }
    }
}
