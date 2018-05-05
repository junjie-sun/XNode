// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service
{
    public class CustomerService : ICustomerService
    {
        #region Data

        private IList<Customer> customers = new List<Customer>()
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

        #endregion

        private IOrderService _orderService;

        public CustomerService(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public Task<Customer> GetCustomers(int customerId)
        {
            var customer = customers.Where(c => c.Id == customerId).SingleOrDefault();

            if (customer == null)
            {
                return Task.FromResult<Customer>(null);
            }

            customer.Orders = _orderService.GetOrders(customerId).Result;

            return Task.FromResult(customer);
        }
    }
}
