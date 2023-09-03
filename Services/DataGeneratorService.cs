using Bogus;
using elastic_search_app.Entities;

namespace elastic_search_app.Services
{
    public class DataGeneratorService
    {
        List<string> genreList = new()
        {
            "Mystery", "Thriller", "Crime", "Detective", "Horror",
            "Fantasy", "Science Fiction (Sci-Fi)", "Dystopian", "Adventure", "Romance",
            "Historical Fiction", "Young Adult", "Childrens", "Comedy", "Drama",
            "Poetry", "Science", "Self-Help", "Biography", "Autobiography",
            "Memoir", "Travel", "Non-Fiction", "Philosophy", "Religion",
            "Psychology", "Sociology", "Political Science", "Economics",
            "Art and Photography"
        };
        private readonly ILogger<DataGeneratorService> _logger;

        public DataGeneratorService(ILogger<DataGeneratorService> logger)
        {
            _logger=logger;
        }

        public List<Book> GenerateBooks(int amount = 1)
        {
            var generator = GetBookGenerator();
            var list = generator.Generate(amount);

            if (generator==null && list==null)
            {
                _logger.LogInformation("Exception in book generating");
                return null;
            }
            _logger.LogInformation($"Successful books(amount={amount}) generating");
            return list;
        }

        private Faker<Book> GetBookGenerator()
        {
            return new Faker<Book>()
                .RuleFor(e => e.Title, f => f.Lorem.Word())
                .RuleFor(e => e.Author, f => f.Name.FullName())
                .RuleFor(e => e.Annotation, f => f.Lorem.Sentence())
                .RuleFor(e => e.Country, f => f.Address.Country())
                .RuleFor(e => e.Genre, f => f.PickRandom(genreList))
                .RuleFor(e => e.PublishYear, f => f.Random.Int(1900, DateTime.Now.Year))
                .RuleFor(e => e.LastModified, _ => DateTime.Now)
                .RuleFor(e => e.IfSynced, _ => false)
                .RuleFor(e => e.LastOperation, _ => Operation.Create);
        }
    }
}
