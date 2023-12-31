﻿namespace elastic_search_app.Entities
{
    public class Book
    {
        public int? Id { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Annotation { get; set; }
        public string? Country { get; set; }
        public string? Genre { get; set; }
        public int? PublishYear { get; set; }
        public DateTime? LastModified { get; set; }
        public bool IfSynced { get; set; }
        public Operation LastOperation { get; set; }
    }
    public enum Operation { Create, Read, Update, Delete }
}
