using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class StudentService
{
    private readonly StudentRepository _repository;
    private readonly DepartmentRepository _departmentRepository;

    public StudentService(StudentRepository repository, DepartmentRepository departmentRepository)
    {
        _repository = repository;
        _departmentRepository = departmentRepository;
    }

    public async Task<int> RegisterAsync(Student student)
    {
        if (string.IsNullOrWhiteSpace(student.Name))
            throw new ArgumentException("Student name is required.");

        if (string.IsNullOrWhiteSpace(student.Email))
            throw new ArgumentException("Student email is required.");

        if (student.Age is < 17 or > 60)
            throw new ArgumentOutOfRangeException(
                nameof(student.Age), "Age must be between 17 and 60.");

        if (student.DepartmentId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(student.DepartmentId), "Select a valid department.");

        var dept = await _departmentRepository.GetByIdAsync(student.DepartmentId);
        if (dept is null || !dept.IsActive)
            throw new InvalidOperationException("Department does not exist or is inactive.");

        return await _repository.AddAsync(student);
    }

    public Task<List<Student>> GetAllStudentsAsync(bool activeOnly = false)
    {
        return _repository.GetAllAsync(activeOnly);
    }

    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Student ID must be greater than zero.");

        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<Student>> SearchStudentsByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return await _repository.GetAllAsync();

        return await _repository.SearchByNameAsync(name);
    }

    public async Task<List<Student>> GetStudentsByDepartmentAsync(int departmentId)
    {
        if (departmentId <= 0)
            throw new ArgumentException("Department ID must be greater than zero.");

        return await _repository.GetByDepartmentAsync(departmentId);
    }

    public async Task<bool> UpdateStudentAsync(Student student)
    {
        if (student.StudentId <= 0)
            throw new ArgumentException("Invalid student ID.");

        if (string.IsNullOrWhiteSpace(student.Name))
            throw new ArgumentException("Student name is required.");

        if (string.IsNullOrWhiteSpace(student.Email))
            throw new ArgumentException("Student email is required.");

        if (student.Age is < 17 or > 60)
            throw new ArgumentOutOfRangeException(nameof(student.Age), "Age must be between 17 and 60.");

        var dept = await _departmentRepository.GetByIdAsync(student.DepartmentId);
        if (dept is null || !dept.IsActive)
            throw new InvalidOperationException("Department does not exist or is inactive.");

        return await _repository.UpdateAsync(student);
    }

    public async Task<bool> DeactivateStudentAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid student ID.");

        return await _repository.DeactivateAsync(id);
    }
}
