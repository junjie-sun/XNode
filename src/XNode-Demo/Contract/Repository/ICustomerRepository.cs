// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Repository
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAll();

        Task<Customer> Get(int id);
    }
}
