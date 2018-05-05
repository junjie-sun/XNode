// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repository
{
    public interface IGoodsRepository
    {
        Task<List<Goods>> GetAll();

        Task<Goods> Get(int id);
    }
}
