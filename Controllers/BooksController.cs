using Kolokwium_s24412_nr1.Models.DTOs;
using Kolokwium_s24412_nr1.Services.Interface;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpPost]
    public async Task<ActionResult<BookResponseDto>> PostBook([FromBody] BookCreateDto bookCreateDto)
    {
        try
        {
            int bookId = await _bookService.AddBookWithAuthors(bookCreateDto);
            if (bookId == 0)
            {
                return BadRequest("Could not add the book and authors.");
            }

            var book = await _bookService.GetBookWithAuthorsAsync(bookId);
            return CreatedAtAction(nameof(GetBookAuthors), new { id = book.Id }, book); //zwraca kod 201
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{id}/authors")]
    public async Task<ActionResult<BookResponseDto>> GetBookAuthors(int id)
    {
        var bookAuthorsDto = await _bookService.GetBookWithAuthorsAsync(id);
        if (bookAuthorsDto == null)
        {
            return NotFound($"No book found with ID {id}");
        }

        return Ok(bookAuthorsDto);
    }



}


