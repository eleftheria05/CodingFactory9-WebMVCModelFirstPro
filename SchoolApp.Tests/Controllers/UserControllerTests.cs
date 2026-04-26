using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SchoolApp.Controllers;
using SchoolApp.DTO;
using SchoolApp.Exceptions;
using SchoolApp.Models;
using SchoolApp.Services;
using System.Security.Claims;

namespace SchoolApp.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly IApplicationService _applicationService;
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly UserController _userController;

        public UserControllerTests()
        {
            _applicationService = Substitute.For<IApplicationService>();
            _userService = Substitute.For<IUserService>();
            _logger = Substitute.For<ILogger<UserController>>();

            _applicationService.UserService.Returns(_userService);

            _userController = new UserController(_applicationService, _logger);
        }

        // ==================== Index ====================

        // Assertion: «Αν το result δεν είναι RedirectToActionResult, απότυχε»
        // Cast: Εφόσον είναι, το κάνει cast ώστε να μπορούμε να προσπελάσουμε τα properties του

        [Fact]
        public void Index_WhenUserIsAdmin_RedirectsToAdminIndex()
        {
            // Arrange
            SetControllerUser(_userController, role: "ADMIN");

            // Act
            IActionResult result = _userController.Index();

            // Assert
            RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Admin", redirect.ControllerName);
        }

        [Fact]
        public void Index_WhenUserIsTeacher_RedirectsToTeacherIndex()
        {
            // Arrange
            SetControllerUser(_userController, role: "TEACHER");

            // Act
            IActionResult result = _userController.Index();

            // Assert
            RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Teacher", redirect.ControllerName);
        }

        [Fact]
        public void Index_WhenUserIsStudent_RedirectsToStudentIndex()
        {
            // Arrange
            SetControllerUser(_userController, role: "STUDENT");

            // Act
            IActionResult result = _userController.Index();

            // Assert
            RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Student", redirect.ControllerName);
        }

        [Fact]
        public void Index_WhenUserHasUnknownRole_RedirectsToAccessDenied()
        {
            // Arrange
            SetControllerUser(_userController, role: "UNKNOWN");

            // Act
            IActionResult result = _userController.Index();

            // Assert
            RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDenied", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        // ==================== Login GET ====================

        [Fact]
        public void LoginGet_WhenUserIsNotAuthenticated_ReturnsView()
        {
            // Arrange
            SetControllerUser(_userController, role: null, isAuthenticated: false);

            // Act
            IActionResult result = _userController.Login();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void LoginGet_WhenUserIsAuthenticated_RedirectsToUserIndex()
        {
            // Arrange
            SetControllerUser(_userController, role: "TEACHER", isAuthenticated: true);

            // Act
            IActionResult result = _userController.Login();

            // Assert
            RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("User", redirect.ControllerName);
        }

        // ==================== Login POST ====================

        // Όταν το ModelState είναι invalid, ο controller επιστρέφει αμέσως τη view με τα ίδια credentials
        // και δεν τρέχει καθόλου τη λογική authentication.

        [Fact]
        public async Task LoginPost_WhenModelStateIsInvalid_ReturnsViewWithCredentials()
        {
            // Arrange
            UserLoginDTO credentials = new UserLoginDTO { Username = "thanos" };
            _userController.ModelState.AddModelError("Password", "Password is required");

            // Act
            IActionResult result = await _userController.Login(credentials);

            // Assert
            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Same(credentials, view.Model);   // Model == UserLoginDTO. Check Ίδιο object στη μνήμη (reference equality)
            await _userService.DidNotReceive().VerifyAndGetUserAsync(Arg.Any<UserLoginDTO>());
        }

        [Fact]
        public async Task LoginPost_WhenCredentialsAreInvalid_ReturnsViewWithBadCredentialsMessage()
        {
            // Arrange
            UserLoginDTO credentials = new UserLoginDTO
            {
                Username = "thanos",
                Password = "WrongPass123!"
            };

            _userService
                .VerifyAndGetUserAsync(credentials)
                .Returns<Task<User>>(_
                => throw new EntityNotAuthorizedException("User", "Bad Credentials"));

            // Act
            IActionResult result = await _userController.Login(credentials);

            // Assert
            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Same(credentials, view.Model);
            Assert.Equal(
                "Bad Credentials. Username or password is invalid",
                _userController.ViewData["ValidateMessage"]
            );
        }

        [Fact]
        public async Task LoginPost_WhenServiceThrowsUnexpectedException_ReturnsViewWithExceptionMessage()
        {
            // Arrange
            UserLoginDTO credentials = new UserLoginDTO
            {
                Username = "thanos",
                Password = "SomePass123!"
            };

            _userService
                .VerifyAndGetUserAsync(credentials)
                .Returns<Task<User>>(_ => throw new InvalidOperationException("Database unreachable"));

            // Act
            IActionResult result = await _userController.Login(credentials);

            // Assert
            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Same(credentials, view.Model);
            Assert.Equal("Database unreachable", _userController.ViewData["ValidateMessage"]);
        }

        // ==================== Helper methods ====================

        // Η SetControllerUser φτιάχνει ένα ClaimsPrincipal με claim ClaimTypes.Role = role
        // και το βάζει στο HttpContext του controller.

        private static void SetControllerUser(
            UserController controller,
            string? role,
            bool isAuthenticated = true)
        {
            List<Claim> claims = new List<Claim>();

            if (role != null)
            {
                claims.Add(new Claim(ClaimTypes.Name, "testuser"));
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            string? authType = isAuthenticated ? "TestAuth" : null;
            ClaimsIdentity identity = new ClaimsIdentity(claims, authType);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
    }
}