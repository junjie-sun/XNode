// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Contract.Repository;
using Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    public class OrderRepository : IOrderRepository
    {
        #region Data

        private IList<Order> Orders
        {
            get
            {
                return new List<Order>()
                {
                    new Order()
                    {
                        Id = 1,
                        CustomerId = 1,
                        Detail = new List<OrderDetail>()
                        {
                            new OrderDetail()
                            {
                                GoodsId = 1,
                                Amount = 5,
                                Discount = 1
                            },
                            new OrderDetail()
                            {
                                GoodsId = 2,
                                Amount = 2,
                                Discount = 0.8M
                            }
                        }
                    },
                    new Order()
                    {
                        Id = 2,
                        CustomerId = 1,
                        Detail = new List<OrderDetail>()
                        {
                            new OrderDetail()
                            {
                                GoodsId = 1,
                                Amount = 10,
                                Discount = 0.5M
                            }
                        }
                    },
                    new Order()
                    {
                        Id = 3,
                        CustomerId = 2,
                        Detail = new List<OrderDetail>()
                        {
                            new OrderDetail()
                            {
                                GoodsId = 2,
                                Amount = 25,
                                Discount = 0.6M
                            }
                        }
                    }
                };
            }
        }

        #endregion

        public Task<List<Order>> GetOrders(int customerId)
        {
            return Task.FromResult(Orders.Where(o => o.CustomerId == customerId).ToList());
        }
    }
}
