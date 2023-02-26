using Carter;


namespace Records
{
    internal record class ShortUrl(string Url)
    {
        public Guid Id { get; set; }

        public string? Chunck { get; set; }
    }
}
