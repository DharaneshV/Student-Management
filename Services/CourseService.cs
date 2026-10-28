using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class CourseService
{
    private readonly CourseRepository _repository;
    private readonly DepartmentRepository _departmentRepository;

    public CourseService(CourseRepository repository, DepartmentRepository departmentRepository)
    {
        _repository = repository;
        _departmentRepository = departmentRepository;
    }

    public async Task<int> AddCourseAsync(Course course)
    {
        if (string.IsNullOrWhiteSpace(course.CourseName))
            throw new ArgumentException("Course name is required.");

        if (course.DurationMonths <= 0)
            throw new ArgumentOutOfRangeException(nameof(course.DurationMonths), "Duration must be greater than zero.");

        if (course.Fee < 0)
            throw new ArgumentOutOfRangeException(nameof(course.Fee), "Fee cannot be negative.");

        if (course.DepartmentId <= 0)
            throw new ArgumentOutOfRangeException(nameof(course.DepartmentId), "Select a valid department.");

        var dept = await _departmentRepository.GetByIdAsync(course.DepartmentId);
        if (dept is null || !dept.IsActive)
            throw new InvalidOperationException("Department does not exist or is inactive.");

        return await _repository.AddAsync(course);
    }

    public Task<List<Course>> GetAllCoursesAsync(bool activeOnly = false)
    {
        return _repository.GetAllAsync(activeOnly);
    }

    public async Task<Course?> GetCourseByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Course ID must be greater than zero.");

        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<Course>> GetCoursesByDepartmentAsync(int departmentId)
    {
        if (departmentId <= 0)
            throw new ArgumentException("Department ID must be greater than zero.");

        return await _repository.GetByDepartmentAsync(departmentId);
    }

    public async Task<bool> UpdateCourseAsync(Course course)
    {
        if (course.CourseId <= 0)
            throw new ArgumentException("Invalid course ID.");

        if (string.IsNullOrWhiteSpace(course.CourseName))
            throw new ArgumentException("Course name is required.");

        if (course.DurationMonths <= 0)
            throw new ArgumentOutOfRangeException(nameof(course.DurationMonths), "Duration must be greater than zero.");

        if (course.Fee < 0)
            throw new ArgumentOutOfRangeException(nameof(course.Fee), "Fee cannot be negative.");

        var dept = await _departmentRepository.GetByIdAsync(course.DepartmentId);
        if (dept is null || !dept.IsActive)
            throw new InvalidOperationException("Department does not exist or is inactive.");

        return await _repository.UpdateAsync(course);
    }

    public async Task<bool> DeactivateCourseAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid course ID.");

        return await _repository.DeactivateAsync(id);
    }
}
