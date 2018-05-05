// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Server;

namespace Server
{
    [Service("SampleService", 10001, true)]
    public interface ISampleService
    {
        [Action("Welcome", 1)]
        string Welcome(Name name);

        [Action("GetOrders", 2)]
        Task<IList<Order>> GetOrders();
    }

    public class SampleService : ISampleService
    {
        public string Welcome(Name name)
        {
            return $"Hello {name.FirstName} {name.LastName}";
        }

        public Task<IList<Order>> GetOrders()
        {
            return Task.FromResult<IList<Order>>(new List<Order>()
            {
                new Order()
                {
                    Id = 1,
                    CustomerId = 1,
                    CustomerName = "Customer1",
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
                    CustomerName = "Customer2",
                    Detail = new List<OrderDetail>()
                    {
                        new OrderDetail()
                        {
                            OrderId = 2,
                            GoodsId = 1,
                            GoodsName = "A",
                            Price = 12,
                            Amount = 3
                        }
                    }
                }
            });
        }
    }
}
