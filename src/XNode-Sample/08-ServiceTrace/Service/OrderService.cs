// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XNode.Server;

namespace Service
{
    public class OrderService : IOrderService
    {
        #region Data

        private IList<Order> orders = new List<Order>()
        {
            new Order()
            {
                Id = 1,
                CustomerId = 1,
                Detail = new List<OrderDetail>()
                {
                    new OrderDetail()
                    {
                        OrderId = 1,
                        GoodsId = 1,
                        GoodsName = "A",
                        Price = 12,
                        Amount = 10
                    },
                    new OrderDetail()
                    {
                        OrderId = 1,
                        GoodsId = 2,
                        GoodsName = "B",
                        Price = 26.5M,
                        Amount = 1
                    },
                    new OrderDetail()
                    {
                        OrderId = 1,
                        GoodsId = 3,
                        GoodsName = "C",
                        Price = 5.5M,
                        Amount = 15
                    }
                }
            },
            new Order()
            {
                Id = 2,
                CustomerId = 2,
                Detail = new List<OrderDetail>()
                {
                    new OrderDetail()
                    {
                        OrderId = 2,
                        GoodsId = 1,
                        GoodsName = "A",
                        Price = 12M,
                        Amount = 3
                    }
                }
            },
            new Order()
            {
                Id = 3,
                CustomerId = 1,
                Detail = new List<OrderDetail>()
                {
                    new OrderDetail()
                    {
                        OrderId = 3,
                        GoodsId = 1,
                        GoodsName = "C",
                        Price = 5.5M,
                        Amount = 5
                    }
                }
            }
        };

        #endregion

        public Task<List<Order>> GetOrders(int customerId)
        {
            return Task.FromResult(orders.Where(o => o.CustomerId == customerId).ToList());
        }
    }
}
