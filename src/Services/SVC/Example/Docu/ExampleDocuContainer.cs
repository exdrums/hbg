using System;
using System.Collections.Generic;
using System.Linq;
using Common.DocuEngine;
using SVC.Example.Data;
using SVC.Example.Model;

namespace SVC.Example
{
    public class ExampleDocuContainer : DocuContainer
    {
        private readonly Context context;

        /// <summary>
        /// Enumerate all types used by the DocuEngine as objects to fill
        /// Type of this object must be ALWAYS here
        /// </summary>
        /// <value></value>
        protected override Type[] Types 
        { 
            get => new Type[]
            {
                this.GetType(),
                typeof(Location), 
                typeof(Order),
                typeof(OrderedProduct),
                typeof(Product)
            };  
        }

        #region Tables

        [DocuTable(typeof(Location), TableKeys.Locations)]
        public IEnumerable<Location> Locations { get => context.Locations; }
        [DocuTable(typeof(Product), TableKeys.Products)]
        public IEnumerable<Product> Products { get => context.Products; }
        [DocuTable(typeof(Order), TableKeys.Orders)]
        public IEnumerable<Order> Orders { get => context.Orders; }
        [DocuTable(typeof(OrderedProduct), TableKeys.OrderedProducts)]
        public IEnumerable<OrderedProduct> OrderedProducts { get => context.OrderedProducts; }
        
        #endregion
        
        #region SingleValues

        public Product Product { get => context.Products.FirstOrDefault(); }


        #endregion

        public ExampleDocuContainer(Context context): base()
        {
            // it can be some other data source, here mock
            this.context = context;
        }

    }
}