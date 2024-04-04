using System.Collections.Generic;
using Common.DocuEngine;
using Common.Models;

namespace SVC.Example.Model
{
    [DocuValue(ObjectKeys.Location, PropType.Object)]
    [ChartEntity(typeof(Location), "svc-example/location")]
    public class Location
    {
        public Location()
        {
            this.Orders = new List<Order>();
        }
        [DocuValue(ValueKeys.Id)]
        [ChartProperty(PropertyType.Number)]
        public int LocationId { get; set; }

        [DocuValue(ValueKeys.Name)]
        [ChartProperty(PropertyType.String)]
        public string Name { get; set; }

        [DocuValue(ValueKeys.Address)]
        [ChartProperty(PropertyType.String)]
        public string Address { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}