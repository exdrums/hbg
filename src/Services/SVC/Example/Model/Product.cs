using System.Collections.Generic;
using Common.DocuEngine;

namespace SVC.Example.Model
{
    [DocuValue(ObjectKeys.Product, PropType.Object)]
    public class Product
    {
        public Product()
        {
            this.OrderedProducts = new List<OrderedProduct>();
        }
        [DocuValue(ValueKeys.Id)]
        public int ProductId { get; set; }
        [DocuValue(ValueKeys.Name)]
        public string Name { get; set; }
        [DocuValue(ValueKeys.Description)]
        public string Description { get; set; }
        [DocuValue(ValueKeys.Category)]
        public ProductCategory Category { get; set; }
        [DocuValue(ValueKeys.PurchasingPrice)]
        public decimal PurchasingPrice { get; set; }
        [DocuValue(ValueKeys.SellingPrice)]
        public decimal SellingPrice { get; set; }
        [DocuValue(ValueKeys.Img, PropType.ImageUrl)]
        public string ImageUrl { get; set; } = "https://www.gstatic.com/webp/gallery/1.jpg";
        [DocuValue(ValueKeys.Show)]
        public bool Show { get; set; } = false;

        public ICollection<OrderedProduct> OrderedProducts { get; set; }
    }
}