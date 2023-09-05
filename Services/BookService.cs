using elastic_search_app.Entities;
using Microsoft.EntityFrameworkCore;

namespace elastic_search_app.Services
{
    public class BookService
    {
        private readonly ILogger<BookService> _logger;
        private readonly AppDbContext _context;
        private readonly DataGeneratorService _generator;
        private readonly SyncService _syncService;

        public BookService(ILogger<BookService> logger,
            AppDbContext context,
            DataGeneratorService generator,
            SyncService syncService)
        {
            _logger=logger;
            _context=context;
            _generator=generator;
            _syncService=syncService;
        }

        public async Task<Book?> GetAsync(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == id);

            if (book==null)
            {
                _logger.LogInformation($"Book(id={id}) not found in DB");
                return null;
            }

            _logger.LogInformation($"Book(id={id}) successfully found in DB");
            return book;
        }

        public async Task GenerateAsync(int amount)
        {
            var generatedList = _generator.GenerateBooks(amount);

            if (generatedList==null)
            {
                _logger.LogInformation($"Generated list is not valid");
                throw new ArgumentNullException("Generated list is not valid");
            }

            await _context.Books.AddRangeAsync(generatedList);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created books with IDs {generatedList.First().Id}-{generatedList.Last().Id}");
        }

        public async Task<int?> CreateAsync(Book book)
        {
            if (book==null)
            {
                _logger.LogInformation($"Book for creation is not valid");
                throw new ArgumentNullException("Book for creation is not valid");
            }

            book.LastModified = DateTime.Now;
            book.IfSynced = false;
            book.LastOperation = Operation.Create;

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created book with ID {book.Id}");

            return book.Id;
        }

        public async Task UpdateAsync(int id, Book book)
        {
            var data = await _context.Books.FirstOrDefaultAsync(x => x.Id == id);

            if (data != null)
            {
                data.Title = book.Title;
                data.Author = book.Author;
                data.Annotation = book.Annotation;
                data.Country = book.Country;
                data.Genre = book.Genre;
                data.PublishYear = book.PublishYear;

                data.LastModified = DateTime.Now;
                data.IfSynced = false;
                data.LastOperation = Operation.Update;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully updated book with id={id}");
            }
            else
            {
                _logger.LogWarning($"Could not find book with id={id} in update method");
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == id);

            if (book==null)
            {
                _logger.LogWarning($"Book(id={id}) not found in DB");
                return false;
            }

            SyncService.AddToElasticDeleteQueue(book);
            _context.Books.Remove(book);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
