// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XNode.Mix.Console.Services
{
    public class CustomerService : ICustomerService
    {
        private static ConcurrentBag<Customer> customers = new ConcurrentBag<Customer>();

        private IOrderService orderService;

        public CustomerService(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        public Task AddCustomer(Customer customer)
        {
            return Task.Run(() =>
            {
                customers.Add(customer);
            });
        }

        public Customer GetCustomer(int id)
        {
            return customers.Where((customer) => customer.Id == id).FirstOrDefault();
        }

        public Task<List<Customer>> QueryCustomer()
        {
            return Task.Run<List<Customer>>(() =>
            {
                return customers.OrderBy((c) => c.Id).ToList();
            });
        }

        public void RemoveCustomer(int id)
        {
            var customer = customers.Where((c) => c.Id == id).FirstOrDefault();
            if (customer == null)
            {
                return;
            }
            customers.TryTake(out customer);
        }

        public void RemoveAllCustomer()
        {
            customers = new ConcurrentBag<Customer>();
        }

        public Task<List<Order>> GetOrders(int customerId)
        {
            var customer = GetCustomer(customerId);
            return orderService.QueryOrder(customer.Id, customer.Name);
        }

        public Task<bool> SaveCustomerPhoto(byte[] data)
        {
            if (data == null)
            {
                System.Console.WriteLine("Data is null.");
                return Task.FromResult(false);
            }

            if (data.Length != 10)
            {
                System.Console.WriteLine("Data's length is not equal 10.");
                return Task.FromResult(false);
            }

            for (var i = 0; i < 10; i++)
            {
                if (data[i] == i % 2)
                {
                    System.Console.WriteLine($"Mod 2 error. i={i}, data={data[i]}");
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(true);
        }

        public Task<byte[]> GetCustomerPhoto()
        {
            var result = new byte[] { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 };
            return Task.FromResult(result);
        }
    }
}
