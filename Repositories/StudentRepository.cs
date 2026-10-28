using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;
using System.Data;

namespace StudentCourseManagement.Repositories;

public class StudentRepository
{
    public async Task<int> AddAsync(Student student)
    {
        const string sql = """
            INSERT INTO Students (Name, Email, Age, DepartmentId, IsActive)
            VALUES (@Name, @Email, @Age, @DepartmentId, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = student.Name;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = student.Email;
        command.Parameters.Add("@Age", SqlDbType.Int).Value = student.Age;
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = student.DepartmentId;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = student.IsActive;

        await connection.OpenAsync();
        object? result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<List<Student>> GetAllAsync(bool activeOnly = false)
    {
        string sql = """
            SELECT StudentId, Name, Email, Age, DepartmentId, AdmissionDate, IsActive
            FROM Students
            """ + (activeOnly ? " WHERE IsActive = 1" : "") + " ORDER BY StudentId;";

        var list = new List<Student>();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(MapStudent(reader));
        }

        return list;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT StudentId, Name, Email, Age, DepartmentId, AdmissionDate, IsActive
            FROM Students
            WHERE StudentId = @StudentId;
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@StudentId", SqlDbType.Int).Value = id;

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapStudent(reader);
        }

        return null;
    }

    public async Task<List<Student>> SearchByNameAsync(string name)
    {
        const string sql = """
            SELECT StudentId, Name, Email, Age, DepartmentId, AdmissionDate, IsActive
            FROM Students
            WHERE Name LIKE '%' + @Name + '%'
            ORDER BY Name;
            """;

        var list = new List<Student>();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(MapStudent(reader));
        }

        return list;
    }

    public async Task<List<Student>> GetByDepartmentAsync(int departmentId)
    {
        const string sql = """
            SELECT StudentId, Name, Email, Age, DepartmentId, AdmissionDate, IsActive
            FROM Students
            WHERE DepartmentId = @DepartmentId
            ORDER BY Name;
            """;

        var list = new List<Student>();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = departmentId;

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(MapStudent(reader));
        }

        return list;
    }

    public async Task<bool> UpdateAsync(Student student)
    {
        const string sql = """
            UPDATE Students
            SET Name = @Name,
                Email = @Email,
                Age = @Age,
                DepartmentId = @DepartmentId,
                IsActive = @IsActive
            WHERE StudentId = @StudentId;
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@StudentId", SqlDbType.Int).Value = student.StudentId;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = student.Name;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = student.Email;
        command.Parameters.Add("@Age", SqlDbType.Int).Value = student.Age;
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = student.DepartmentId;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = student.IsActive;

        await connection.OpenAsync();
        int rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        const string sql = """
            UPDATE Students
            SET IsActive = 0
            WHERE StudentId = @StudentId;
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@StudentId", SqlDbType.Int).Value = id;

        await connection.OpenAsync();
        int rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    private static Student MapStudent(SqlDataReader reader)
    {
        return new Student
        {
            StudentId = reader.GetInt32(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2),
            Age = reader.GetInt32(3),
            DepartmentId = reader.GetInt32(4),
            AdmissionDate = reader.GetDateTime(5),
            IsActive = reader.GetBoolean(6)
        };
    }
}
