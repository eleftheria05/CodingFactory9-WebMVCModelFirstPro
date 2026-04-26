namespace SchoolApp.Repositories
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        ITeacherRepository TeacherRepository { get; }
        IStudentRepository StudentRepository { get; }
        ICourseRepository CourseRepository { get; }

        Task<bool> SaveAsync();
    }
}
