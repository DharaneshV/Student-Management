using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;
using System.Data;

namespace StudentCourseManagement.Repositories;

public class AuditLogRepository
{
    public async Task<List<AuditLog>> GetRecentLogsAsync(int count = 20)
    {
        const string sql = """
            SELECT TOP (@Count) AuditId, EntityName, EntityId, Action, Details, CreatedAt
            FROM AuditLogs
            ORDER BY AuditId DESC;
            """;

        var list = new List<AuditLog>();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@Count", SqlDbType.Int).Value = count;

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new AuditLog
            {
                AuditId = reader.GetInt64(0),
                EntityName = reader.GetString(1),
                EntityId = reader.GetInt32(2),
                Action = reader.GetString(3),
                Details = reader.IsDBNull(4) ? null : reader.GetString(4),
                CreatedAt = reader.GetDateTime(5)
            });
        }

        return list;
    }
}
