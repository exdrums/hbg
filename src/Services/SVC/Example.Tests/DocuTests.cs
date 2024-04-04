using Common.DocuEngine;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SVC.Example;
using SVC.Example.Data;
using SVC.Example.Model;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Example.Tests
{
    public class DocuTests
    {
        private readonly ITestOutputHelper output;

        public DocuTests(ITestOutputHelper output)
        {
            this.output = output;
        }
        [Fact]
        public void WordDocu()
        {
            Logs.ResetLogger();
            // EF options
            DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase("hbg-example")
                .Options;

            using (var context = new Context(options))
            {
                var mock = new MockData();
                // Fill context
                context.Locations.AddRange(mock.Locations);
                context.Orders.AddRange(mock.Orders);
                context.Products.AddRange(mock.Products);
                context.OrderedProducts.AddRange(mock.OrderedProducts);
                context.SaveChanges();

                // Create container for the docu
                var container = new ExampleDocuContainer(context);
                Template template;
                var fileName = "template.docx";
                var resultFile = "result.docx";

                // GetTemplate for the docu
                PhysicalFileProvider provider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
                var file = provider.GetFileInfo(fileName);

                using (var memory = new MemoryStream())
                using (var stream = file.CreateReadStream())
                {
                    stream.CopyTo(memory);

                    template = new Template(fileName, memory);

                    var docu = DocuFactory.CreateDocu(container, template);
                    docu.FillDocu();

                    File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), resultFile), memory.ToArray());

                }

                foreach(var log in Logs.Logger.GetAllLogs()) {
                    output.WriteLine(log);
                }

                Assert.Empty(Logs.Logger.GetExceptions());
            }
        }
    }
}
