using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class DepartmentService
{
    private readonly DepartmentRepository _repository;

    public DepartmentService(DepartmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> AddDepartmentAsync(Department department)
    {
        if (string.IsNullOrWhiteSpace(department.DepartmentName))
            throw new ArgumentException("Department name is required.");

        return await _repository.AddAsync(department);
    }

    public Task<List<Department>> GetAllDepartmentsAsync(bool activeOnly = false)
    {
        return _repository.GetAllAsync(activeOnly);
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Department ID must be greater than zero.");

        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateDepartmentAsync(Department department)
    {
        if (department.DepartmentId <= 0)
            throw new ArgumentException("Invalid department ID.");

        if (string.IsNullOrWhiteSpace(department.DepartmentName))
            throw new ArgumentException("Department name is required.");

        return await _repository.UpdateAsync(department);
    }

    public async Task<bool> DeactivateDepartmentAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid department ID.");

        return await _repository.DeactivateAsync(id);
    }
}
