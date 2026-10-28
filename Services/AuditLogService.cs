using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class AuditLogService
{
    private readonly AuditLogRepository _repository;

    public AuditLogService(AuditLogRepository repository)
    {
        _repository = repository;
    }

    public Task<List<AuditLog>> GetRecentLogsAsync(int count = 20)
    {
        return _repository.GetRecentLogsAsync(count);
    }
}
