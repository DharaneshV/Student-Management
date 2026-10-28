using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;
using StudentCourseManagement.Services;

// Initialize Repositories
var departmentRepo = new DepartmentRepository();
var studentRepo = new StudentRepository();
var courseRepo = new CourseRepository();
var enrollmentRepo = new EnrollmentRepository();
var auditLogRepo = new AuditLogRepository();
var reportRepo = new ReportRepository();

// Initialize Services
var departmentService = new DepartmentService(departmentRepo);
var studentService = new StudentService(studentRepo, departmentRepo);
var courseService = new CourseService(courseRepo, departmentRepo);
var enrollmentService = new EnrollmentService(enrollmentRepo, studentRepo, courseRepo);
var auditLogService = new AuditLogService(auditLogRepo);

while (true)
{
    Console.WriteLine("\n========================================");
    Console.WriteLine("     STUDENT COURSE MANAGEMENT SYSTEM   ");
    Console.WriteLine("========================================");
    Console.WriteLine("1. Department Management");
    Console.WriteLine("2. Student Management");
    Console.WriteLine("3. Course Management");
    Console.WriteLine("4. Enrollment Management");
    Console.WriteLine("5. Reports");
    Console.WriteLine("6. View Audit Logs");
    Console.WriteLine("0. Exit");
    Console.WriteLine("========================================");
    Console.Write("Enter your choice: ");

    string? choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
                await ManageDepartmentsAsync(departmentService);
                break;
            case "2":
                await ManageStudentsAsync(studentService);
                break;
            case "3":
                await ManageCoursesAsync(courseService);
                break;
            case "4":
                await ManageEnrollmentsAsync(enrollmentService);
                break;
            case "5":
                await ViewReportsAsync(reportRepo);
                break;
            case "6":
                await ViewAuditLogsAsync(auditLogService);
                break;
            case "0":
                Console.WriteLine("Application exited.");
                return;
            default:
                Console.WriteLine("Invalid option. Please try again.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Error]: {ex.Message}");
    }
}

// ----------------------------------------------------
// 1. Department Submenu
// ----------------------------------------------------
static async Task ManageDepartmentsAsync(DepartmentService service)
{
    Console.WriteLine("\n--- DEPARTMENT MANAGEMENT ---");
    Console.WriteLine("1. Add Department");
    Console.WriteLine("2. List All Departments");
    Console.WriteLine("3. List Active Departments Only");
    Console.WriteLine("4. Search Department By ID");
    Console.WriteLine("5. Update Department");
    Console.WriteLine("6. Deactivate Department");
    Console.Write("Choose: ");
    string? sub = Console.ReadLine();

    switch (sub)
    {
        case "1":
        {
            Console.Write("Department Name: ");
            string name = Console.ReadLine() ?? "";
            Console.Write("Location: ");
            string? loc = Console.ReadLine();

            int id = await service.AddDepartmentAsync(new Department
            {
                DepartmentName = name,
                Location = string.IsNullOrWhiteSpace(loc) ? null : loc,
                IsActive = true
            });
            Console.WriteLine($"Department created with ID: {id}");
            break;
        }
        case "2":
        case "3":
        {
            bool activeOnly = sub == "3";
            var depts = await service.GetAllDepartmentsAsync(activeOnly);
            if (depts.Count == 0)
            {
                Console.WriteLine("No departments found.");
                break;
            }
            Console.WriteLine("ID | Name | Location | Active");
            foreach (var d in depts)
                Console.WriteLine($"{d.DepartmentId} | {d.DepartmentName} | {d.Location ?? "N/A"} | {d.IsActive}");
            break;
        }
        case "4":
        {
            Console.Write("Enter Department ID: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var d = await service.GetDepartmentByIdAsync(id);
                if (d is null) Console.WriteLine("Department not found.");
                else Console.WriteLine($"ID: {d.DepartmentId}\nName: {d.DepartmentName}\nLocation: {d.Location ?? "N/A"}\nActive: {d.IsActive}");
            }
            break;
        }
        case "5":
        {
            Console.Write("Enter Department ID to update: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var existing = await service.GetDepartmentByIdAsync(id);
                if (existing is null) { Console.WriteLine("Department not found."); break; }

                Console.Write($"New Name ({existing.DepartmentName}): ");
                string? name = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(name)) existing.DepartmentName = name;

                Console.Write($"New Location ({existing.Location ?? "None"}): ");
                string? loc = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(loc)) existing.Location = loc;

                bool ok = await service.UpdateDepartmentAsync(existing);
                Console.WriteLine(ok ? "Department updated." : "Update failed.");
            }
            break;
        }
        case "6":
        {
            Console.Write("Enter Department ID to deactivate: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                bool ok = await service.DeactivateDepartmentAsync(id);
                Console.WriteLine(ok ? "Department deactivated." : "Deactivation failed.");
            }
            break;
        }
    }
}

