using SchoolApp.Data;
using SchoolApp.Models;
using SchoolApp.Repositories;
using SchoolApp.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolApp.Tests.Repositories
{
    public class TeacherRepositoryTests
    {
        private readonly SchoolMvc9proContext _context;
        private readonly TeacherRepository _repository;
        private static CancellationToken Ct => TestContext.Current.CancellationToken;

        // @BeforeEach
        public TeacherRepositoryTests()
        {
            _context = TestDbContextFactory.Create();
            _repository = new TeacherRepository(_context);
        }

        // Happy path test for GetUserTeacherByUsernameAsync
        [Fact]
        public async Task GetUserTeacherByUsernameAsync_WhenUserIsTeacher_ReturnsUserWithTeacher()
        {
            // ══════════ ARRANGE ══════════
            // Στήνουμε έναν user που είναι teacher στη βάση 
            User teacherUser;
            User? user;

            teacherUser = CreateTeacherUser(
                username: "thanos",
                institution: "University of Athens"
            );

            await _context.Users.AddAsync(teacherUser, Ct);
            await _context.SaveChangesAsync(Ct);

            // ══════════ ACT ══════════
            // Καλούμε τη μέθοδο που τεστάρουμε
            user = await _repository.GetUserTeacherByUsernameAsync("thanos");

            // ══════════ ASSERT ══════════
            // Επαληθεύουμε το αποτέλεσμα
            Assert.NotNull(user);
            Assert.Equal("thanos", user.Username);
            Assert.NotNull(user.Teacher);
            Assert.Equal("University of Athens", user.Teacher.Institution);
        }

        // Null input test for GetUserTeacherByUsernameAsync
        [Fact]
        public async Task GetUserTeacherByUsernameAsync_WhenUsernameDoesNotExist_ReturnsNull()
        {
            // Arrange
            User? user;

            // Act
            user = await _repository.GetUserTeacherByUsernameAsync("nonexistent");

            // Assert
            Assert.Null(user);
        }

        // Business rule — ότι ο κανόνας «μόνο teachers» εφαρμόζεται σωστά
        [Fact]
        public async Task GetUserTeacherByUsernameAsync_WhenUserExistsButIsNotTeacher_ReturnsNull()
        {
            // Arrange
            User nonTeacherUser;
            User? user;

            nonTeacherUser = CreateNonTeacherUser(
                username: "anna"
            );

            await _context.Users.AddAsync(nonTeacherUser, Ct);
            await _context.SaveChangesAsync(Ct);

            // Act
            user = await _repository.GetUserTeacherByUsernameAsync("anna");

            // Assert
            Assert.Null(user);
        }

        // ---------- Helper methods ----------

        private static User CreateTeacherUser(string username, string institution)
        {
            Teacher teacher;
            User user;

            teacher = new Teacher
            {
                Institution = institution,
                PhoneNumber = "6900000000",
            };

            user = new User
            {
                Username = username,
                Email = $"{username}@gmail.com",
                Password = "hashed_password",
                Firstname = "Athanassios",
                Lastname = "Androutsos",
                RoleId = 2,
                Teacher = teacher
            };

            return user;
        }

        private static User CreateNonTeacherUser(string username)
        {
            User user;

            user = new User
            {
                Username = username,
                Email = $"{username}@gmail.com",
                Password = "hashed_password",
                Firstname = "Anna",
                Lastname = "Giannoutsos",
                RoleId = 3,
                Teacher = null
            };

            return user;
        }
    }
}