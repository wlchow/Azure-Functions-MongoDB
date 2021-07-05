using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;


namespace AzureFunctionsMongoDB.Functions
{
    public class InsertDocument
    {
        private readonly MongoClient _mongoClient;
        private readonly MongoDBOptions _mongoOptions;
        private readonly IMongoCollection<BsonDocument> _collection;

        public InsertDocument(MongoClient mongoClient, IOptions<MongoDBOptions> mongoOptions)
        {
            this._mongoClient = mongoClient;
            this._mongoOptions = mongoOptions.Value;

            var database = _mongoClient.GetDatabase(_mongoOptions.DATABASE_NAME);
            this._collection = database.GetCollection<BsonDocument>(_mongoOptions.COLLECTION_NAME);
        }

        [FunctionName("InsertDocument")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "InsertDocument")] HttpRequest req, ILogger log)
        {
            log.LogInformation("InsertDocument - C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = BsonSerializer.Deserialize<BsonDocument>(requestBody);

            try
            {
                await _collection.InsertOneAsync(data);
            }
            catch (Exception ex)
            {
                log.LogInformation("InsertDocument - Error: " + ex.Message);
                return new BadRequestObjectResult("InsertDocument - Error: " + ex.Message);
            }
            return (ActionResult)new OkObjectResult("InsertDocument - Inserted Document");
        }
    }
}
