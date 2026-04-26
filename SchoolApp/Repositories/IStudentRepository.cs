using SchoolApp.Core;
using SchoolApp.Data;
using SchoolApp.Models;
using System.Linq.Expressions;

namespace SchoolApp.Repositories
{
    public interface IStudentRepository : IBaseRepository<Student>
    {
        Task<List<Course>> GetStudentCoursesAsync(int studentId);
        Task<Student?> GetByAmAsync(string? am);
        Task<PaginatedResult<User>> GetPaginatedUsersStudentsAsync(int pageNumber, int pageSize);
        Task<PaginatedResult<Student>> GetPaginatedUsersStudentsFilteredAsync(int pageNumber, int pageSize,
            List<Expression<Func<Student, bool>>> predicates);
    }
}