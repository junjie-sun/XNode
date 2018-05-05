// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Server.Route
{
    public class RouteNotFoundException : Exception
    {
        public int ServiceId { get; set; }

        public int ActionId { get; set; }

        public RouteNotFoundException(int serviceId, int actionId)
            : base($"Route not found. ServiceId={serviceId}, ActionId={actionId}")
        {
            ServiceId = serviceId;
            ActionId = actionId;
        }
    }
}
