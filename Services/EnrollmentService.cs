using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class EnrollmentService
{
    private readonly EnrollmentRepository _repository;
    private readonly StudentRepository _studentRepository;
    private readonly CourseRepository _courseRepository;

    public EnrollmentService(
        EnrollmentRepository repository,
        StudentRepository studentRepository,
        CourseRepository courseRepository)
    {
        _repository = repository;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
    }

    public async Task<int> EnrollStudentAsync(int studentId, int courseId)
    {
        if (studentId <= 0)
            throw new ArgumentOutOfRangeException(nameof(studentId), "Invalid student ID.");

        if (courseId <= 0)
            throw new ArgumentOutOfRangeException(nameof(courseId), "Invalid course ID.");

        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student is null || !student.IsActive)
            throw new InvalidOperationException("Student does not exist or is inactive.");

        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course is null || !course.IsActive)
            throw new InvalidOperationException("Course does not exist or is inactive.");

        return await _repository.EnrollWithAuditAsync(studentId, courseId);
    }

    public Task<List<Enrollment>> GetAllEnrollmentsAsync()
    {
        return _repository.GetAllAsync();
    }

    public async Task<bool> UpdateEnrollmentStatusAsync(int enrollmentId, string status)
    {
        if (enrollmentId <= 0)
            throw new ArgumentOutOfRangeException(nameof(enrollmentId), "Invalid enrollment ID.");

        string[] allowed = ["Enrolled", "Completed", "Cancelled"];
        if (!allowed.Contains(status, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException("Status must be Enrolled, Completed, or Cancelled.");

        return await _repository.UpdateStatusAsync(enrollmentId, status);
    }

    public Task<List<(int CourseId, string CourseName, string Status, DateTime EnrolledAt)>> GetCoursesForStudentAsync(int studentId)
    {
        if (studentId <= 0)
            throw new ArgumentOutOfRangeException(nameof(studentId), "Invalid student ID.");

        return _repository.GetCoursesByStudentAsync(studentId);
    }

    public Task<List<(int StudentId, string StudentName, string Status, DateTime EnrolledAt)>> GetStudentsForCourseAsync(int courseId)
    {
        if (courseId <= 0)
            throw new ArgumentOutOfRangeException(nameof(courseId), "Invalid course ID.");

        return _repository.GetStudentsByCourseAsync(courseId);
    }
}
