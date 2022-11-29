using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography.X509Certificates;

using LiteDB;
using productsApi.src.products.productsSales;
using src.utils;

namespace productsApi.src.products.productSales
{
    public interface IProductSaleService
    {
        ProductSaleRes ProcessSale(ProductSaleReq req, bool buyMoreStock = false);
    }

    public class ProductSaleService : IProductSaleService
    {
        private string connectionString;
        private readonly long emergencyStock = 10;

        private readonly IRabbitService rabbitService;

        public ProductSaleService
        (
            IConfiguration configuration,
            IRabbitService rabbitService
        )
        {
            this.rabbitService = rabbitService;
            this.connectionString = configuration.GetConnectionString("dev");

            rabbitService.receive("sales", mesage => ProcessSale(mesage));
            rabbitService.receive("salesForce", message => ProcessSale(message, buyMoreStock: true));
        }

        private void ProcessSale(string json, bool buyMoreStock = false) 
        {
            ProductSaleReq saleReq = ProductSaleReq.FromJson(json);
            ProductSaleRes saleRes = ProcessSale(saleReq, buyMoreStock);

            rabbitService.publish(saleReq.saleId, saleRes.ToJson());
        }

        public ProductSaleRes ProcessSale(ProductSaleReq req, bool buyMoreStock = false)
        {
            ProductSaleRes res = new ProductSaleRes() { saleId = req.saleId };

            using (var lite = new LiteDatabase(connectionString))
            {
                try
                {
                    lite.BeginTrans();
                    double totalPrice = 0;
                    var collection = lite.GetCollection("Products");

                    foreach (var productSale in req.products)
                    {
                        ObjectId objectId = new ObjectId(productSale.id);
                        BsonDocument doc = collection.FindById(objectId);

                        if (doc == null) 
                        {
                            res.state = "notFound";
                            return res;
                        }

                        Product product = Product.FromDocument(doc);
                        if (product.stock < productSale.stock) 
                        {
                            if (!buyMoreStock)
                            {
                                res.state = "stockLess";
                                return res;
                            }
                            else 
                            {
                                totalPrice += product.price * productSale.stock;
                                long stockDiff = productSale.stock - product.stock;
                                product.stock = product.stock + stockDiff + emergencyStock;
                            }
                        }

                        doc["stock"] = product.stock - productSale.stock;
                        collection.Update(objectId, doc);
                    }

                    lite.Commit();
                    res.state = "success";
                    res.totalPrice = totalPrice;
                }
                catch (Exception)
                {
                    lite.Rollback();
                    res.state = "error";
                }

                lite.Dispose();
                return res;
            }
        }
    }
}
