using System;
using System.Collections.Generic;
using Common.DocuEngine;
using Common.Models;

namespace SVC.Example.Model
{
    [ChartEntity(typeof(Order), "svc-example/order")]
    [DocuValue(ObjectKeys.Order, PropType.Object)]
    public class Order
    {
        public Order()
        {
            this.OrderedProducts = new List<OrderedProduct>();
        }
        [DocuValue(ValueKeys.Id)]
        [ChartProperty(PropertyType.Number)]
        public int OrderId { get; set; }

        [ChartProperty(PropertyType.Number)]
        public int LocationId { get; set; }

        [DocuValue(ValueKeys.Date)]
        [ChartProperty(PropertyType.DateTime)]
        public DateTime Date { get; set; }

        [DocuValue(ValueKeys.Status)]
        [ChartProperty(PropertyType.Number)]
        public OrderStatus Status { get; set; }

        // attribute declaired for whole class, not required here
        public Location Location { get; set; }
        public ICollection<OrderedProduct> OrderedProducts { get; set; }
    }
}