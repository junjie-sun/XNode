// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.DTO
{
    public class GoodsDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public static GoodsDTO From(Goods goods)
        {
            if (goods == null)
            {
                return null;
            }

            return new GoodsDTO()
            {
                Id = goods.Id,
                Name = goods.Name,
                Price = goods.Price
            };
        }

        public static List<GoodsDTO> From(List<Goods> list)
        {
            if (list == null)
            {
                return null;
            }

            var result = new List<GoodsDTO>();

            foreach (var goods in list)
            {
                result.Add(From(goods));
            }

            return result;
        }
    }
}
