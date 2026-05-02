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

        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM GradeComponent WHERE SubjectId = {0}",
            id.ToString());

        existing.Components.Clear();

        foreach (ComponentDto dto in request.Components)
        {
            GradeComponent component = new GradeComponent(
                dto.Name,
                dto.Weight,
                dto.Complexity,
                dto.CurrentGrade,
                dto.IsGraded
            )
            {
                IsBlocking = dto.IsBlocking,
                MinimumGrade = dto.MinimumGrade,
                IsAutoGrade = dto.IsAutoGrade,
                AutoGradeMinScore = dto.AutoGradeMinScore,
                SubjectId = id
            };

            existing.Components.Add(component);
            _context.Entry(component).State = EntityState.Added;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Subject subject)
    {
        _context.Subjects.Remove(subject);
        await _context.SaveChangesAsync();
    }
}