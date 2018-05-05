// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Contract.Repository;
using Contract.Service;
using Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class OrderService : IOrderService
    {
        private IOrderRepository _orderRepository;

        private IGoodsService _goodsService;

        public OrderService(IOrderRepository orderRepository,
            IGoodsService goodsService)
        {
            _orderRepository = orderRepository;
            _goodsService = goodsService;
        }

        public async Task<List<Order>> GetOrders(int customerId)
        {
            var orders = await _orderRepository.GetOrders(customerId);
            foreach (var order in orders)
            {
                foreach (var detail in order.Detail)
                {
                    detail.GoodsInfo = await _goodsService.Get(detail.GoodsId);
                }
            }
            return orders;
        }
    }
}
