using schedule_automation_app_server.Domain.Entities;
using schedule_automation_app_server.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace schedule_automation_app_server.Infrastructure.Repositories;

public class SubjectRepository
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
}