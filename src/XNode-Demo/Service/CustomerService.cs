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
    public class CustomerService : ICustomerService
    {
        private ICustomerRepository _customerRepository;

        private IOrderService _orderService;

        public CustomerService(ICustomerRepository customerRepository,
            IOrderService orderService)
        {
            _customerRepository = customerRepository;
            _orderService = orderService;
        }

        public async Task<Customer> Get(int customerId)
        {
            var customer = await _customerRepository.Get(customerId);
            customer.Orders = await _orderService.GetOrders(customerId);
            return customer;
        }

        public Task<List<Customer>> GetAll()
        {
            return _customerRepository.GetAll();
        }
    }
}
