// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Client;
using XNode.Server;

namespace Contract.Service
{
    [Service("GoodService", 10001)]
    [ServiceProxy("GoodService", 10001)]
    public interface IGoodsService
    {
        [Action("GetAllGoods", 1)]
        [ActionProxy("GetAllGoods", 1)]
        Task<List<Goods>> GetAll();

        [Action("GetGoods", 2)]
        [ActionProxy("GetGoods", 2)]
        Task<Goods> Get(int goodsId);
    }
}