// ----------------------------------------------------
// 2. Student Submenu
// ----------------------------------------------------
static async Task ManageStudentsAsync(StudentService service)
{
    Console.WriteLine("\n--- STUDENT MANAGEMENT ---");
    Console.WriteLine("1. Register Student");
    Console.WriteLine("2. List All Students");
    Console.WriteLine("3. List Active Students Only");
    Console.WriteLine("4. Search Student By ID");
    Console.WriteLine("5. Search Students By Name");
    Console.WriteLine("6. Filter Students By Department");
    Console.WriteLine("7. Update Student");
    Console.WriteLine("8. Deactivate Student");
    Console.Write("Choose: ");
    string? sub = Console.ReadLine();

    switch (sub)
    {
        case "1":
        {
            Console.Write("Name: ");
            string name = Console.ReadLine() ?? "";
            Console.Write("Email: ");
            string email = Console.ReadLine() ?? "";
            Console.Write("Age (17-60): ");
            int.TryParse(Console.ReadLine(), out int age);
            Console.Write("Department ID: ");
            int.TryParse(Console.ReadLine(), out int deptId);

            int id = await service.RegisterAsync(new Student
            {
                Name = name,
                Email = email,
                Age = age,
                DepartmentId = deptId,
                IsActive = true
            });
            Console.WriteLine($"Student registered with ID: {id}");
            break;
        }
        case "2":
        case "3":
        {
            bool activeOnly = sub == "3";
            var students = await service.GetAllStudentsAsync(activeOnly);
            PrintStudents(students);
            break;
        }
        case "4":
        {
            Console.Write("Student ID: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var s = await service.GetStudentByIdAsync(id);
                if (s is null) Console.WriteLine("Student not found.");
                else PrintStudents([s]);
            }
            break;
        }
        case "5":
        {
            Console.Write("Enter search name: ");
            string query = Console.ReadLine() ?? "";
            var results = await service.SearchStudentsByNameAsync(query);
            PrintStudents(results);
            break;
        }
        case "6":
        {
            Console.Write("Enter Department ID: ");
            if (int.TryParse(Console.ReadLine(), out int deptId))
            {
                var results = await service.GetStudentsByDepartmentAsync(deptId);
                PrintStudents(results);
            }
            break;
        }
        case "7":
        {
            Console.Write("Enter Student ID to update: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var s = await service.GetStudentByIdAsync(id);
                if (s is null) { Console.WriteLine("Student not found."); break; }

                Console.Write($"New Name ({s.Name}): ");
                string? name = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(name)) s.Name = name;

                Console.Write($"New Email ({s.Email}): ");
                string? email = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(email)) s.Email = email;

                Console.Write($"New Age ({s.Age}): ");
                if (int.TryParse(Console.ReadLine(), out int age)) s.Age = age;

                Console.Write($"New Department ID ({s.DepartmentId}): ");
                if (int.TryParse(Console.ReadLine(), out int deptId)) s.DepartmentId = deptId;

                bool ok = await service.UpdateStudentAsync(s);
                Console.WriteLine(ok ? "Student updated." : "Update failed.");
            }
            break;
        }
        case "8":
        {
            Console.Write("Enter Student ID to deactivate: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                bool ok = await service.DeactivateStudentAsync(id);
                Console.WriteLine(ok ? "Student deactivated." : "Deactivation failed.");
            }
            break;
        }
    }
}

