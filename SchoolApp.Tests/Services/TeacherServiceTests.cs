using AutoMapper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SchoolApp.DTO;
using SchoolApp.Exceptions;
using SchoolApp.Models;
using SchoolApp.Repositories;
using SchoolApp.Security;
using SchoolApp.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolApp.Tests.Services
{
    public class TeacherServiceTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEncryptionUtil _encryptionUtil;
        private readonly ILogger<TeacherService> _logger;
        private readonly TeacherService _service;

        public TeacherServiceTests()
        {
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _mapper = Substitute.For<IMapper>();
            _encryptionUtil = Substitute.For<IEncryptionUtil>();
            _logger = Substitute.For<ILogger<TeacherService>>();

            _service = new TeacherService(_unitOfWork, _mapper, _logger, _encryptionUtil);
        }

        [Fact]
        public async Task SignUpUserAsync_WhenUsernameIsNew_AddsUserAndTeacherAndSaves()
        {
            // Arrange
            TeacherSignupDTO teacherSignupDTO;
            Teacher mappedTeacher;
            User mappedUser;

            teacherSignupDTO = CreateValidSignupDTO("thanos");
            mappedTeacher = new Teacher { Institution = "UoA" };
            mappedUser = new User { Username = "thanos", Password = "PlainPass123!" };

            _mapper.Map<Teacher>(teacherSignupDTO).Returns(mappedTeacher);
            _mapper.Map<User>(teacherSignupDTO).Returns(mappedUser);
            _unitOfWork.UserRepository.GetUserByUsernameAsync("thanos").Returns((User?)null);
            _encryptionUtil.Encrypt("PlainPass123!").Returns("encrypted_password");

            // Act
            await _service.SignUpUserAsync(teacherSignupDTO);

            // Assert
            await _unitOfWork.UserRepository.Received(1).AddAsync(mappedUser);
            await _unitOfWork.TeacherRepository.Received(1).AddAsync(mappedTeacher);
            await _unitOfWork.Received(1).SaveAsync();
        }

        [Fact]
        public async Task SignUpUserAsync_WhenUsernameAlreadyExists_ThrowsEntityAlreadyExistsException()
        {
            // Arrange
            TeacherSignupDTO teacherSignupDTO;
            User existingUser;
            Teacher mappedTeacher;
            User mappedUser;

            teacherSignupDTO = CreateValidSignupDTO("thanos");
            mappedTeacher = new Teacher { Institution = "UoA" };
            mappedUser = new User { Username = "thanos" };
            existingUser = new User { Id = 1, Username = "thanos" };

            _mapper.Map<Teacher>(teacherSignupDTO).Returns(mappedTeacher);
            _mapper.Map<User>(teacherSignupDTO).Returns(mappedUser);
            _unitOfWork.UserRepository.GetUserByUsernameAsync("thanos").Returns(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<EntityAlreadyExistsException>(
                () => _service.SignUpUserAsync(teacherSignupDTO)
            );

            // Επαληθεύουμε ότι ΔΕΝ έγινε αποθήκευση
            await _unitOfWork.UserRepository.DidNotReceive().AddAsync(Arg.Any<User>());
            await _unitOfWork.TeacherRepository.DidNotReceive().AddAsync(Arg.Any<Teacher>());
            await _unitOfWork.DidNotReceive().SaveAsync();
        }

        [Fact]
        public async Task SignUpUserAsync_WhenUsernameIsNew_EncryptsPasswordBeforeSaving()
        {
            // Arrange
            TeacherSignupDTO request;
            Teacher mappedTeacher;
            User mappedUser;

            request = CreateValidSignupDTO("thanos");
            mappedTeacher = new Teacher { Institution = "UoA" };
            mappedUser = new User { Username = "thanos", Password = "PlainPass123!" };

            _mapper.Map<Teacher>(request).Returns(mappedTeacher);
            _mapper.Map<User>(request).Returns(mappedUser);
            _unitOfWork.UserRepository.GetUserByUsernameAsync("thanos").Returns((User?)null);
            _encryptionUtil.Encrypt("PlainPass123!").Returns("encrypted_password");

            // Act
            await _service.SignUpUserAsync(request);

            // Assert
            _encryptionUtil.Received(1).Encrypt("PlainPass123!");
            Assert.Equal("encrypted_password", mappedUser.Password);
        }

        // ---------- Helper methods ----------

        private static TeacherSignupDTO CreateValidSignupDTO(string username)
        {
            TeacherSignupDTO request = new()
            {
                Username = username,
                Email = $"{username}@gmail.com",
                Password = "PlainPass123!",
                Firstname = "Anna",
                Lastname = "Giannoutsos",
                PhoneNumber = "6900000000",
                Institution = "UoA",
                RoleId = 2
            };

            return request;
        }
    }
}
