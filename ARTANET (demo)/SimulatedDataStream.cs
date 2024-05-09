using System;
using System.Collections.Generic;
using System.Threading;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using static ARTANET__demo_.ReportAssetCreation;

namespace ARTANET__demo_
{
    public class SimulatedDataStream
    {
        static List<TrafficSecond> dataStream = new List<TrafficSecond>();
        public static string[] simArray = new string[] { "Ethernet", "IPv4", "IPv6", "TCP", "UDP", "ICMP", "ARP", "DNS", "HTTP", "HTTPS", "FTP", "SMTP", "POP3", "IMAP" };

        // Simulated Stream Identifier
        const bool simulatedDataStream = true;
        // Get current sec stored within latest data in db
        static int second = 0;

        public static void Simulate()
        {
            // Start timer that triggers every one second
            Timer timer = new Timer(ProcessDataStream, null, TimeSpan.FromSeconds(second), TimeSpan.FromSeconds(1));

            // Start timer that triggers every one second
            object[] parameters = { second, simulatedDataStream };
            Timer timerAssets = new Timer((state) =>
            {
                CreateReportAssets(parameters);
            }, null, TimeSpan.FromSeconds(second), TimeSpan.FromSeconds(60*15));

            // Simulate constant data stream
            SimulateDataStream();

            // Keep the program running
            Console.ReadLine();
        }
        
        public static async void ProcessDataStream(object state)
        {
            lock (dataStream)
            {
                // Increment the second
                second++;

                // Create a TrafficWrapper object
                TrafficWrapper trafficWrapper = new TrafficWrapper { seshSecond = second, details = dataStream.ToArray() };

                // Serialize and output the trafficWrapper
                string trafficJson = JsonSerializer.Serialize(trafficWrapper);
                Console.WriteLine(trafficJson);

                // Create a UTC datetime value
                DateTime utcNow = DateTime.UtcNow;

                // Convert it to BSON datetime
                BsonDateTime bsonDateTime = BsonDateTime.Create(utcNow);

                // Create a BsonDocument for MongoDB insertion
                BsonDocument trafficBson = new BsonDocument
                {
                    { "TrafficData", BsonDateTime.Create(DateTime.UtcNow) },
                    { "trafficWrapper", BsonDocument.Parse(trafficJson) }
                };

                var writeToDb = true;

                // Connect to the database asynchronously
                Task.Run(async () =>
                {
                    await MongoDBPackageData.ConnectDB(trafficBson, simulatedDataStream, writeToDb);
                }).Wait();

                // Clear the data stream
                dataStream.Clear();
            }
        }

        static void SimulateDataStream()
        {
            var ArLength = simArray;
            int itemNum = 0;

            while (true)
            {
                itemNum++;
                // Simulate data stream
                Random rand = new Random();
                int randmili = rand.Next(200, 1500);
                int randpro = rand.Next(0, ArLength.Length);
                Thread.Sleep(randmili); // Simulate data arriving every 0.5 second

                // Create a new TrafficSecond object and add it to the list
                dataStream.Add(new TrafficSecond { seshItemNum = itemNum, protocol = ArLength[randpro] });
            }
        }
    }

    public class TrafficSecond
    {
        public int seshItemNum { get; set; }
        public string protocol { get; set; }
    }

    public class TrafficWrapper
    {
        public int seshSecond { get; set; }
        public TrafficSecond[] details { get; set; }
    }
}
