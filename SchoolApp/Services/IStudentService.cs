using SchoolApp.Core;
using SchoolApp.Data;
using SchoolApp.DTO;
using SchoolApp.Models;

namespace SchoolApp.Services
{
    public interface IStudentService
    {
        Task<PaginatedResult<UserReadOnlyDTO>> GetPaginatedStudentsAsync(int pageNumber, int pageSize);
    }
}