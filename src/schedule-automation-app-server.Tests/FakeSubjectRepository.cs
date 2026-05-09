using schedule_automation_app_server.Application.DTOs;
using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Tests;

public class FakeSubjectRepository : Application.Services.Interfaces.ISubjectRepository
{
    private readonly List<Subject> _subjects;
    public FakeSubjectRepository(List<Subject> subjects) { _subjects = subjects; }
    public Task<List<Subject>> GetAllAsync() => Task.FromResult(_subjects);
    public Task<Subject?> GetByIdAsync(Guid id) => Task.FromResult(_subjects.FirstOrDefault(s => s.Id == id));
    public Task AddAsync(Subject subject) { _subjects.Add(subject); return Task.CompletedTask; }
    public Task DeleteAsync(Subject subject)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Guid id, UpdateSubjectRequest request)
    {
        throw new NotImplementedException();
    }
}