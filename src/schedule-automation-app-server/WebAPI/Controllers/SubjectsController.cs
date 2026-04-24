using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using schedule_automation_app_server.Application.DTOs;
using schedule_automation_app_server.Application.Mappers;
using schedule_automation_app_server.Application.Services.Interfaces;
using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectRepository _repository;
    private readonly IGradeCalculationService _calculationService;
    private readonly IValidator<CreateSubjectRequest> _createValidator;
    private readonly IValidator<UpdateSubjectRequest> _updateValidator;

    public SubjectsController(
        ISubjectRepository repository,
        IGradeCalculationService calculationService,
        IValidator<CreateSubjectRequest> createValidator,
        IValidator<UpdateSubjectRequest> updateValidator)
    {
        _repository = repository;
        _calculationService = calculationService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        List<Subject> subjects = await _repository.GetAllAsync();

        List<SubjectListItemResponse> response = subjects
            .Select(s => SubjectMapper.ToListItemResponse(s, _calculationService.CalculateCurrentGrade(s)))
            .ToList();

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new { error = "Неверный ID предмета." });
        }

        Subject? subject = await _repository.GetByIdAsync(id);

        if (subject == null)
        {
            return NotFound(new { error = $"Предмет с ID {id} не найден." });
        }

        OptimizationPlan plan = _calculationService.CalculateOptimizationPlan(subject);
        return Ok(SubjectMapper.ToResponse(subject, plan));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubjectRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Тело запроса не может быть пустым." });
        }

        ValidationResult validation = await _createValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            List<string> errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { errors });
        }

        try
        {
            Subject subject = SubjectMapper.ToDomain(request);
            await _repository.AddAsync(subject);

            OptimizationPlan plan = _calculationService.CalculateOptimizationPlan(subject);
            return Ok(SubjectMapper.ToResponse(subject, plan));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubjectRequest request)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new { error = "Неверный ID предмета." });
        }

        if (request == null)
        {
            return BadRequest(new { error = "Тело запроса не может быть пустым." });
        }

        ValidationResult validation = await _updateValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            List<string> errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { errors });
        }

        Subject? subject = await _repository.GetByIdAsync(id);

        if (subject == null)
        {
            return NotFound(new { error = $"Предмет с ID {id} не найден." });
        }

        try
        {
            await _repository.UpdateAsync(id, request);

            Subject? updated = await _repository.GetByIdAsync(id);
            OptimizationPlan plan = _calculationService.CalculateOptimizationPlan(updated!);
            return Ok(SubjectMapper.ToResponse(updated!, plan));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/optimization-plan")]
    public async Task<IActionResult> GetOptimizationPlan(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new { error = "Неверный ID предмета." });
        }

        Subject? subject = await _repository.GetByIdAsync(id);

        if (subject == null)
        {
            return NotFound(new { error = $"Предмет с ID {id} не найден." });
        }

        OptimizationPlan plan = _calculationService.CalculateOptimizationPlan(subject);

        return Ok(new
        {
            SubjectId = subject.Id,
            SubjectName = subject.Name,
            TargetGrade = subject.TargetGrade,
            CurrentGrade = Math.Round(plan.CurrentGrade, 2),
            NecessaryPoints = Math.Round(plan.NecessaryPoints, 2),
            IsAchievable = plan.IsAchievable,
            Recommendation = plan.Recommendation,
            Plan = plan.Items.Select(i => new
            {
                i.ComponentName,
                CurrentGrade = Math.Round(i.CurrentGrade, 2),
                i.RequiredGrade,
                i.Priority,
                i.Reason
            })
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new { error = "Неверный ID предмета." });
        }

        Subject? subject = await _repository.GetByIdAsync(id);

        if (subject == null)
        {
            return NotFound(new { error = $"Предмет с ID {id} не найден." });
        }

        await _repository.DeleteAsync(subject);
        return NoContent();
    }
}