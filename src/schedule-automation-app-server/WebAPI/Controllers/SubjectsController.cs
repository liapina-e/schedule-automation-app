using Microsoft.AspNetCore.Mvc;
using schedule_automation_app_server.Application.DTOs;
using schedule_automation_app_server.Application.Services.Interfaces;
using schedule_automation_app_server.Domain.Entities;
using schedule_automation_app_server.Domain.ValueObjects;

namespace schedule_automation_app_server.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectRepository _repository;
    private readonly IGradeCalculationService _calculationService;

    public SubjectsController(ISubjectRepository repository, IGradeCalculationService calculationService)
    {
        _repository = repository;
        _calculationService = calculationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        List<Subject> subjects = await _repository.GetAllAsync();

        var response = subjects.Select(s => new
        {
            s.Id,
            s.Name,
            s.TargetGrade,
            CurrentGrade = Math.Round(_calculationService.CalculateCurrentGrade(s), 2)
        });

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubjectRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Тело запроса не может быть пустым." });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Название предмета обязательно." });
        }

        if (request.Components == null || request.Components.Count == 0)
        {
            return BadRequest(new { error = "Нужен хотя бы один компонент формулы." });
        }

        double totalWeight = request.Components.Sum(c => c.Weight);
        if (Math.Abs(totalWeight - 100) > 0.01)
        {
            return BadRequest(new { error = $"Сумма весов должна быть 100%. Сейчас: {totalWeight:F1}%." });
        }

        try
        {
            Subject subject = new Subject(request.Name, request.TargetGrade);

            foreach (ComponentDto dto in request.Components)
            {
                GradeComponent component = new GradeComponent(
                    dto.Name,
                    new Weight(dto.Weight),
                    new Complexity(dto.Complexity),
                    new Grade(dto.CurrentGrade)
                )
                {
                    IsBlocking = dto.IsBlocking,
                    MinimumGrade = dto.MinimumGrade
                };

                subject.AddComponent(component);
            }

            await _repository.AddAsync(subject);

            OptimizationPlan plan = _calculationService.CalculateOptimizationPlan(subject);

            SubjectResponse response = new SubjectResponse
            {
                Id = subject.Id,
                Name = subject.Name,
                CurrentGrade = Math.Round(plan.CurrentGrade, 2),
                TargetGrade = subject.TargetGrade,
                IsAchievable = plan.IsAchievable,
                Recommendation = plan.Recommendation,
                OptimalPlan = plan.Items.Select(i => new OptimizationItemDto
                {
                    ComponentName = i.ComponentName,
                    CurrentGrade = Math.Round(i.CurrentGrade, 2),
                    RequiredGrade = i.RequiredGrade,
                    Priority = i.Priority,
                    Reason = i.Reason
                }).ToList()
            };

            return Ok(response);
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

        try
        {
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
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при расчёте плана.", details = ex.Message });
        }
    }
}
