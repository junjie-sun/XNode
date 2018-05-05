// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Client.Console.Services
{
    //[ServiceProxy(10002)]
    public interface IOrderService
    {
        //[ActionProxy(1)]
        Task AddOrder(Order order);

        //[ActionProxy(2)]
        Task<List<Order>> QueryOrder(int customerId, string customerName);
    }
}
