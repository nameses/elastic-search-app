using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using elastic_search_app.DTO;
using elastic_search_app.Entities;
using elastic_search_app.Settings.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace elastic_search_app.Services
{
    public class SyncService
    {
        private readonly IOptions<ElasticConfig> _config;
        private readonly AppDbContext _context;
        private readonly ILogger<SyncService> _logger;
        //default url = localhost:9200
        private readonly ElasticsearchClient client;
        private readonly string bookIndexName = "book_index";

        public static List<int?> bookIdsToDelete = new();

        public SyncService(IOptions<ElasticConfig> config, AppDbContext context, ILogger<SyncService> logger)
        {
            _config=config;
            _context=context;
            _logger=logger;

            var settings = new ElasticsearchClientSettings(_config.Value.CloudId!,
                new BasicAuthentication(_config.Value.User!, _config.Value.Password!));

            client = new ElasticsearchClient(settings);
        }

        public static void AddToElasticDeleteQueue(Book book)
        {
            if (book == null) throw new NullReferenceException();

            //book.LastModified = DateTime.Now;
            //book.IfSynced = false;
            //book.LastOperation = Operation.Delete;

            bookIdsToDelete.Add(book.Id);
        }

        public async Task SyncData()
        {
            var unsyncedRows = await _context.Books.Where(row => row.IfSynced == false)
                .OrderByDescending(row => row.LastModified)
                .ToListAsync();

            foreach (var row in unsyncedRows)
            {
                var document = new BookDTO()
                {
                    Id = row.Id,
                    Title = row.Title,
                    Author = row.Author,
                    Annotation = row.Annotation,
                    Country = row.Country,
                    Genre = row.Genre,
                    PublishYear = row.PublishYear
                };

                //logic for create
                if (row.LastOperation==Operation.Create)
                {
                    var indexResponse = await client.IndexAsync(document, bookIndexName);

                    if (indexResponse.IsValidResponse)
                    {
                        row.IfSynced = true;
                    }
                    else
                    {
                        _logger.LogWarning($"Elastic: Failed to add document to index: {indexResponse.DebugInformation}");
                    }
                }
                else if (row.LastOperation==Operation.Update)
                {
                    var scriptSource = "ctx._source = params.updatedObject";
                    var scriptParams = new Dictionary<string, object>
                    {
                        { "updatedObject", document }
                    };

                    var inlineScript = new InlineScript(scriptSource)
                    {
                        Params=scriptParams
                    };
                    var script = new Script(inlineScript);

                    var updateDescriptor = new UpdateByQueryRequestDescriptor<BookDTO>(bookIndexName)
                        .Script(script)
                        .Query(q => q
                            .Match(m => m
                                .Field(f => f.Id)
                                .Query(document.Id.ToString())
                            )
                        );

                    var response = await client.UpdateByQueryAsync<BookDTO>(updateDescriptor);

                    if (response.IsValidResponse)
                    {
                        row.IfSynced = true;
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Elastic: Updated document with Id {document.Id}");
                    }
                    else
                    {
                        // Handle errors
                        _logger.LogInformation($"Elastic: Error updating document with Id {document.Id}: {response.ElasticsearchServerError}");
                    }
                }
                //logic for update


            }

            await this.SyncDeletedBookAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Elastic: Successfully synced all created/updated data");
        }
        public async Task SyncDeletedBookAsync()
        {
            var isValidResponse = true;
            foreach (var id in bookIdsToDelete)
            {
                var result = await client.DeleteByQueryAsync<BookDTO>(bookIndexName,
                    d => d.Query(q => q
                            .Match(m => m
                                .Field(p => p.Id)
                                .Query(id.ToString()!)
                            )
                        )
                );

                if (!result.IsValidResponse)
                {
                    isValidResponse=false;
                }
            }
            if (isValidResponse)
            {
                bookIdsToDelete.Clear();
                _logger.LogInformation("Elastic: Successfully synced all deleted data");
            }
            else
                _logger.LogInformation("Elastic: Exception during syncing deleted data");
        }
    }
}
