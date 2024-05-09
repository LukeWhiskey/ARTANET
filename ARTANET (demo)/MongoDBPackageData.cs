using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;

namespace ARTANET__demo_
{
    public class MongoDBPackageData
    {
        public static async Task ConnectDB(BsonDocument trafficBson, bool simulatedDataStream, bool writeToDb)
        {
            // Retrieval of Connection Manager to MongoDB server
            var client = ConnectionManager.Connect();

            // Get a reference to the database
            string dbcol = simulatedDataStream ? "NetworkAnalysisTest" : "NetworkAnalysis0";
            var database = client.GetDatabase(dbcol);

            // Get a reference to the collection
            var collection = database.GetCollection<BsonDocument>(dbcol);

            if (writeToDb)
            {
                try
                {
                    // Package Data
                    await PackageData(trafficBson, collection);

                    Console.WriteLine("Document inserted successfully.");
                }
                catch (MongoException ex)
                {
                    Console.WriteLine($"MongoDB Exception: {ex.Message}");
                }
            }
        }

        public static async Task PackageData(BsonDocument trafficBson, IMongoCollection<BsonDocument> collection)
        {
            // Insert the document into the collection
            await collection.InsertOneAsync(trafficBson);
        }
    }
}
