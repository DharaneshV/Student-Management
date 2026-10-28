using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;
using System.Data;

namespace StudentCourseManagement.Repositories;

public class DepartmentRepository
{
    public async Task<int> AddAsync(Department department)
    {
        const string sql = """
            INSERT INTO Departments (DepartmentName, Location, IsActive)
            VALUES (@DepartmentName, @Location, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@DepartmentName", SqlDbType.NVarChar, 100).Value = department.DepartmentName;
        command.Parameters.Add("@Location", SqlDbType.NVarChar, 100).Value = (object?)department.Location ?? DBNull.Value;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = department.IsActive;

        await connection.OpenAsync();
        object? result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<List<Department>> GetAllAsync(bool activeOnly = false)
    {
        string sql = """
            SELECT DepartmentId, DepartmentName, Location, IsActive
            FROM Departments
            """ + (activeOnly ? " WHERE IsActive = 1" : "") + " ORDER BY DepartmentId;";

        var list = new List<Department>();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new Department
            {
                DepartmentId = reader.GetInt32(0),
                DepartmentName = reader.GetString(1),
                Location = reader.IsDBNull(2) ? null : reader.GetString(2),
                IsActive = reader.GetBoolean(3)
            });
        }

        return list;
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT DepartmentId, DepartmentName, Location, IsActive
            FROM Departments
            WHERE DepartmentId = @DepartmentId;
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = id;

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Department
            {
                DepartmentId = reader.GetInt32(0),
                DepartmentName = reader.GetString(1),
                Location = reader.IsDBNull(2) ? null : reader.GetString(2),
                IsActive = reader.GetBoolean(3)
            };
        }

        return null;
    }

    public async Task<bool> UpdateAsync(Department department)
    {
        const string sql = """
            UPDATE Departments
            SET DepartmentName = @DepartmentName,
                Location = @Location,
                IsActive = @IsActive
            WHERE DepartmentId = @DepartmentId;
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = department.DepartmentId;
        command.Parameters.Add("@DepartmentName", SqlDbType.NVarChar, 100).Value = department.DepartmentName;
        command.Parameters.Add("@Location", SqlDbType.NVarChar, 100).Value = (object?)department.Location ?? DBNull.Value;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = department.IsActive;

        await connection.OpenAsync();
        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        const string sql = """
            UPDATE Departments
            SET IsActive = 0
            WHERE DepartmentId = @DepartmentId;
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = id;

        await connection.OpenAsync();
        int rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }
}
