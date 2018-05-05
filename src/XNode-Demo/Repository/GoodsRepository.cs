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
    public class GoodsRepository : IGoodsRepository
    {
        #region Data

        private IList<Goods> Goods
        {
            get
            {
                return new List<Goods>()
                {
                    new Goods()
                    {
                        Id = 1,
                        Name = "Goods01",
                        Price = 10.5M
                    },
                    new Goods()
                    {
                        Id = 2,
                        Name = "Goods02",
                        Price = 22
                    }
                };
            }
        }

        #endregion

        public Task<Goods> Get(int id)
        {
            return Task.FromResult(Goods.Where(g => g.Id == id).SingleOrDefault());
        }

        public Task<List<Goods>> GetAll()
        {
            return Task.FromResult(Goods.ToList());
        }
    }
}
