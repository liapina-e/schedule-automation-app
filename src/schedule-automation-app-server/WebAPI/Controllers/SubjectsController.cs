using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private const string StatsCacheKey = "stats";

    public SubjectsController(
        ISubjectRepository repository,
        IGradeCalculationService calculationService,
        IValidator<CreateSubjectRequest> createValidator,
        IValidator<UpdateSubjectRequest> updateValidator,
        IMemoryCache cache)
    {
        _repository = repository;
        _calculationService = calculationService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _cache = cache;
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

        string cacheKey = $"plan_{id}";

        if (_cache.TryGetValue(cacheKey, out SubjectResponse? cached))
        {
            return Ok(cached);
        }

        Subject? subject = await _repository.GetByIdAsync(id);

        if (subject == null)
        {
            return NotFound(new { error = $"Предмет с ID {id} не найден." });
        }

        OptimizationPlan plan = _calculationService.CalculateOptimizationPlan(subject);
        SubjectResponse response = SubjectMapper.ToResponse(subject, plan);

        _cache.Set(cacheKey, response, CacheDuration);

        return Ok(response);
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
            SubjectResponse response = SubjectMapper.ToResponse(subject, plan);

            _cache.Set($"plan_{subject.Id}", response, CacheDuration);
            _cache.Remove(StatsCacheKey);
            return Ok(response);
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

            _cache.Remove($"plan_{id}");

            Subject? updated = await _repository.GetByIdAsync(id);
            OptimizationPlan plan = _calculationService.CalculateOptimizationPlan(updated!);
            SubjectResponse response = SubjectMapper.ToResponse(updated!, plan);

            _cache.Set($"plan_{id}", response, CacheDuration);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/what-if")]
    public async Task<IActionResult> WhatIf(Guid id, [FromBody] List<WhatIfComponentDto> hypotheticalGrades)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new { error = "Неверный ID предмета." });
        }

        if (hypotheticalGrades == null || hypotheticalGrades.Count == 0)
        {
            return BadRequest(new { error = "Нужно передать хотя бы одну гипотетическую оценку." });
        }

        Subject? subject = await _repository.GetByIdAsync(id);

        if (subject == null)
        {
            return NotFound(new { error = $"Предмет с ID {id} не найден." });
        }

        WhatIfResponse result = _calculationService.CalculateWhatIf(subject, hypotheticalGrades);
        return Ok(result);
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

        _cache.Remove($"plan_{id}");
        _cache.Remove(StatsCacheKey);
        await _repository.DeleteAsync(subject);
        return NoContent();
    }
}