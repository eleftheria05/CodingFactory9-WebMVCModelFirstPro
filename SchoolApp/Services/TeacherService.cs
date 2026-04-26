using AutoMapper;

using SchoolApp.DTO;
using SchoolApp.Exceptions;
using SchoolApp.Models;
using SchoolApp.Repositories;
using SchoolApp.Security;


namespace SchoolApp.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEncryptionUtil _encryptionUtil;
        private readonly ILogger<TeacherService> _logger;

        public TeacherService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TeacherService> logger, IEncryptionUtil encryptionUtil)
        {
            _encryptionUtil = encryptionUtil;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task SignUpUserAsync(TeacherSignupDTO request)
        {
            Teacher teacher = _mapper.Map<Teacher>(request);
            User user = _mapper.Map<User>(request);

            try
            {
                User? existingUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(user.Username);

                if (existingUser != null)
                {
                    throw new EntityAlreadyExistsException("User", "User with username " +
                        existingUser.Username + " already exists");
                }

                user.Teacher = teacher;
                user.Password = _encryptionUtil.Encrypt(user.Password);
                await _unitOfWork.UserRepository.AddAsync(user);
                await _unitOfWork.TeacherRepository.AddAsync(teacher);

                await _unitOfWork.SaveAsync();
                _logger.LogInformation("Teacher {Teacher} signed up successfully.", teacher);        // ToDo toString in Teacher
            }
            catch (EntityAlreadyExistsException ex)
            {
                _logger.LogError("Error signing up tecaher {Teacher}. {Message}", teacher, ex.Message);
                throw;
            }
        }
    }
}
