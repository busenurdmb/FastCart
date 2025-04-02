using FastCart.Application.Interfaces;
using Nest;


namespace FastCart.Infrastructure.Pipeline.Elasticsearch;

public class ElasticService : IElasticService
{
    private readonly ElasticClient _client;

    public ElasticService()
    {
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("fastcart-logs-*");

        _client = new ElasticClient(settings);
    }

    public async Task<List<object>> SearchLogsByProductNameAsync(string productName)
    {
        var response = await _client.SearchAsync<object>(s => s
            .Query(q => q
                .Match(m => m
                    .Field("ProductName")
                    .Query(productName)
                )
            )
        );

        return response.Documents.ToList();
    }
}
