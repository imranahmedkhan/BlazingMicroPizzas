﻿using BlazingPizza.ComponentsLibrary.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazingPizza.Orders
{
    public class OrderWithStatus
    {
        public Order Order { get; set; }

        public string StatusText { get; set; }

        public List<Marker> MapMarkers { get; set; }

        public int Progress { get; set; }

        public static OrderWithStatus FromOrder(Order order)
        {
            if (order == null) return null;

            // To simulate a real backend process, we fake status updates based on the amount
            // of time since the order was placed
            string statusText;
            List<Marker> mapMarkers;
            var dispatchTime = order.CreatedTime.AddSeconds(10);
            var deliveryDuration = TimeSpan.FromMinutes(1); // Unrealistic, but more interesting to watch
            int progress = 0;

            if (DateTime.UtcNow < dispatchTime)
            {
                statusText = "Preparing";
                progress = 33;
                mapMarkers = new List<Marker>
                {
                    ToMapMarker("You", order.DeliveryLocation, showPopup: true)
                };
            }
            else if (DateTime.UtcNow < dispatchTime + deliveryDuration)
            {
                statusText = "Out for delivery";
                progress = 66;

                var startPosition = ComputeStartPosition(order);
                var proportionOfDeliveryCompleted = Math.Min(1, (DateTime.Now - dispatchTime).TotalMilliseconds / deliveryDuration.TotalMilliseconds);
                var driverPosition = LatLong.Interpolate(startPosition, order.DeliveryLocation, proportionOfDeliveryCompleted);
                mapMarkers = new List<Marker>
                {
                    ToMapMarker("You", order.DeliveryLocation),
                    ToMapMarker("Driver", driverPosition, showPopup: true),
                };
            }
            else
            {
                statusText = "Delivered";
                progress = 100;
                mapMarkers = new List<Marker>
                {
                    ToMapMarker("Delivery location", order.DeliveryLocation, showPopup: true),
                };
            }

            return new OrderWithStatus
            {
                Order = order,
                StatusText = statusText,
                MapMarkers = mapMarkers,
                Progress = progress
            };
        }

        private static LatLong ComputeStartPosition(Order order)
        {
            var rng = new Random();
            var distance = 0.01 + rng.NextDouble() * 0.02;
            var angle = rng.NextDouble() * Math.PI * 2;
            var offset = (distance * Math.Cos(angle), distance * Math.Sin(angle));
            return new LatLong(order.DeliveryLocation.Latitude + offset.Item1, order.DeliveryLocation.Longitude + offset.Item2);
        }

        static Marker ToMapMarker(string description, LatLong coords, bool showPopup = false)
            => new Marker { Description = description, X = coords.Longitude, Y = coords.Latitude, ShowPopup = showPopup };
    }
}
