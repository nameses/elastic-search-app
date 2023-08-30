using elastic_search_app.Entities;
using Microsoft.EntityFrameworkCore;

namespace elastic_search_app.Services
{
    public class BookService
    {
        private readonly ILogger<BookService> _logger;
        private readonly AppDbContext _context;

        public BookService(ILogger<BookService> logger, AppDbContext context)
        {
            _logger=logger;
            _context=context;
        }
        public async Task<Book?> GetByIdAsync(int id)
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

        public async Task<int?> CreateAsync(Book book)
        {
            if (book==null)
            {
                _logger.LogInformation($"Book for creation is not valid");
                throw new ArgumentNullException("Book for creation is not valid");
            }

            book.Created = DateTime.Now;

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created book with ID {book.Id}");

            return book.Id;
        }

        //public async Task UpdateAsync(int id, Book book) { }

        public async Task DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book==null)
            {
                _logger.LogInformation($"Book(id={id}) not found in DB");
                return;
            }

            _context.Books.Remove(book);
        }
    }
}
