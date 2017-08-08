﻿using Dotmim.Sync;
using Dotmim.Sync.Builders;
using Dotmim.Sync.Data;
using Dotmim.Sync.Data.Surrogate;
using Dotmim.Sync.Enumerations;
using Dotmim.Sync.Proxy;
using Dotmim.Sync.SampleConsole;
using Dotmim.Sync.SQLite;
using Dotmim.Sync.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {

        

        //TestSync().Wait();

        //TestSyncThroughKestrellAsync().Wait();

        //TestAllAvailablesColumns().Wait();

        TestSyncThroughWebApi().Wait();

        Console.ReadLine();

    }


    private static async Task TestSyncThroughWebApi()
    {
        ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("config.json", true);
        IConfiguration Configuration = configurationBuilder.Build();
        var clientConfig = Configuration["AppConfiguration:ClientConnectionString"];

        var clientProvider = new SqlSyncProvider(clientConfig);
        var proxyClientProvider = new WebProxyClientProvider(new Uri("http://localhost:56782/api/values"));

        var agent = new SyncAgent(clientProvider, proxyClientProvider);

        Console.WriteLine("Press a key to start...");
        Console.ReadKey();
        do
        {
            Console.Clear();
            Console.WriteLine("Sync Start");
            try
            {
                var s = await agent.SynchronizeAsync();
            }
            catch (SyncException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("UNKNOW EXCEPTION : " + e.Message);
            }


            Console.WriteLine("Sync Ended. Press a key to start again, or Escapte to end");
        } while (Console.ReadKey().Key != ConsoleKey.Escape);

        Console.WriteLine("End");

    }

  
    private static async Task FilterSync()
    {
        // Get SQL Server connection string
        ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("config.json", true);
        IConfiguration Configuration = configurationBuilder.Build();
        var serverConfig = Configuration["AppConfiguration:ServerFilteredConnectionString"];
        var clientConfig = "sqlitefiltereddb.db";

        SqlSyncProvider serverProvider = new SqlSyncProvider(serverConfig);
        SQLiteSyncProvider clientProvider = new SQLiteSyncProvider(clientConfig);

        // With a config when we are in local mode (no proxy)
        SyncConfiguration configuration = new SyncConfiguration(new string[] { "ServiceTickets" });
        //configuration.DownloadBatchSizeInKB = 500;
        configuration.UseBulkOperations = false;
        // Adding filters on schema
        configuration.Filters.Add("ServiceTickets", "CustomerID");

        SyncAgent agent = new SyncAgent(clientProvider, serverProvider, configuration);

        // Adding a parameter for this agent
        agent.Parameters.Add("ServiceTickets", "CustomerID", 1);

        do
        {
            Console.Clear();
            Console.WriteLine("Sync Start");
            try
            {
                var s = await agent.SynchronizeAsync();

            }
            catch (SyncException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("UNKNOW EXCEPTION : " + e.Message);
            }


            Console.WriteLine("Sync Ended. Press a key to start again, or Escapte to end");
        } while (Console.ReadKey().Key != ConsoleKey.Escape);

        Console.WriteLine("End");
    }

    private static async Task TestSyncSQLite()
    {
        // Get SQL Server connection string
        ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("config.json", true);
        IConfiguration Configuration = configurationBuilder.Build();
        var serverConfig = Configuration["AppConfiguration:ServerConnectionString"];
        var clientConfig = Configuration["AppConfiguration:ClientSQLiteConnectionString"];
        var clientConfig2 = Configuration["AppConfiguration:ClientSQLiteConnectionString2"];
        var clientConfig3 = Configuration["AppConfiguration:ClientConnectionString"];

        SqlSyncProvider serverProvider = new SqlSyncProvider(serverConfig);
        SQLiteSyncProvider clientProvider = new SQLiteSyncProvider(clientConfig);
        SQLiteSyncProvider clientProvider2 = new SQLiteSyncProvider(clientConfig2);
        SqlSyncProvider clientProvider3 = new SqlSyncProvider(clientConfig3);

        // With a config when we are in local mode (no proxy)
        SyncConfiguration configuration = new SyncConfiguration(new string[] { "ServiceTickets" });
        //configuration.DownloadBatchSizeInKB = 500;
        configuration.UseBulkOperations = false;

        SyncAgent agent = new SyncAgent(clientProvider, serverProvider, configuration);
        SyncAgent agent2 = new SyncAgent(clientProvider2, serverProvider, configuration);
        SyncAgent agent3 = new SyncAgent(clientProvider3, serverProvider, configuration);

        agent.SyncProgress += SyncProgress;
        agent2.SyncProgress += SyncProgress;
        agent3.SyncProgress += SyncProgress;
        // agent.ApplyChangedFailed += ApplyChangedFailed;

        do
        {
            Console.Clear();
            Console.WriteLine("Sync Start");
            try
            {
                var s = await agent.SynchronizeAsync();
                var s2 = await agent2.SynchronizeAsync();
                var s3 = await agent3.SynchronizeAsync();

            }
            catch (SyncException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("UNKNOW EXCEPTION : " + e.Message);
            }


            Console.WriteLine("Sync Ended. Press a key to start again, or Escapte to end");
        } while (Console.ReadKey().Key != ConsoleKey.Escape);

        Console.WriteLine("End");
    }

    private static async Task TestSync()
    {
        // Get SQL Server connection string
        ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("config.json", true);
        IConfiguration Configuration = configurationBuilder.Build();
        var serverConfig = Configuration["AppConfiguration:ServerConnectionString"];
        var clientConfig = Configuration["AppConfiguration:ClientConnectionString"];

        SqlSyncProvider serverProvider = new SqlSyncProvider(serverConfig);
        SqlSyncProvider clientProvider = new SqlSyncProvider(clientConfig);

        // With a config when we are in local mode (no proxy)
        SyncConfiguration configuration = new SyncConfiguration(new string[] { "ServiceTickets" });

        // Configure the default resolution priority
        // Default : Server wins
        configuration.ConflictResolutionPolicy = ConflictResolutionPolicy.ServerWins;
        // Configure the batch size when memory is limited.
        // Default : 0. Batch is disabled
        configuration.DownloadBatchSizeInKB = 1000;
        // Configure the batch directory if batch size is specified
        // Default : Windows tmp folder
        configuration.BatchDirectory = "D://tmp";
        // configuration is stored in memory, you can disable this behavior
        // Default : false
        configuration.OverwriteConfiguration = true;
        // Configure the default serialization mode (Json or Binary)
        // Default : Json
        configuration.SerializationFormat = SerializationFormat.Json;
        // Configure the default model to Insert / Update / Delete rows (SQL Server use TVP to bulk insert)
        // Default true if SQL Server
        configuration.UseBulkOperations = true;

        //configuration.DownloadBatchSizeInKB = 500;
        SyncAgent agent = new SyncAgent(clientProvider, serverProvider, configuration);

        agent.SyncProgress += SyncProgress;
        agent.ApplyChangedFailed += ApplyChangedFailed;

        do
        {
            Console.Clear();
            Console.WriteLine("Sync Start");
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                CancellationToken token = cts.Token;
                var s = await agent.SynchronizeAsync(token);

            }
            catch (SyncException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("UNKNOW EXCEPTION : " + e.Message);
            }


            Console.WriteLine("Sync Ended. Press a key to start again, or Escapte to end");
        } while (Console.ReadKey().Key != ConsoleKey.Escape);

        Console.WriteLine("End");
    }

    private static void ServerProvider_SyncProgress(object sender, SyncProgressEventArgs e)
    {
        SyncProgress(e, ConsoleColor.Red);
    }

    private static void SyncProgress(object sender, SyncProgressEventArgs e)
    {
        SyncProgress(e);
    }

    private static void SyncProgress(SyncProgressEventArgs e, ConsoleColor? consoleColor = null)
    {
        var sessionId = e.Context.SessionId.ToString();

        if (consoleColor.HasValue)
            Console.ForegroundColor = consoleColor.Value;

        switch (e.Context.SyncStage)
        {
            case SyncStage.BeginSession:
                Console.WriteLine($"Begin Session.");
                break;
            case SyncStage.EndSession:
                Console.WriteLine($"End Session.");
                break;
            case SyncStage.EnsureMetadata:
                if (e.Configuration != null)
                {
                    var ds = e.Configuration.ScopeSet;

                    Console.WriteLine($"Configuration readed. {ds.Tables.Count} table(s) involved.");

                    Func<JsonSerializerSettings> settings = new Func<JsonSerializerSettings>(() =>
                    {
                        var s = new JsonSerializerSettings();
                        s.Formatting = Formatting.Indented;
                        s.StringEscapeHandling = StringEscapeHandling.Default;
                        return s;
                    });
                    JsonConvert.DefaultSettings = settings;
                    var dsString = JsonConvert.SerializeObject(new DmSetSurrogate(ds));

                    //Console.WriteLine(dsString);
                }
                if (e.DatabaseScript != null)
                {
                    Console.WriteLine($"Database is created");
                    //Console.WriteLine(e.DatabaseScript);
                }
                break;
            case SyncStage.SelectedChanges:
                Console.WriteLine($"Selected changes : {e.ChangesStatistics.TotalSelectedChanges}");

                //Console.WriteLine($"{sessionId}. Selected added Changes : {e.ChangesStatistics.TotalSelectedChangesInserts}");
                //Console.WriteLine($"{sessionId}. Selected updates Changes : {e.ChangesStatistics.TotalSelectedChangesUpdates}");
                //Console.WriteLine($"{sessionId}. Selected deleted Changes : {e.ChangesStatistics.TotalSelectedChangesDeletes}");
                break;

            case SyncStage.AppliedChanges:
                Console.WriteLine($"Applied changes : {e.ChangesStatistics.TotalAppliedChanges}");
                break;
            //case SyncStage.ApplyingInserts:
            //    Console.WriteLine($"{sessionId}. Applying Inserts : {e.ChangesStatistics.AppliedChanges.Where(ac => ac.State == DmRowState.Added).Sum(ac => ac.ChangesApplied) }");
            //    break;
            //case SyncStage.ApplyingDeletes:
            //    Console.WriteLine($"{sessionId}. Applying Deletes : {e.ChangesStatistics.AppliedChanges.Where(ac => ac.State == DmRowState.Deleted).Sum(ac => ac.ChangesApplied) }");
            //    break;
            //case SyncStage.ApplyingUpdates:
            //    Console.WriteLine($"{sessionId}. Applying Updates : {e.ChangesStatistics.AppliedChanges.Where(ac => ac.State == DmRowState.Modified).Sum(ac => ac.ChangesApplied) }");
            //    break;
            case SyncStage.WriteMetadata:
                if (e.Scopes != null)
                {
                    Console.WriteLine($"Writing Scopes : ");
                    e.Scopes.ForEach(sc => Console.WriteLine($"\t{sc.Id} synced at {sc.LastSync}. "));
                }
                break;
            case SyncStage.CleanupMetadata:
                Console.WriteLine($"CleanupMetadata");
                break;
        }

        Console.ResetColor();
    }

    static void ApplyChangedFailed(object sender, ApplyChangeFailedEventArgs e)
    {
        // tables name
        string serverTableName = e.Conflict.RemoteChanges.TableName;
        string clientTableName = e.Conflict.LocalChanges.TableName;

        // server row in conflict
        var dmRowServer = e.Conflict.RemoteChanges.Rows[0];
        var dmRowClient = e.Conflict.LocalChanges.Rows[0];

        // Example 1 : Resolution based on rows values
        if ((int)dmRowServer["ClientID"] == 100 && (int)dmRowClient["ClientId"] == 0)
            e.Action = ApplyAction.Continue;
        else
            e.Action = ApplyAction.RetryWithForceWrite;

        // Example 2 : resolution based on conflict type
        // Line exist on client, not on server, force to create it
        //if (e.Conflict.Type == ConflictType.RemoteInsertLocalNoRow || e.Conflict.Type == ConflictType.RemoteUpdateLocalNoRow)
        //    e.Action = ApplyAction.RetryWithForceWrite;
        //else
        //    e.Action = ApplyAction.RetryWithForceWrite;
    }
}