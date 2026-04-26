namespace SchoolApp.Services
{
    public class ApplicationService : IApplicationService
    {
        public IUserService UserService { get; }

        public ITeacherService TeacherService { get; }

        public IStudentService StudentService { get; }

        public ApplicationService(IUserService userService, ITeacherService teacherService,
            IStudentService studentService)
        {
            UserService = userService;
            TeacherService = teacherService;
            StudentService = studentService;
        }
    }
}
