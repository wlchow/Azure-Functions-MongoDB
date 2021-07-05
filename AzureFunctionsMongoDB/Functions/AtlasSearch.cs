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
    public class AtlasSearch
    {
        private readonly MongoClient _mongoClient;
        private readonly MongoDBOptions _mongoOptions;
        private readonly IMongoCollection<BsonDocument> _collection;

        public AtlasSearch(MongoClient mongoClient, IOptions<MongoDBOptions> mongoOptions)
        {
            this._mongoClient = mongoClient;
            this._mongoOptions = mongoOptions.Value;

            var database = _mongoClient.GetDatabase("sample_mflix");
            this._collection = database.GetCollection<BsonDocument>("movies");
        }

        [FunctionName("AtlasSearch")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "AtlasSearch/{query}")] HttpRequest req, string query,
            ILogger log)
        {
            log.LogInformation("AtlasSearch - C# HTTP trigger function processed a request.");

            IActionResult returnValue = null;
            try
            {
                /* Atlas Search Aggregation Pipeline to run:
                    db.movies.aggregate(
                        [{$search: {
                          text: {
                            query: '<query>',
                            path: 'fullplot',
                            fuzzy: {
                              maxEdits: 2,
                              prefixLength: 0
                            }
                          }
                        }}, {$project: {
                          _id: 0,
                          title: 1,
                          year: 1,
                          fullplot: 1,
                          score: {
                            $meta: 'searchScore'
                          }
                        }}, {$limit: 15}]
                */

                PipelineDefinition<BsonDocument, BsonDocument> pipeline = new BsonDocument[]
                    {
                        new BsonDocument("$search",
                            new BsonDocument("text",
                                new BsonDocument
                                        {
                                            { "query", query },
                                            { "path", "fullplot" },
                                            { "fuzzy",
                                new BsonDocument
                                            {
                                                { "maxEdits", 2 },
                                                { "prefixLength", 0 }
                                            } }
                                        })),
                        new BsonDocument("$project",
                            new BsonDocument
                                {
                                    { "_id", 0 },
                                    { "title", 1 },
                                    { "year", 1 },
                                    { "fullplot", 1 },
                                    { "score",
                            new BsonDocument("$meta", "searchScore") }
                                }),
                        new BsonDocument("$limit", 15)
                    };

                // Find all the documents
                var result = await _collection.Aggregate(pipeline).ToListAsync();

                if (result == null)
                {
                    log.LogInformation("AtlasSearch - There are no documents in this collection");
                    returnValue = new NotFoundResult();
                }
                else
                {
                    log.LogInformation("AtlasSearch - Searched all documents");
                    returnValue = new OkObjectResult(result.ToJson());
                }

            }
            catch (Exception ex)
            {
                log.LogInformation("AtlasSearch - Error: " + ex.Message);
                returnValue = new BadRequestObjectResult("AtlasSearch - Error: " + ex.Message);
            }
            return returnValue;
        }
    }
}
