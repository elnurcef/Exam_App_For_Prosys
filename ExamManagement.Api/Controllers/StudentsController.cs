using ExamManagement.Api.DTOs;
using ExamManagement.Api.Helpers;
using ExamManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/students")]
public sealed class StudentsController(IStudentService studentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StudentDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await studentService.GetAllAsync(User.GetTeacherId(), cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await studentService.GetByIdAsync(User.GetTeacherId(), id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<StudentDto>> Create(StudentCreateDto request, CancellationToken cancellationToken)
    {
        var student = await studentService.CreateAsync(User.GetTeacherId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<StudentDto>> Update(int id, StudentUpdateDto request, CancellationToken cancellationToken)
    {
        return Ok(await studentService.UpdateAsync(User.GetTeacherId(), id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await studentService.DeleteAsync(User.GetTeacherId(), id, cancellationToken);
        return NoContent();
    }
}
