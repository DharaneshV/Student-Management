using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;

namespace StudentCourseManagement.Repositories;

public class ReportRepository
{
    // 1. Students with Department Names
    public async Task<List<(int StudentId, string Name, string Email, string DepartmentName)>> GetStudentsWithDepartmentsAsync()
    {
        const string sql = """
            SELECT s.StudentId, s.Name, s.Email, d.DepartmentName
            FROM Students AS s
            INNER JOIN Departments AS d ON d.DepartmentId = s.DepartmentId
            ORDER BY s.Name;
            """;

        var list = new List<(int, string, string, string)>();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add((reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
        }

        return list;
    }

    // 2. Student Count by Department
    public async Task<List<(string DepartmentName, int StudentCount)>> GetStudentCountByDepartmentAsync()
    {
        const string sql = """
            SELECT d.DepartmentName, COUNT(s.StudentId) AS StudentCount
            FROM Departments AS d
            LEFT JOIN Students AS s ON d.DepartmentId = s.DepartmentId
            GROUP BY d.DepartmentName
            ORDER BY StudentCount DESC;
            """;

        var list = new List<(string, int)>();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add((reader.GetString(0), reader.GetInt32(1)));
        }

        return list;
    }

    // 3. Enrollment Count per Course
    public async Task<List<(int CourseId, string CourseName, int EnrollmentCount)>> GetEnrollmentCountPerCourseAsync()
    {
        const string sql = """
            SELECT c.CourseId, c.CourseName, COUNT(e.EnrollmentId) AS EnrollmentCount
            FROM Courses AS c
            LEFT JOIN Enrollments AS e ON e.CourseId = c.CourseId
            GROUP BY c.CourseId, c.CourseName
            ORDER BY EnrollmentCount DESC;
            """;

        var list = new List<(int, string, int)>();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add((reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2)));
        }

        return list;
    }

    // 4. Students Without Enrollments (NOT EXISTS)
    public async Task<List<(int StudentId, string Name)>> GetStudentsWithoutEnrollmentsAsync()
    {
        const string sql = """
            SELECT s.StudentId, s.Name
            FROM Students AS s
            WHERE NOT EXISTS
            (
                SELECT 1 FROM Enrollments AS e
                WHERE e.StudentId = s.StudentId
            );
            """;

        var list = new List<(int, string)>();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add((reader.GetInt32(0), reader.GetString(1)));
        }

        return list;
    }

    // 5. Courses Without Enrollments (LEFT JOIN / IS NULL)
    public async Task<List<(int CourseId, string CourseName)>> GetCoursesWithoutEnrollmentsAsync()
    {
        const string sql = """
            SELECT c.CourseId, c.CourseName
            FROM Courses AS c
            LEFT JOIN Enrollments AS e ON e.CourseId = c.CourseId
            WHERE e.EnrollmentId IS NULL;
            """;

        var list = new List<(int, string)>();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add((reader.GetInt32(0), reader.GetString(1)));
        }

        return list;
    }
}
