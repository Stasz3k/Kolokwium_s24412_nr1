namespace Kolokwium_s24412_nr1.Models.DTOs
{
    public class BookResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<AuthorDto> Authors { get; set; }
    }
}
