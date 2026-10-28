using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;
using System.Data;

namespace StudentCourseManagement.Repositories;

public class CourseRepository
{
    public async Task<int> AddAsync(Course course)
    {
        const string sql = """
            INSERT INTO Courses (CourseName, DurationMonths, Fee, DepartmentId, IsActive)
            VALUES (@CourseName, @DurationMonths, @Fee, @DepartmentId, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@CourseName", SqlDbType.NVarChar, 100).Value = course.CourseName;
        command.Parameters.Add("@DurationMonths", SqlDbType.Int).Value = course.DurationMonths;
        command.Parameters.Add("@Fee", SqlDbType.Decimal).Value = course.Fee;
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = course.DepartmentId;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = course.IsActive;

        await connection.OpenAsync();
        object? result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<List<Course>> GetAllAsync(bool activeOnly = false)
    {
        string sql = """
            SELECT CourseId, CourseName, DurationMonths, Fee, DepartmentId, IsActive
            FROM Courses
            """ + (activeOnly ? " WHERE IsActive = 1" : "") + " ORDER BY CourseId;";

        var list = new List<Course>();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(MapCourse(reader));
        }

        return list;
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT CourseId, CourseName, DurationMonths, Fee, DepartmentId, IsActive
            FROM Courses
            WHERE CourseId = @CourseId;
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@CourseId", SqlDbType.Int).Value = id;

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapCourse(reader);
        }

        return null;
    }

    public async Task<List<Course>> GetByDepartmentAsync(int departmentId)
    {
        const string sql = """
            SELECT CourseId, CourseName, DurationMonths, Fee, DepartmentId, IsActive
            FROM Courses
            WHERE DepartmentId = @DepartmentId
            ORDER BY CourseName;
            """;

        var list = new List<Course>();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = departmentId;

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(MapCourse(reader));
        }

        return list;
    }

    public async Task<bool> UpdateAsync(Course course)
    {
        const string sql = """
            UPDATE Courses
            SET CourseName = @CourseName,
                DurationMonths = @DurationMonths,
                Fee = @Fee,
                DepartmentId = @DepartmentId,
                IsActive = @IsActive
            WHERE CourseId = @CourseId;
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@CourseId", SqlDbType.Int).Value = course.CourseId;
        command.Parameters.Add("@CourseName", SqlDbType.NVarChar, 100).Value = course.CourseName;
        command.Parameters.Add("@DurationMonths", SqlDbType.Int).Value = course.DurationMonths;
        command.Parameters.Add("@Fee", SqlDbType.Decimal).Value = course.Fee;
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = course.DepartmentId;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = course.IsActive;

        await connection.OpenAsync();
        int rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        const string sql = """
            UPDATE Courses
            SET IsActive = 0
            WHERE CourseId = @CourseId;
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@CourseId", SqlDbType.Int).Value = id;

        await connection.OpenAsync();
        int rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    private static Course MapCourse(SqlDataReader reader)
    {
        return new Course
        {
            CourseId = reader.GetInt32(0),
            CourseName = reader.GetString(1),
            DurationMonths = reader.GetInt32(2),
            Fee = reader.GetDecimal(3),
            DepartmentId = reader.GetInt32(4),
            IsActive = reader.GetBoolean(5)
        };
    }
}
