using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.Formatters;

using LiteDB;
using Newtonsoft.Json;

using src.utils;
using salesApi.src.sales.productsSales;

namespace salesApi.src.sales
{
    public interface ISaleService 
    {
        IEnumerable<SaleRes> FindAll();
        string AddNewSalePending(SaleReq sale);
        Task<SaleRes> FindAndForceUpdateSaleState(string saleId);
    }

    public class SaleService : ISaleService
    {
        public readonly string connectionString;
        public readonly IRabbitService rabbitService;
        private readonly string[] finalStates = new string[] { "success", "error", "notFound" };

        public SaleService
        (
            IConfiguration configuration,
            IRabbitService rabbitService
        ) 
        {
            this.rabbitService = rabbitService;
            this.connectionString = configuration.GetConnectionString("dev");
        }

        public IEnumerable<SaleRes> FindAll()
        {
            using (var lite = new LiteDatabase(connectionString))
            {
                var collection = lite.GetCollection("Sales");

                IEnumerable<SaleRes> sales = collection
                    .FindAll().OrderByDescending(s => s["_id"])
                    .Select(doc => SaleRes.FromDocument(doc));

                lite.Dispose();

                return sales;
            }
        }

        public Task<SaleRes> FindAndForceUpdateSaleState(string saleId) 
        {
            TaskCompletionSource<SaleRes> promise = new TaskCompletionSource<SaleRes>();

            try
            {
                using (var lite = new LiteDatabase(connectionString))
                {
                    try
                    {
                        lite.BeginTrans();
                        var collection = lite.GetCollection("Sales");
                        
                        ObjectId objectId = new(saleId);
                        var doc = collection.FindById(objectId);

                        if (doc == null) 
                        {
                            throw new Exception("saleId not Found");
                        }

                        if (finalStates.Contains(doc["state"].AsString)) 
                        {
                            throw new Exception($"sale is already {doc["state"]}");
                        }

                        string json = JsonConvert.SerializeObject(new 
                        {
                            saleId,
                            products = doc["products"].AsArray.Select(p => new ProductSale() 
                            { 
                                id = p["id"].AsString,
                                stock = long.Parse($"{p["stock"]}")
                            })
                        });

                        rabbitService.publish("salesForce", json);
                        rabbitService.receive(saleId, message =>
                        {
                            var res = ProductSaleRes.FromJson(message);
                            SaleRes saleRes = SaleRes.FromDocument(doc);

                            saleRes.state = res.state;
                            saleRes.totalPrice = res.totalPrice * 1.13;
                            collection.Update(saleRes.ToDocument());
                            
                            lite.Commit();
                            lite.Dispose();
                            
                            promise.SetResult(saleRes);
                        });
                    }
                    catch (Exception)
                    {
                        lite.Rollback();
                        lite.Dispose();
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                promise.SetException(e);
            }

            return promise.Task;
        }

        public string AddNewSalePending(SaleReq saleReq)
        {
            using (var lite = new LiteDatabase(connectionString))
            {
                try
                {
                    lite.BeginTrans();
                    var collection = lite.GetCollection<SaleRes>("Sales");

                    ObjectId objectId = ObjectId.NewObjectId();
                    SaleRes saleRes = new()
                    {
                        _id = objectId,
                        state = "pending",
                        products = saleReq.products,
                        personDoc = saleReq.personDoc,
                        personName = saleReq.personName,
                        description = saleReq.description,
                    };

                    var value = collection.Insert(saleRes);
                    if (value == null) 
                    {
                        throw new Exception("Sale Not Inserted");
                    }

                    string productSaleJson = JsonConvert.SerializeObject(new { 
                        saleId = objectId.ToString(),
                        saleReq.products,
                    });

                    rabbitService.publish("sales", productSaleJson);
                    rabbitService.receive(objectId.ToString(), message =>
                    {
                        var res = ProductSaleRes.FromJson(message);

                        saleRes.state = res.state;
                        saleRes.totalPrice = res.totalPrice;

                        collection.Update(objectId, saleRes);
                    });

                    lite.Commit();
                    lite.Dispose();

                    return objectId.ToString();
                }
                catch (Exception)
                {
                    lite.Rollback();
                    lite.Dispose();
                    throw;
                }
            }
        }
    }
}
