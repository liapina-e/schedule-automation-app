using Microsoft.EntityFrameworkCore;
using schedule_automation_app_server.Application.DTOs;
using schedule_automation_app_server.Application.Services.Interfaces;
using schedule_automation_app_server.Domain.Entities;
using schedule_automation_app_server.Infrastructure.Data;

namespace schedule_automation_app_server.Infrastructure.Repositories;

public class SubjectRepository : ISubjectRepository
{
    private readonly AppDbContext _context;

    public SubjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Subject>> GetAllAsync()
    {
        return await _context.Subjects
            .Include(s => s.Components)
            .ToListAsync();
    }

    public async Task<Subject?> GetByIdAsync(Guid id)
    {
        return await _context.Subjects
            .Include(s => s.Components)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(Subject subject)
    {
        await _context.Subjects.AddAsync(subject);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guid id, UpdateSubjectRequest request)
    {
        Subject? existing = await _context.Subjects
            .Include(s => s.Components)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (existing == null)
        {
            return;
        }

        existing.Update(request.Name, request.TargetGrade);

        _context.Set<GradeComponent>().RemoveRange(existing.Components);

        List<GradeComponent> newComponents = request.Components.Select(dto => new GradeComponent(
            dto.Name,
            dto.Weight,
            dto.Complexity,
            dto.CurrentGrade
        )
        {
            IsBlocking = dto.IsBlocking,
            MinimumGrade = dto.MinimumGrade,
            SubjectId = id
        }).ToList();

        await _context.Set<GradeComponent>().AddRangeAsync(newComponents);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Subject subject)
    {
        _context.Subjects.Remove(subject);
        await _context.SaveChangesAsync();
    }
}