﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Clients;
using ClusterClient.Utils;
using Fclp;
using log4net;
using log4net.Config;

namespace ClusterClient
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            
            string[] replicaAddresses;
            if (!TryGetReplicaAddresses(args, out replicaAddresses))
                return;

            try
            {
                var clients = new ClusterClientBase[]
                {
                    new RandomClusterClient(replicaAddresses),
                    new SmartClusterClient(replicaAddresses),
                    new ConcurrentClusterClient(replicaAddresses),
                    new RoundRobinClusterClient(replicaAddresses),
                    //new BatchClusterClient(replicaAddresses, replicaAddresses.Length / 2),
                };
                //for (int i = 0; i < 2; i++)
                {
                    var queries = new[]
                    {
                        "От",
                        ///*"топота", "копыт", "пыль", "по", "полю", "летит", "На", "дворе", "трава", "на", */"траве", "дрова"
                    };

                    foreach (var client in clients)
                    {
                        Console.WriteLine("Testing {0} started", client.GetType());
                        Task.WaitAll(queries.Select(
                            async query =>
                            {
                                var timer = Stopwatch.StartNew();
                                try
                                {
                                    await client.ProcessRequestAsync(query, TimeSpan.FromSeconds(5));

                                    Console.WriteLine("Processed query \"{0}\" in {1} ms", query,
                                        timer.ElapsedMilliseconds);
                                }
                                catch (TimeoutException)
                                {
                                    Console.WriteLine("Query \"{0}\" timeout ({1} ms)", query,
                                        timer.ElapsedMilliseconds);
                                }
                                catch (ClusterClientException e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                            }).ToArray());
                        Console.WriteLine("Testing {0} finished", client.GetType());
                    }
                    //Thread.Sleep(5000);
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e);
            }
        }

        private static bool TryGetReplicaAddresses(string[] args, out string[] replicaAddresses)
        {
            var argumentsParser = new FluentCommandLineParser();
            string[] result = {};

            argumentsParser.Setup<string>('f', "file")
                .WithDescription("Path to the file with replica addresses")
                .Callback(fileName => result = File.ReadAllLines(fileName))
                .Required();

            argumentsParser.SetupHelp("?", "h", "help")
                .Callback(text => Console.WriteLine(text));

            var parsingResult = argumentsParser.Parse(args);

            if (parsingResult.HasErrors)
            {
                argumentsParser.HelpOption.ShowHelp(argumentsParser.Options);
                replicaAddresses = null;
                return false;
            }

            replicaAddresses = result;
            return !parsingResult.HasErrors;
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
    }
}
