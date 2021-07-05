using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;


namespace AzureFunctionsMongoDB.Functions
{
    public class GetAllDocuments
    {
        private readonly MongoClient _mongoClient;
        private readonly MongoDBOptions _mongoOptions;
        private readonly IMongoCollection<BsonDocument> _collection;

        public GetAllDocuments(MongoClient mongoClient, IOptions<MongoDBOptions> mongoOptions)
        {
            this._mongoClient = mongoClient;
            this._mongoOptions = mongoOptions.Value;

            var database = _mongoClient.GetDatabase(_mongoOptions.DATABASE_NAME);
            this._collection = database.GetCollection<BsonDocument>(_mongoOptions.COLLECTION_NAME);
        }

        [FunctionName("GetAllDocuments")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetAllDocuments")] HttpRequest req, ILogger log)
        {
            log.LogInformation("GetAllDocuments- C# HTTP trigger function processed a request.");

            IActionResult returnValue = null;
            try
            {
                // Find all the documents
                var result = await _collection.Find(new BsonDocument()).ToListAsync();

                if (result == null)
                {
                    log.LogInformation("GetAllDocuments - There are no documents in this collection");
                    returnValue = new NotFoundResult();
                }
                else
                {
                    log.LogInformation("GetAllDocuments - Got all documents");
                    returnValue = new OkObjectResult(result.ToJson());
                }

            }
            catch (Exception ex)
            {
                log.LogInformation("GetAllDocuments - Error: " + ex.Message);
                returnValue = new BadRequestObjectResult("GetAllDocuments - Error: " + ex.Message);
            }
            return returnValue;
        }
    }
}
