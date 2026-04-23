using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Application.Services.Interfaces;

public interface ISubjectRepository
{
    Task<List<Subject>> GetAllAsync();
    Task<Subject?> GetByIdAsync(Guid id);
    Task AddAsync(Subject subject);
}
