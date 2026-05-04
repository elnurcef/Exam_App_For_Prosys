using ExamManagement.Api.DTOs;
using ExamManagement.Api.Helpers;
using ExamManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/exams")]
public sealed class ExamsController(IExamService examService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExamDto>>> GetAll([FromQuery] ExamFilterDto filter, CancellationToken cancellationToken)
    {
        return Ok(await examService.GetAllAsync(User.GetTeacherId(), filter, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExamDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await examService.GetByIdAsync(User.GetTeacherId(), id, cancellationToken));
    }

    [HttpPost("schedule")]
    public async Task<ActionResult<ExamDto>> Schedule(ExamScheduleDto request, CancellationToken cancellationToken)
    {
        var exam = await examService.ScheduleAsync(User.GetTeacherId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = exam.Id }, exam);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExamDto>> Update(int id, ExamUpdateDto request, CancellationToken cancellationToken)
    {
        return Ok(await examService.UpdateAsync(User.GetTeacherId(), id, request, cancellationToken));
    }

    [HttpPut("{id:int}/grade")]
    public async Task<ActionResult<ExamDto>> Grade(int id, ExamGradeDto request, CancellationToken cancellationToken)
    {
        return Ok(await examService.GradeAsync(User.GetTeacherId(), id, request, cancellationToken));
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<ActionResult<ExamDto>> Cancel(int id, CancellationToken cancellationToken)
    {
        return Ok(await examService.CancelAsync(User.GetTeacherId(), id, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await examService.DeleteAsync(User.GetTeacherId(), id, cancellationToken);
        return NoContent();
    }
}
