using Microsoft.EntityFrameworkCore;
using SchoolApp.Core;
using SchoolApp.Data;
using SchoolApp.Models;
using System.Linq.Expressions;

namespace SchoolApp.Repositories
{
    public class TeacherRepository : BaseRepository<Teacher>, ITeacherRepository
    {
        public TeacherRepository(SchoolMvc9proContext context) : base(context)
        {
        }

        public async Task<User?> GetUserTeacherByUsernameAsync(string username)
        {
            var userTeacher = await _context.Users
                .Include(u => u.Teacher) // Eager loading like Join in SQL
                .Where(u => u.Username == username && u.Teacher != null)
                .SingleOrDefaultAsync();    // fetches 0 or 1 results, throws Exception

            return userTeacher;
        }
        public async Task<PaginatedResult<User>> GetPaginatedTeachersAsync(int pageNumber, int pageSize, List<Expression<Func<User, bool>>> predicates)
        {
            int totalRecords;
            IQueryable<User> query = _context.Users
                .Include(u => u.Teacher)
                .Where(u => u.Teacher != null); // Φιλτράρουμε μόνο τους χρήστες που είναι δάσκαλοι

            if (predicates != null && predicates.Count > 0)
            {
                foreach (var predicate in predicates)
                {
                    query = query.Where(predicate); // υπονοείται το AND
                }
            }

            totalRecords = await query.CountAsync();
            int skip = (pageNumber - 1) * pageSize;

            var data = await query
                .OrderBy(u => u.Id) // Πάντα OrderBy για να διασφαλίσουμε την σταθερή σειρά των αποτελεσμάτων
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<User>()
            {
                Data = data,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<List<Course>> GetTeacherCoursesAsync(int teacherId)
        {
            List<Course> courses;

            courses = await _context.Courses
                .Where(c => c.TeacherId == teacherId)
                .ToListAsync();

            return courses;
        }


    }

}
