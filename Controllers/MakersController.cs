using Microsoft.AspNetCore.Mvc;
using MakersApi.DTOs;
using MakersApi.Exceptions;
using MakersApi.Services;

namespace MakersApi.Controllers;

[ApiController]
[Route("api/makers")]
public class MakersController : ControllerBase
{
    private readonly IMakerService _makerService;

    public MakersController(IMakerService makerService)
    {
        _makerService = makerService;
    }


    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var maker = await _makerService.GetByIdAsync(id);
        if (maker is null)
            return NotFound();

        return Ok(maker);
    }







    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMakerRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _makerService.CreateAsync(request);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ConflictException ex)
        {
            return Conflict(ex.Message);
        }

        return Created("/api/makers", null);




    }
}
