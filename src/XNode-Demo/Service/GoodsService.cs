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
    public class GoodsService : IGoodsService
    {
        private IGoodsRepository _goodsRepository;

        public GoodsService(IGoodsRepository goodsRepository)
        {
            _goodsRepository = goodsRepository;
        }

        public Task<Goods> Get(int goodsId)
        {
            return _goodsRepository.Get(goodsId);
        }

        public Task<List<Goods>> GetAll()
        {
            return _goodsRepository.GetAll();
        }
    }
}
