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
    public class CustomerRepository : ICustomerRepository
    {
        #region Data

        private IList<Customer> Customers
        {
            get
            {
                return new List<Customer>()
                {
                    new Customer()
                    {
                        Id = 1,
                        Name = "Customer01"
                    },
                    new Customer()
                    {
                        Id = 2,
                        Name = "Customer02"
                    },
                    new Customer()
                    {
                        Id = 3,
                        Name = "Customer03"
                    }
                };
            }
        }

        #endregion

        public Task<Customer> Get(int id)
        {
            return Task.FromResult(Customers.Where(c => c.Id == id).SingleOrDefault());
        }

        public Task<List<Customer>> GetAll()
        {
            return Task.FromResult(Customers.ToList());
        }
    }
}
