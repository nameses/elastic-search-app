using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using elastic_search_app.DTO;
using elastic_search_app.Entities;
using elastic_search_app.Settings.Configuration;
using Microsoft.Extensions.Options;

namespace elastic_search_app.Services
{
    public class SearchService
    {
        private readonly IOptions<ElasticConfig> _config;
        private readonly AppDbContext _context;
        private readonly ILogger<SearchService> _logger;
        //default url = localhost:9200
        private readonly ElasticsearchClient _client;
        private readonly string bookIndexName = "book_index";

        public SearchService(IOptions<ElasticConfig> config, AppDbContext context, ILogger<SearchService> logger)
        {
            _config=config;
            _context=context;
            _logger=logger;

            var settings = new ElasticsearchClientSettings(_config.Value.CloudId!,
                new BasicAuthentication(_config.Value.User!, _config.Value.Password!));

            _client = new ElasticsearchClient(settings);
        }


        public async Task<IEnumerable<BookDTO>> SearchByQueryAsync(string query)
        {
            var fuzzinessConst = 2;//possible: 0,1,2
            if (query.Count()<5) fuzzinessConst = 1;
            else if (query.Count()<2) fuzzinessConst = 0;
            var books = new List<BookDTO>();

            var response = await _client.SearchAsync<BookDTO>(s => s
                .Index(bookIndexName)
                .From(0)
                .Query(q => q
                    //.Match(m => m
                    //    .Field(f => f.Title)
                    //    .Query(query)
                    .Fuzzy(f => f
                        .Field(fld => fld.Title)
                        .Value(query)
                        .Fuzziness(new Fuzziness(fuzzinessConst)) // maximum edit distance
                    )
                )
                .Size(20)
            );

            if (response.IsSuccess())
                books.AddRange(response.Documents.ToList());

            if (books==null)
            {
                _logger.LogInformation($"Books(query={query}) not found in Elasticsearch");
                return null;
            }

            _logger.LogInformation($"Books(query={query}) successfully found in Elasticsearch");
            return books;
        }
    }
}
