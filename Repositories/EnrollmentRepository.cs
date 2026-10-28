using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;
using System.Data;

namespace StudentCourseManagement.Repositories;

public class EnrollmentRepository
{
    public async Task<int> EnrollWithAuditAsync(int studentId, int courseId)
    {
        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        await using SqlTransaction transaction =
            (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            const string enrollmentSql = """
                INSERT INTO Enrollments (StudentId, CourseId, Status)
                VALUES (@StudentId, @CourseId, N'Enrolled');
                SELECT CAST(SCOPE_IDENTITY() AS INT);
                """;

            int enrollmentId;
            await using (SqlCommand command = new(enrollmentSql, connection, transaction))
            {
                command.Parameters.Add("@StudentId", SqlDbType.Int).Value = studentId;
                command.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;
                enrollmentId = Convert.ToInt32(await command.ExecuteScalarAsync());
            }

            const string auditSql = """
                INSERT INTO AuditLogs (EntityName, EntityId, Action, Details)
                VALUES (@EntityName, @EntityId, @Action, @Details);
                """;

            await using (SqlCommand command = new(auditSql, connection, transaction))
            {
                command.Parameters.Add("@EntityName", SqlDbType.NVarChar, 50).Value = "Enrollment";
                command.Parameters.Add("@EntityId", SqlDbType.Int).Value = enrollmentId;
                command.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "INSERT";
                command.Parameters.Add("@Details", SqlDbType.NVarChar, 500).Value =
                    $"Student {studentId} enrolled in course {courseId}.";
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return enrollmentId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Enrollment>> GetAllAsync()
    {
        const string sql = """
            SELECT EnrollmentId, StudentId, CourseId, EnrolledAt, Status
            FROM Enrollments
            ORDER BY EnrollmentId;
            """;

        var list = new List<Enrollment>();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new Enrollment
            {
                EnrollmentId = reader.GetInt32(0),
                StudentId = reader.GetInt32(1),
                CourseId = reader.GetInt32(2),
                EnrolledAt = reader.GetDateTime(3),
                Status = reader.GetString(4)
            });
        }

        return list;
    }

    public async Task<bool> UpdateStatusAsync(int enrollmentId, string status)
    {
        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        await using SqlTransaction transaction =
            (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            const string sql = """
                UPDATE Enrollments
                SET Status = @Status
                WHERE EnrollmentId = @EnrollmentId;
                """;

            int rows;
            await using (SqlCommand command = new(sql, connection, transaction))
            {
                command.Parameters.Add("@EnrollmentId", SqlDbType.Int).Value = enrollmentId;
                command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = status;
                rows = await command.ExecuteNonQueryAsync();
            }

            if (rows > 0)
            {
                const string auditSql = """
                    INSERT INTO AuditLogs (EntityName, EntityId, Action, Details)
                    VALUES (@EntityName, @EntityId, @Action, @Details);
                    """;

                await using (SqlCommand auditCmd = new(auditSql, connection, transaction))
                {
                    auditCmd.Parameters.Add("@EntityName", SqlDbType.NVarChar, 50).Value = "Enrollment";
                    auditCmd.Parameters.Add("@EntityId", SqlDbType.Int).Value = enrollmentId;
                    auditCmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "UPDATE";
                    auditCmd.Parameters.Add("@Details", SqlDbType.NVarChar, 500).Value =
                        $"Enrollment {enrollmentId} status updated to {status}.";
                    await auditCmd.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync();
            return rows > 0;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<(int CourseId, string CourseName, string Status, DateTime EnrolledAt)>> GetCoursesByStudentAsync(int studentId)
    {
        const string sql = """
            SELECT c.CourseId, c.CourseName, e.Status, e.EnrolledAt
            FROM Enrollments e
            INNER JOIN Courses c ON e.CourseId = c.CourseId
            WHERE e.StudentId = @StudentId
            ORDER BY c.CourseName;
            """;

        var list = new List<(int, string, string, DateTime)>();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@StudentId", SqlDbType.Int).Value = studentId;

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add((reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetDateTime(3)));
        }

        return list;
    }

    public async Task<List<(int StudentId, string StudentName, string Status, DateTime EnrolledAt)>> GetStudentsByCourseAsync(int courseId)
    {
        const string sql = """
            SELECT s.StudentId, s.Name, e.Status, e.EnrolledAt
            FROM Enrollments e
            INNER JOIN Students s ON e.StudentId = s.StudentId
            WHERE e.CourseId = @CourseId
            ORDER BY s.Name;
            """;

        var list = new List<(int, string, string, DateTime)>();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add((reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetDateTime(3)));
        }

        return list;
    }
}
