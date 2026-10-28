

USE master;
GO

IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'StudentCourseDB')
BEGIN
    ALTER DATABASE StudentCourseDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE StudentCourseDB;
END
GO

CREATE DATABASE StudentCourseDB;
GO

USE StudentCourseDB;
GO

CREATE TABLE Departments
(
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL UNIQUE,
    Location NVARCHAR(100) NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE Students
(
    StudentId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Age INT NOT NULL CHECK (Age BETWEEN 17 AND 60),
    DepartmentId INT NOT NULL,
    AdmissionDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Students_Departments
        FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId)
);
GO

CREATE TABLE Courses
(
    CourseId INT IDENTITY(1,1) PRIMARY KEY,
    CourseName NVARCHAR(100) NOT NULL UNIQUE,
    DurationMonths INT NOT NULL CHECK (DurationMonths > 0),
    Fee DECIMAL(10,2) NOT NULL CHECK (Fee >= 0),
    DepartmentId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Courses_Departments
        FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId)
);
GO

CREATE TABLE Enrollments
(
    EnrollmentId INT IDENTITY(1,1) PRIMARY KEY,
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    EnrolledAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    Status NVARCHAR(20) NOT NULL DEFAULT N'Enrolled',
    CONSTRAINT FK_Enrollments_Students
        FOREIGN KEY (StudentId) REFERENCES Students(StudentId),
    CONSTRAINT FK_Enrollments_Courses
        FOREIGN KEY (CourseId) REFERENCES Courses(CourseId),
    CONSTRAINT UQ_Enrollments_Student_Course UNIQUE (StudentId, CourseId),
    CONSTRAINT CK_Enrollments_Status
        CHECK (Status IN (N'Enrolled', N'Completed', N'Cancelled'))
);
GO

CREATE TABLE AuditLogs
(
    AuditId BIGINT IDENTITY(1,1) PRIMARY KEY,
    EntityName NVARCHAR(50) NOT NULL,
    EntityId INT NOT NULL,
    Action NVARCHAR(20) NOT NULL,
    Details NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
GO

-- Sample Seed Data
INSERT INTO Departments (DepartmentName, Location)
VALUES (N'Computer Science', N'Block A'),
       (N'Electronics', N'Block B'),
       (N'Information Technology', N'Block A');

INSERT INTO Students (Name, Email, Age, DepartmentId)
VALUES (N'Arun Kumar', N'arun@example.com', 21, 1),
       (N'Priya Devi', N'priya@example.com', 22, 3),
       (N'Kavin Raj', N'kavin@example.com', 20, 1);

INSERT INTO Courses (CourseName, DurationMonths, Fee, DepartmentId)
VALUES (N'C# Programming', 3, 8500.00, 1),
       (N'SQL Server', 2, 6000.00, 1),
       (N'Web Development', 4, 12000.00, 3);

INSERT INTO Enrollments (StudentId, CourseId)
VALUES (1, 1), (1, 2), (2, 3);
GO