// ----------------------------------------------------
// 3. Course Submenu
// ----------------------------------------------------
static async Task ManageCoursesAsync(CourseService service)
{
    Console.WriteLine("\n--- COURSE MANAGEMENT ---");
    Console.WriteLine("1. Add Course");
    Console.WriteLine("2. List All Courses");
    Console.WriteLine("3. List Active Courses Only");
    Console.WriteLine("4. Search Course By ID");
    Console.WriteLine("5. Filter Courses By Department");
    Console.WriteLine("6. Update Course");
    Console.WriteLine("7. Deactivate Course");
    Console.Write("Choose: ");
    string? sub = Console.ReadLine();

    switch (sub)
    {
        case "1":
        {
            Console.Write("Course Name: ");
            string name = Console.ReadLine() ?? "";
            Console.Write("Duration (months): ");
            int.TryParse(Console.ReadLine(), out int duration);
            Console.Write("Fee: ");
            decimal.TryParse(Console.ReadLine(), out decimal fee);
            Console.Write("Department ID: ");
            int.TryParse(Console.ReadLine(), out int deptId);

            int id = await service.AddCourseAsync(new Course
            {
                CourseName = name,
                DurationMonths = duration,
                Fee = fee,
                DepartmentId = deptId,
                IsActive = true
            });
            Console.WriteLine($"Course added with ID: {id}");
            break;
        }
        case "2":
        case "3":
        {
            bool activeOnly = sub == "3";
            var courses = await service.GetAllCoursesAsync(activeOnly);
            PrintCourses(courses);
            break;
        }
        case "4":
        {
            Console.Write("Course ID: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var c = await service.GetCourseByIdAsync(id);
                if (c is null) Console.WriteLine("Course not found.");
                else PrintCourses([c]);
            }
            break;
        }
        case "5":
        {
            Console.Write("Department ID: ");
            if (int.TryParse(Console.ReadLine(), out int deptId))
            {
                var courses = await service.GetCoursesByDepartmentAsync(deptId);
                PrintCourses(courses);
            }
            break;
        }
        case "6":
        {
            Console.Write("Enter Course ID to update: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var c = await service.GetCourseByIdAsync(id);
                if (c is null) { Console.WriteLine("Course not found."); break; }

                Console.Write($"New Name ({c.CourseName}): ");
                string? name = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(name)) c.CourseName = name;

                Console.Write($"New Duration ({c.DurationMonths}): ");
                if (int.TryParse(Console.ReadLine(), out int dur)) c.DurationMonths = dur;

                Console.Write($"New Fee ({c.Fee:N2}): ");
                if (decimal.TryParse(Console.ReadLine(), out decimal fee)) c.Fee = fee;

                Console.Write($"New Department ID ({c.DepartmentId}): ");
                if (int.TryParse(Console.ReadLine(), out int deptId)) c.DepartmentId = deptId;

                bool ok = await service.UpdateCourseAsync(c);
                Console.WriteLine(ok ? "Course updated." : "Update failed.");
            }
            break;
        }
        case "7":
        {
            Console.Write("Enter Course ID to deactivate: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                bool ok = await service.DeactivateCourseAsync(id);
                Console.WriteLine(ok ? "Course deactivated." : "Deactivation failed.");
            }
            break;
        }
    }
}

// ----------------------------------------------------
// 4. Enrollment Submenu (with Transactions)
// ----------------------------------------------------
static async Task ManageEnrollmentsAsync(EnrollmentService service)
{
    Console.WriteLine("\n--- ENROLLMENT MANAGEMENT ---");
    Console.WriteLine("1. Enroll Student (Transactional: Enrollment + AuditLog)");
    Console.WriteLine("2. List All Enrollments");
    Console.WriteLine("3. View Courses For Student");
    Console.WriteLine("4. View Students Enrolled In Course");
    Console.WriteLine("5. Update Enrollment Status (Completed/Cancelled)");
    Console.Write("Choose: ");
    string? sub = Console.ReadLine();

    switch (sub)
    {
        case "1":
        {
            Console.Write("Student ID: ");
            int.TryParse(Console.ReadLine(), out int studentId);
            Console.Write("Course ID: ");
            int.TryParse(Console.ReadLine(), out int courseId);

            int enrollmentId = await service.EnrollStudentAsync(studentId, courseId);
            Console.WriteLine($"Enrollment created successfully with ID: {enrollmentId} (Audit logged in transaction).");
            break;
        }
        case "2":
        {
            var list = await service.GetAllEnrollmentsAsync();
            if (list.Count == 0) { Console.WriteLine("No enrollments found."); break; }
            Console.WriteLine("ID | Student ID | Course ID | Status | Enrolled At");
            foreach (var e in list)
                Console.WriteLine($"{e.EnrollmentId} | {e.StudentId} | {e.CourseId} | {e.Status} | {e.EnrolledAt:g}");
            break;
        }
        case "3":
        {
            Console.Write("Student ID: ");
            if (int.TryParse(Console.ReadLine(), out int sId))
            {
                var courses = await service.GetCoursesForStudentAsync(sId);
                if (courses.Count == 0) Console.WriteLine("No enrolled courses.");
                else
                {
                    Console.WriteLine("Course ID | Course Name | Status | Enrolled Date");
                    foreach (var c in courses)
                        Console.WriteLine($"{c.CourseId} | {c.CourseName} | {c.Status} | {c.EnrolledAt:d}");
                }
            }
            break;
        }
        case "4":
        {
            Console.Write("Course ID: ");
            if (int.TryParse(Console.ReadLine(), out int cId))
            {
                var students = await service.GetStudentsForCourseAsync(cId);
                if (students.Count == 0) Console.WriteLine("No students enrolled.");
                else
                {
                    Console.WriteLine("Student ID | Student Name | Status | Enrolled Date");
                    foreach (var s in students)
                        Console.WriteLine($"{s.StudentId} | {s.StudentName} | {s.Status} | {s.EnrolledAt:d}");
                }
            }
            break;
        }
        case "5":
        {
            Console.Write("Enrollment ID: ");
            int.TryParse(Console.ReadLine(), out int eId);
            Console.Write("New Status (Enrolled / Completed / Cancelled): ");
            string status = Console.ReadLine() ?? "";

            bool ok = await service.UpdateEnrollmentStatusAsync(eId, status);
            Console.WriteLine(ok ? "Status updated and audit log recorded." : "Update failed.");
            break;
        }
    }
}

