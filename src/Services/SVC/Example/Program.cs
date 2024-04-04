using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Common.DocuEngine;
using SVC.Example.Model;
using System.IO;
using Microsoft.Extensions.FileProviders;
using SVC.Example.Data;
using Microsoft.EntityFrameworkCore;

namespace SVC.Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

            // // EF options
            // DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>()
            //    .UseInMemoryDatabase("svc-example")
            //    .Options;

            // using(var context = new Context(options)) {
            //    var mock = new MockData();
            //    // Fill context
            //    context.Locations.AddRange(mock.Locations);
            //    context.Orders.AddRange(mock.Orders);
            //    context.Products.AddRange(mock.Products);
            //    context.OrderedProducts.AddRange(mock.OrderedProducts);
            //    context.SaveChanges();

            //    // Create container for the docu
            //    var container = new ExampleDocuContainer(context);
            //    Template template;
            //    var fileName = "template.docx";
            //    var resultFile = "result.docx";

            //    // GetTemplate for the docu
            //    PhysicalFileProvider provider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
            //    var file = provider.GetFileInfo(fileName);

            //    using (var memory = new MemoryStream())
            //    using (var stream = file.CreateReadStream()) {
            //        stream.CopyTo(memory);

            //        template = new Template(fileName, memory);

            //        var docu = DocuFactory.CreateDocu(container, template);
            //        docu.FillDocu();

            //        File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), resultFile), memory.ToArray());

            //    }
            // }


        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
