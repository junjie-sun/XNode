// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Contract
{
    [DataContract]
    public class Order
    {
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 2)]
        public int CustomerId { get; set; }

        [DataMember(Order = 3)]
        public List<OrderDetail> Detail { get; set; }
    }

    [DataContract]
    public class OrderDetail
    {
        [DataMember(Order = 1)]
        public int OrderId { get; set; }

        [DataMember(Order = 2)]
        public int GoodsId { get; set; }

        [DataMember(Order = 3)]
        public string GoodsName { get; set; }

        [DataMember(Order = 4)]
        public decimal Price { get; set; }

        [DataMember(Order = 5)]
        public int Amount { get; set; }
    }
}
