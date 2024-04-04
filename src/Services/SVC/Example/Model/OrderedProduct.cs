using Common.DocuEngine;

namespace SVC.Example.Model
{
    public class OrderedProduct
    {
        [DocuValue(ValueKeys.Id)]
        public int OrderedProductId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        [DocuValue(ValueKeys.SoldPrice)]
        public decimal SoldPrice { get; set; }
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}