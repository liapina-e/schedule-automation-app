using schedule_automation_app_server.Application.DTOs;
using schedule_automation_app_server.Application.Services;
using schedule_automation_app_server.Domain.Entities;
using schedule_automation_app_server.Domain.ValueObjects;
using schedule_automation_app_server.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly SubjectRepository _repository;
    private readonly IGradeCalculationService _calculationService;

    public SubjectsController(SubjectRepository repository, IGradeCalculationService calculationService)
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
            CurrentGrade = _calculationService.CalculateCurrentGrade(s)
        });
        
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubjectRequest request)
    {
        try
        {
            Subject subject = new Subject(request.Name, request.TargetGrade);
        
            foreach (ComponentDto componentDto in request.Components)
            {
                GradeComponent component = new GradeComponent(
                    componentDto.Name,
                    new Weight(componentDto.Weight),
                    new Complexity(componentDto.Complexity),
                    new Grade(componentDto.CurrentGrade)
                )
                { 
                    IsBlocking = componentDto.IsBlocking,
                    MinimumGrade = componentDto.MinimumGrade
                };
            
                subject.Components.Add(component);
            }
        
            await _repository.AddAsync(subject);
        
            List<GradeComponent> optimalPlan = _calculationService.GetOptimalPlan(subject);
        
            SubjectResponse response = new SubjectResponse
            {
                Id = subject.Id,
                Name = subject.Name,
                CurrentGrade = _calculationService.CalculateCurrentGrade(subject),
                TargetGrade = subject.TargetGrade,
                OptimalPlan = optimalPlan.Select(c => new ComponentDto
                {
                    Name = c.Name,
                    Weight = c.Weight.Value,
                    Complexity = c.Complexity.Value,
                    CurrentGrade = c.CurrentGrade.Value,
                    IsBlocking = c.IsBlocking,
                    MinimumGrade = c.MinimumGrade
                }).ToList()
            };
        
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/plan")]
    public async Task<IActionResult> GetPlan(Guid id)
    {
        var subject = await _repository.GetByIdAsync(id);
        if (subject == null)
        {
            return NotFound();
        }
        
        List<GradeComponent> optimalPlan = _calculationService.GetOptimalPlan(subject);
        
        return Ok(new
        {
            subject.Name,
            CurrentGrade = _calculationService.CalculateCurrentGrade(subject),
            OptimalPlan = optimalPlan.Select(c => new
            {
                c.Name,
                Efficiency = c.GetEfficiency()
            })
        });
    }
}