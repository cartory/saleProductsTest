using LiteDB;
using System.Data;

namespace productsApi.src.products
{
    public interface IProductService
    {
        public Product FindById(string id);
        public IEnumerable<Product> FindAll();

        public bool Delete(string id);
        public bool Create(Product entity);
        public bool Update(string id, Product entity);
    }

    public class ProductService : IProductService
    {
        private string connectionString;

        public ProductService(IConfiguration configuration) 
        {
            this.connectionString = configuration.GetConnectionString("dev");
        }

        public IEnumerable<Product> FindAll()
        {
            using (LiteDatabase liteDb = new LiteDatabase(connectionString))
            {
                var collection = liteDb.GetCollection("Products");

                IEnumerable<Product> products = collection
                    .FindAll().OrderByDescending(p => p["_id"])
                    .Select(doc => Product.FromDocument(doc));

                liteDb.Dispose();

                return products;
            }
        }

        public Product FindById(string id) 
        {
            using (LiteDatabase liteDb = new LiteDatabase(connectionString))
            {
                var collection = liteDb.GetCollection("Products");
                
                ObjectId objectId = new ObjectId(id);
                BsonDocument doc = collection.FindById(objectId);

                liteDb.Dispose();

                return doc != null ? Product.FromDocument(doc) : null;
            }
        }

        public bool Create(Product product)
        {
            using (LiteDatabase liteDb = new LiteDatabase(connectionString))
            {
                product._id = ObjectId.NewObjectId();

                var collection = liteDb.GetCollection<Product>("Products");
                BsonValue value = collection.Insert(product);
                
                liteDb.Dispose();
                return value != null;
            }
        }

        public bool Update(string id, Product product)
        {
            using (LiteDatabase liteDb = new LiteDatabase(connectionString))
            {
                ILiteCollection<Product> collection = liteDb.GetCollection<Product>("Products");

                ObjectId objectId = new ObjectId(id);
                bool updated = collection.Update(objectId, product);

                liteDb.Dispose();
                return updated;
            }
        }

        public bool Delete(string id) 
        {
            using (LiteDatabase liteDb = new LiteDatabase(connectionString))
            {
                ILiteCollection<Product> collection = liteDb.GetCollection<Product>("Products");

                ObjectId objectId = new ObjectId(id);
                bool deleted = collection.Delete(objectId);

                liteDb.Dispose();
                return deleted;
            }
        }
    }
}
