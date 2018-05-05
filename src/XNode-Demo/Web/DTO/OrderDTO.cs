// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.DTO
{
    public class OrderDTO
    {
        public int Id { get; set; }

        public List<OrderDetailDTO> Detail { get; set; }

        public static OrderDTO From(Order order)
        {
            if (order == null)
            {
                return null;
            }

            return new OrderDTO()
            {
                Id = order.Id,
                Detail = OrderDetailDTO.From(order.Detail)
            };
        }

        public static List<OrderDTO> From(List<Order> list)
        {
            if (list == null)
            {
                return null;
            }

            var result = new List<OrderDTO>();

            foreach (var order in list)
            {
                result.Add(From(order));
            }

            return result;
        }
    }

    public class OrderDetailDTO
    {
        public int GoodsId { get; set; }

        public decimal Discount { get; set; }

        public int Amount { get; set; }

        public GoodsDTO GoodsInfo { get; set; }

        public decimal Price
        {
            get
            {
                return GoodsInfo.Price * Discount;
            }
        }

        public static OrderDetailDTO From(OrderDetail detail)
        {
            if (detail == null)
            {
                return null;
            }

            return new OrderDetailDTO()
            {
                Amount = detail.Amount,
                Discount = detail.Discount,
                GoodsId = detail.GoodsId,
                GoodsInfo = GoodsDTO.From(detail.GoodsInfo)
            };
        }

        public static List<OrderDetailDTO> From(List<OrderDetail> list)
        {
            if (list == null)
            {
                return null;
            }

            var result = new List<OrderDetailDTO>();

            foreach (var detail in list)
            {
                result.Add(From(detail));
            }

            return result;
        }
    }
}
