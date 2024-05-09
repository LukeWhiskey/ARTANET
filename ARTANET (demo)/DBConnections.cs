using MongoDB.Driver;
using System;

namespace ARTANET__demo_
{
    public class ConnectionManager
    {
        // MongoDB ConnectionString to NetworkAnalysis Database
        static string connectionString = "mongodb+srv://luke2003123:luke2003123@networkanalysiscluster0.ed3el62.mongodb.net/?retryWrites=true&w=majority";
        public static IMongoClient Connect()
        {
            // Parse the connection string and create MongoClientSettings
            var settings = MongoClientSettings.FromConnectionString(connectionString);

            // Set the ServerApi field of the settings object to set the version of the Stable API on the client
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);

            // Create a new client and connect to the server
            return new MongoClient(settings);
        }
    }
}
