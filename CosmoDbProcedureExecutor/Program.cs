// See https://aka.ms/new-console-template for more information
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Newtonsoft.Json;

// check stored procedure code here
// https://github.com/Azure/azure-cosmosdb-js-server/blob/master/samples/stored-procedures/update.js

Console.WriteLine("Executed...");

var client = new CosmosClient(
    "https://localhost:8081",
    "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");

try
{
    // get all records
    Container container = client.GetContainer("pos-briefing", "Briefings");
    // Create query using a SQL string and parameters
    var query = new QueryDefinition(
        query: "SELECT * FROM root"
    );

    using FeedIterator<BriefingModel> feed = container.GetItemQueryIterator<BriefingModel>(
        queryDefinition: query
    );

    Console.WriteLine($"Has Results: {feed.HasMoreResults}");

    while (feed.HasMoreResults)
    {
        var responses = await feed.ReadNextAsync();
        Console.WriteLine($"fround {responses.Count} records");
        Console.WriteLine();

        foreach (var item in responses)
        {
            var infoText = $"BriefingId: {item.BriefingId}, Id: {item.Id}";

            try
            {
                var m = new RenameModel();
                m.rename = new Dictionary<string, string>();
                m.rename.Add("ApprovalDeadline", "ResponseDeadline");

                StoredProcedureExecuteResponse<object> response = await container.Scripts.ExecuteStoredProcedureAsync<object>(
                                "updateSproc", // your stored procedure name
                                new PartitionKey(item.BriefingId),
                                new dynamic[] { item.Id, m });

                var resource = response.Resource;
                Console.WriteLine($"Done: {infoText}");
            }
            catch (Exception exmsg)
            {
                Console.WriteLine();
                Console.WriteLine($"Error: {infoText}. \n msg: {exmsg.Message}");
                Console.WriteLine();
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

Console.WriteLine("Finished!");
Console.ReadKey();

public class RenameModel
{
    [JsonProperty("$rename")]
    public Dictionary<string, string> rename { get; set; }
}

public record BriefingModel
{
    public string BriefingId { get; set; }
    public string Id { get; set; }
}
