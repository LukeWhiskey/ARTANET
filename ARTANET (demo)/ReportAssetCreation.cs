using System;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IO;
using RDotNet;
using System.Collections;

namespace ARTANET__demo_
{
    public static class ReportAssetCreation
    {
        // Assets will be created using RScript NuGet Extension
        public static async void CreateReportAssets(object state)
        {
            // Cast the state object to an array of objects
            var parametersArray = (object[])state;

            // Extract the values from the array
            int second = (int)parametersArray[0];
            bool simulatedDataStream = (bool)parametersArray[1];

            // Retrieval of Connection Manager to MongoDB server
            var client = ConnectionManager.Connect();

            // Get a reference to the database
            string dbcol = simulatedDataStream ? "NetworkAnalysisAssetsTest" : "NetworkAnalysisAssets0";
            var database = client.GetDatabase(dbcol);

            // Get a reference to the collection
            var collection = database.GetCollection<BsonDocument>(dbcol);

            // Grab Recorded Data for Graphs
            var protocolCounts = await GrabRecordedData(collection);

            // Set graph type identifier
            string[] graphType = { "Bar", "Pie", "Scatter", "Heat"};

            // Bson data package
            var graphDocuments = new BsonDocument();

            // Create graph documents for each graph type
            foreach (var type in graphType)
            {
                graphDocuments.Add(type, CreateGraph(protocolCounts, type, second, simulatedDataStream));
            }

            // Add the UTC datetime to the document
            graphDocuments.Add("TrafficDataTest", BsonDateTime.Create(DateTime.UtcNow));

            // Attempt upload to MongoDB
            await UploadGraphPackage(graphDocuments, collection);

            // Empty temp folder
            string folderPath = "temp";
            EmptyTemp(folderPath);
        }

        public static BsonBinaryData CreateGraph(Dictionary<string, int> protocolCounts, string graphType, int second, bool simulatedDataStream)
        {
            // Set up R.NET
            try
            {
                REngine.SetEnvironmentVariables();
                using (REngine engine = REngine.GetInstance())
                {
                    engine.Initialize();

                    string[] simArray = SimulatedDataStream.simArray;

                    // Data to plot
                    string[] x = simArray;
                    // Extract the counts from the protocolCounts dictionary and convert them to double[]
                    double[] y = protocolCounts.Values.Select(count => (double)count).ToArray();

                    // Pass data to R
                    engine.SetSymbol("Protocol", engine.CreateCharacterVector(x));
                    engine.SetSymbol("Quantity", engine.CreateNumericVector(y));

                    // Execute the R script to generate the graph
                    try
                    {
                        engine.Evaluate(Graphs.graphType(graphType, x, y)); //Create more Rscript plot scripts
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing R script: {ex.Message}");
                    }

                    // Save the plot as an image file
                    string fileName = BsonDateTime.Create(DateTime.UtcNow) + graphType + ".png";
                    engine.Evaluate($"ggsave('temp/{fileName}', plot, device='png')");

                    // Temp created graph handling
                    var graphDocument = new BsonBinaryData(File.ReadAllBytes(fileName));

                    return graphDocument;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up REngine: {ex.Message}");
                return null; // Or handle the error in an appropriate way for your application
            }
        }

        public static async Task UploadGraphPackage(BsonDocument graphDocuments, IMongoCollection<BsonDocument> collection)
        {
            try
            {
                // Insert the document into the collection
                await collection.InsertOneAsync(graphDocuments);

                Console.WriteLine("Graphs exported and uploaded to MongoDB.");
            }
            catch (MongoException ex)
            {
                Console.WriteLine($"MongoDB Exception: {ex.Message}");
            }
        }

        public static async Task<Dictionary<string, int>> GrabRecordedData(IMongoCollection<BsonDocument> collection)
        {
            // Define a dictionary to store protocol counts
            Dictionary<string, int> protocolCounts = new Dictionary<string, int>();

            // Define sort and limit options to get the most recent 900 documents
            var sort = Builders<BsonDocument>.Sort.Descending("TrafficData"); // Replace "timestamp_field" with your actual timestamp field
            var limit = 900;

            // Query the database to retrieve the most recent 900 documents
            var filter = Builders<BsonDocument>.Filter.Empty; // Add filters as needed
            var documents = await collection.Find(filter).Sort(sort).Limit(limit).ToListAsync();

            // Iterate over each document
            foreach (var document in documents)
            {
                // Extract the protocols array
                var protocols = document["trafficWrapper"]["details"].AsBsonArray;

                // Iterate over each protocol in the array
                foreach (var protocol in protocols)
                {
                    // Extract the protocol value
                    var protocolValue = protocol["protocol"].AsString;

                    // Update the protocol count in the dictionary
                    if (protocolCounts.ContainsKey(protocolValue))
                    {
                        protocolCounts[protocolValue]++;
                    }
                    else
                    {
                        protocolCounts[protocolValue] = 1;
                    }
                }
            }

            // Print the protocol counts
            foreach (var kvp in protocolCounts)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }

            return protocolCounts;
        }

        public static void EmptyTemp(string folderPath)
        {
            // Check if the folder exists
            if (Directory.Exists(folderPath))
            {
                // Get all files in the folder
                string[] files = Directory.GetFiles(folderPath);

                // Delete each file
                foreach (string file in files)
                {
                    File.Delete(file);
                }

                Console.WriteLine("Temp emptied successfully.");
            }
            else
            {
                Console.WriteLine("Temp does not exist.");
                Directory.CreateDirectory(folderPath);
                Console.WriteLine("Creating 'temp' folder...");
                Console.WriteLine("Folder created successfully.");
            }
        }
    }
}
