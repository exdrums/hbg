using System;
using System.Collections.Generic;

namespace SVC.Example.Model
{
    public class MockData
    {
        public List<Location> Locations = new List<Location>(){
            new Location() { Name = "Muenchen", Address = "Some address" },
            new Location() { Name = "Nuernberg", Address = "Some address" },
            new Location() { Name = "Berlin", Address = "Some address" }
        };

        public List<Order> Orders = new List<Order>() {
            new Order() { LocationId = 1, Date = DateTime.Today, Status = OrderStatus.Status1 },
            new Order() { LocationId = 2, Date = DateTime.Today, Status = OrderStatus.Status2 },
            new Order() { LocationId = 3, Date = DateTime.Today, Status = OrderStatus.Status2 }
        };

        public List<Product> Products { get; set; } = new List<Product>() {
            new Product() { Name = "EMyFirstProduct", Description = "Product in Exemple.SVC project", Category = ProductCategory.FirstCategory, PurchasingPrice = 20.00m, SellingPrice = 45.50m, Show = true },
            new Product() { Name = "FMySecondProduct", Description = "Product in Exemple.SVC project", Category = ProductCategory.FirstCategory, PurchasingPrice = 20.00m, SellingPrice = 45.50m, Show = false },
            new Product() { Name = "BMyThirdProduct", Description = "Product in Exemple.SVC project", Category = ProductCategory.FirstCategory, PurchasingPrice = 20.00m, SellingPrice = 45.50m, Show = true },
            new Product() { Name = "AMyFourthProduct", Description = "Product in Exemple.SVC project", Category = ProductCategory.FirstCategory, PurchasingPrice = 20.00m, SellingPrice = 45.50m, Show = false },
            new Product() { Name = "CMyFifthProduct", Description = "Product in Exemple.SVC project", Category = ProductCategory.FirstCategory, PurchasingPrice = 20.00m, SellingPrice = 45.50m, Show = true },
            new Product() { Name = "DMySixthProduct", Description = "Product in Exemple.SVC project", Category = ProductCategory.FirstCategory, PurchasingPrice = 20.00m, SellingPrice = 45.50m, Show = false }
        };

        public List<OrderedProduct> OrderedProducts { get; set; }
        public MockData()
        {
            this.Locations[0].Orders.Add(Orders[0]);
            this.Locations[1].Orders.Add(Orders[1]);
            this.Locations[2].Orders.Add(Orders[2]);

            this.Orders[0].Location = this.Locations[0];
            this.Orders[1].Location = this.Locations[1];
            this.Orders[2].Location = this.Locations[2];

            var op00 = new OrderedProduct() {
                Order = this.Orders[0],
                OrderId = this.Orders[0].OrderId,
                Product = this.Products[0],
                ProductId = this.Products[0].ProductId,
                SoldPrice = this.Products[0].SellingPrice + 4
            };
            var op01 = new OrderedProduct() {
                Order = this.Orders[0],
                OrderId = this.Orders[0].OrderId,
                Product = this.Products[1],
                ProductId = this.Products[1].ProductId,
                SoldPrice = this.Products[1].SellingPrice + 4
            };
            var op02 = new OrderedProduct() {
                Order = this.Orders[0],
                OrderId = this.Orders[0].OrderId,
                Product = this.Products[2],
                ProductId = this.Products[2].ProductId,
                SoldPrice = this.Products[2].SellingPrice + 4
            };
            var op10 = new OrderedProduct() {
                Order = this.Orders[1],
                OrderId = this.Orders[1].OrderId,
                Product = this.Products[0],
                ProductId = this.Products[0].ProductId,
                SoldPrice = this.Products[0].SellingPrice + 4
            };
            var op11 = new OrderedProduct() {
                Order = this.Orders[1],
                OrderId = this.Orders[1].OrderId,
                Product = this.Products[1],
                ProductId = this.Products[1].ProductId,
                SoldPrice = this.Products[1].SellingPrice + 4
            };
            var op12 = new OrderedProduct() {
                Order = this.Orders[1],
                OrderId = this.Orders[1].OrderId,
                Product = this.Products[2],
                ProductId = this.Products[2].ProductId,
                SoldPrice = this.Products[2].SellingPrice + 4
            };
            var op20 = new OrderedProduct() {
                Order = this.Orders[2],
                OrderId = this.Orders[2].OrderId,
                Product = this.Products[0],
                ProductId = this.Products[0].ProductId,
                SoldPrice = this.Products[0].SellingPrice + 4
            };
            var op21 = new OrderedProduct() {
                Order = this.Orders[2],
                OrderId = this.Orders[2].OrderId,
                Product = this.Products[1],
                ProductId = this.Products[1].ProductId,
                SoldPrice = this.Products[1].SellingPrice + 4
            };
            
            this.Orders[0].OrderedProducts.Add(op00);
            this.Orders[0].OrderedProducts.Add(op01);
            this.Orders[0].OrderedProducts.Add(op02);
            this.Orders[1].OrderedProducts.Add(op10);
            this.Orders[1].OrderedProducts.Add(op11);
            this.Orders[1].OrderedProducts.Add(op12);
            this.Orders[2].OrderedProducts.Add(op20);
            this.Orders[2].OrderedProducts.Add(op21);

            this.OrderedProducts = new List<OrderedProduct>() {
                op00, op01, op02, op10, op11, op12, op20, op21
            };
        }

    }

}
