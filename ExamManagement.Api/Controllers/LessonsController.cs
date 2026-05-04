using ExamManagement.Api.DTOs;
using ExamManagement.Api.Helpers;
using ExamManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/lessons")]
public sealed class LessonsController(ILessonService lessonService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LessonDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await lessonService.GetAllAsync(User.GetTeacherId(), cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LessonDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await lessonService.GetByIdAsync(User.GetTeacherId(), id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<LessonDto>> Create(LessonCreateDto request, CancellationToken cancellationToken)
    {
        var lesson = await lessonService.CreateAsync(User.GetTeacherId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = lesson.Id }, lesson);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LessonDto>> Update(int id, LessonUpdateDto request, CancellationToken cancellationToken)
    {
        return Ok(await lessonService.UpdateAsync(User.GetTeacherId(), id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await lessonService.DeleteAsync(User.GetTeacherId(), id, cancellationToken);
        return NoContent();
    }
}
