using Kolokwium_s24412_nr1.Models.DTOs;

namespace Kolokwium_s24412_nr1.Services.Interface
{
    public interface IBookService
    {
        Task<bool>DoesBookExist(int id);

        Task<bool> DoesAuthorExist(string firstName, string lastName);

        Task<int> AddBookWithAuthors(BookCreateDto bookCreateDto);

        Task<BookResponseDto> GetBookWithAuthorsAsync(int bookId);

    }
}