// ----------------------------------------------------
// 5. Reports Submenu
// ----------------------------------------------------
static async Task ViewReportsAsync(ReportRepository reports)
{
    Console.WriteLine("\n--- REPORTS & ANALYTICS ---");
    Console.WriteLine("1. Students with Department Names (JOIN)");
    Console.WriteLine("2. Student Count by Department (GROUP BY & Aggregate)");
    Console.WriteLine("3. Enrollment Count per Course (LEFT JOIN & COUNT)");
    Console.WriteLine("4. Students Without Enrollments (NOT EXISTS)");
    Console.WriteLine("5. Courses Without Enrollments (LEFT JOIN & NULL)");
    Console.Write("Choose: ");
    string? sub = Console.ReadLine();

    switch (sub)
    {
        case "1":
        {
            var list = await reports.GetStudentsWithDepartmentsAsync();
            Console.WriteLine("Student ID | Name | Email | Department");
            foreach (var r in list)
                Console.WriteLine($"{r.StudentId} | {r.Name} | {r.Email} | {r.DepartmentName}");
            break;
        }
        case "2":
        {
            var list = await reports.GetStudentCountByDepartmentAsync();
            Console.WriteLine("Department | Total Students");
            foreach (var r in list)
                Console.WriteLine($"{r.DepartmentName} | {r.StudentCount}");
            break;
        }
        case "3":
        {
            var list = await reports.GetEnrollmentCountPerCourseAsync();
            Console.WriteLine("Course ID | Course Name | Enrollments");
            foreach (var r in list)
                Console.WriteLine($"{r.CourseId} | {r.CourseName} | {r.EnrollmentCount}");
            break;
        }
        case "4":
        {
            var list = await reports.GetStudentsWithoutEnrollmentsAsync();
            if (list.Count == 0) Console.WriteLine("All students have at least one enrollment.");
            else
            {
                Console.WriteLine("Student ID | Name");
                foreach (var r in list)
                    Console.WriteLine($"{r.StudentId} | {r.Name}");
            }
            break;
        }
        case "5":
        {
            var list = await reports.GetCoursesWithoutEnrollmentsAsync();
            if (list.Count == 0) Console.WriteLine("All courses have enrollments.");
            else
            {
                Console.WriteLine("Course ID | Course Name");
                foreach (var r in list)
                    Console.WriteLine($"{r.CourseId} | {r.CourseName}");
            }
            break;
        }
    }
}

// ----------------------------------------------------
// 6. View Audit Logs
// ----------------------------------------------------
static async Task ViewAuditLogsAsync(AuditLogService service)
{
    Console.WriteLine("\n--- RECENT AUDIT LOGS ---");
    var logs = await service.GetRecentLogsAsync(20);
    if (logs.Count == 0)
    {
        Console.WriteLine("No audit records found.");
        return;
    }

    Console.WriteLine("Audit ID | Entity | ID | Action | Details | Timestamp");
    foreach (var a in logs)
    {
        Console.WriteLine($"{a.AuditId} | {a.EntityName} | {a.EntityId} | {a.Action} | {a.Details ?? "N/A"} | {a.CreatedAt:g}");
    }
}

// Helper Print Methods
static void PrintStudents(List<Student> students)
{
    if (students.Count == 0)
    {
        Console.WriteLine("No students found.");
        return;
    }
    Console.WriteLine("ID | Name | Email | Age | Dept ID | Admitted | Active");
    foreach (var s in students)
        Console.WriteLine($"{s.StudentId} | {s.Name} | {s.Email} | {s.Age} | {s.DepartmentId} | {s.AdmissionDate:d} | {s.IsActive}");
}

static void PrintCourses(List<Course> courses)
{
    if (courses.Count == 0)
    {
        Console.WriteLine("No courses found.");
        return;
    }
    Console.WriteLine("ID | Name | Duration | Fee | Dept ID | Active");
    foreach (var c in courses)
        Console.WriteLine($"{c.CourseId} | {c.CourseName} | {c.DurationMonths} mos | {c.Fee:N2} | {c.DepartmentId} | {c.IsActive}");
}
