namespace elastic_search_app.DTO
{
    public class BookDTO
    {
        public int? Id { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Annotation { get; set; }
        public string? Country { get; set; }
        public string? Genre { get; set; }
        public int? PublishYear { get; set; }
    }
}
