// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.DTO
{
    public class CustomerDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<OrderDTO> Orders { get; set; }

        public static CustomerDTO From(Customer customer)
        {
            if (customer == null)
            {
                return null;
            }

            return new CustomerDTO()
            {
                Id = customer.Id,
                Name = customer.Name,
                Orders = OrderDTO.From(customer.Orders)
            };
        }

        public static List<CustomerDTO> From(List<Customer> list)
        {
            if (list == null)
            {
                return null;
            }

            var result = new List<CustomerDTO>();

            foreach (var customer in list)
            {
                result.Add(From(customer));
            }

            return result;
        }
    }
}
