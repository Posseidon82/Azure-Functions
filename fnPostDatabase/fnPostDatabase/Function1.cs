using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace fnPostDatabase;

public class Function1
{
    private readonly ILogger<Function1> _logger;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }

    [Function("movie")]
    [CosmosDBOutput(
        databaseName: "DatabaseName",
        containerName: "movies",
        Connection = "CosmoDBConnection", 
        CreateIfNotExists = true,
        PartitionKey = "id")]
    public async Task<Object?> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        MovieRequest movieRequest = null;

        var content = await new StreamReader(req.Body).ReadToEndAsync();

        try
        {
            movieRequest = JsonConvert.DeserializeObject<MovieRequest>(content);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult("Erro ao deserializar o objeto: " + ex.Message);
        }

        return JsonConvert.SerializeObject(movieRequest);
    }
}