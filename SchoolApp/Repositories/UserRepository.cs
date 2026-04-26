using SchoolApp.Core;
using SchoolApp.Data;
using SchoolApp.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace SchoolApp.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(SchoolMvc9proContext context) : base(context)
        {
        }
        public async Task<User?> GetUserByUsernameAsync(string username) =>
            await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

        public async Task<PaginatedResult<User>> GetUsersAsync(int pageNumber, int pageSize,
            List<Expression<Func<User, bool>>> predicates)
        {
            int totalRecords;
            IQueryable<User> query = _context.Users;

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
    }
}