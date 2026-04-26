using AutoMapper;
using SchoolApp.Core;
using SchoolApp.Core.Filters;
using SchoolApp.DTO;
using SchoolApp.Exceptions;
using SchoolApp.Models;
using SchoolApp.Repositories;
using SchoolApp.Security;
using System.Linq.Expressions;

namespace SchoolApp.Services
{
    public class UserService : IUserService
    {
        private readonly IEncryptionUtil _encryptionUtil;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper,
            ILogger<UserService> logger, IEncryptionUtil encryptionUtil)
        {
            _encryptionUtil = encryptionUtil;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PaginatedResult<UserReadOnlyDTO>> GetPaginatedUsersFilteredAsync(
            int pageNumber, int pageSize, UserFiltersDTO userFiltersDTO)
        {
            List<User> users = [];
            List<Expression<Func<User, bool>>> predicates = [];

            if (!string.IsNullOrEmpty(userFiltersDTO.Username))
            {
                predicates.Add(u => u.Username == userFiltersDTO.Username);
            }
            if (!string.IsNullOrEmpty(userFiltersDTO.Email))
            {
                predicates.Add(u => u.Email == userFiltersDTO.Email);
            }
            if (!string.IsNullOrEmpty(userFiltersDTO.UserRole))
            {
                predicates.Add(u => u.Role.Name == userFiltersDTO.UserRole);
            }

            var result = await _unitOfWork.UserRepository.GetUsersAsync(pageNumber, pageSize,
                predicates);

            var dtoResult = new PaginatedResult<UserReadOnlyDTO>()
            {
                Data = _mapper.Map<List<UserReadOnlyDTO>>(result.Data),
                TotalRecords = result.TotalRecords,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            _logger.LogInformation("Retrieved {Count} users", dtoResult.Data.Count);
            return dtoResult;
        }

        public async Task<UserReadOnlyDTO?> GetUserByUsernameAsync(string username)
        {
            try
            {
                User? user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    throw new EntityNotFoundException("User", "User with username: " +
                        " not found");
                }

                _logger.LogInformation("User found: {Username}", username);
                return _mapper.Map<UserReadOnlyDTO>(user);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogError("Error retrieving user by username: {Username}. {Message}",
                    username, ex.Message);
                throw;
            }
        }

        public async Task<User> VerifyAndGetUserAsync(UserLoginDTO credentials)
        {
            User? user = null;
            try
            {
                user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(credentials.Username!);

                if (user == null || !_encryptionUtil.IsValidPassword(credentials.Password!,
                    user.Password))
                {
                    throw new EntityNotAuthorizedException("User", Resources.ErrorMessages.BadCredentials);
                }

                _logger.LogInformation("User with username {Username} verified for login",
                    credentials.Username!);
            }
            catch (EntityNotAuthorizedException e)
            {
                _logger.LogError("Authentication failed for username {Username}. {Message}",
                    credentials.Username, e.Message);
            }
            return user;
        }
    }
}