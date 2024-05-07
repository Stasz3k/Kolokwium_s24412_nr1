namespace Kolokwium_s24412_nr1.Models.DTOs
{
    public class BookCreateDto
    {
        public string Title { get; set; }
        public List<AuthorDto> Authors { get; set; }
    }

}
