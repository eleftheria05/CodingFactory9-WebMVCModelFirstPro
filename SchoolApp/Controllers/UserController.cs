using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApp.DTO;
using SchoolApp.Exceptions;
using SchoolApp.Services;
using System.Security.Claims;

namespace SchoolApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IApplicationService applicationService;
        private readonly ILogger<UserController> logger;

        public UserController(IApplicationService applicationService, ILogger<UserController> logger)
        {
            this.applicationService = applicationService;
            this.logger = logger;
        }

        [HttpGet]
        [Authorize] // αυτό το action απαιτεί authenticated user".
        public IActionResult Index()
        {
            // User is a property on Controller (base class) that returns HttpContext.User
            // the principal
            //var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var roleClaim = User.FindFirstValue(ClaimTypes.Role);
            logger.LogInformation("DEBUG - Role claim value is: '{Role}'", roleClaim);

            if (User.IsInRole("ADMIN"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (User.IsInRole("TEACHER"))
            {
                return RedirectToAction("Index", "Teacher");
            }
            else if (User.IsInRole("STUDENT"))
            {
                return RedirectToAction("Index", "Student");
            }
            else
            {
                return RedirectToAction("AccessDenied", "Home");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            ClaimsPrincipal? principal = HttpContext.User;

            if (!principal!.Identity!.IsAuthenticated)
            {
                return View();
            }
            return RedirectToAction("Index", "User");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDTO credentials)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(credentials);
                }

                var user = await applicationService.UserService.VerifyAndGetUserAsync(credentials);

                //if (user == null)
                //{
                //    ViewData["ValidateMessage"] = "Bad Credentials. Username or Password is invalid.";
                //    return View();
                //}

                // Αυτά τα claims μπαίνουν μέσα στο authentication cookie
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Usually the user ID
                    new Claim(ClaimTypes.Name, user.Username), // This sets User.Identity.Name
                    new Claim(ClaimTypes.Role, user.Role.Name)
                };

                ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // AllowRefresh = true → Κάθε request ανανεώνει τον χρόνο λήξης
                // Δουλεύει μαζί με το SlidingExpiration Program.cs

                // Αν ο χρήστης τσέκαρε "Remember me", το cookie επιβιώνει και
                // μετά το κλείσιμο του browser.
                // Αλλιώς, είναι session cookie και χάνεται.
                AuthenticationProperties properties = new()
                {
                    AllowRefresh = true,
                    IsPersistent = credentials.KeepLoggedIn
                };

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    properties);

                //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                //    new ClaimsPrincipal(identity), properties);

                //var principal = new ClaimsPrincipal(identity);

                logger.LogInformation("User {Username} logged in", principal.Identity?.Name);
                return RedirectToAction("Index", "User");
            }
            catch (EntityNotAuthorizedException ex)
            {
                logger.LogWarning(ex, "Unauthorized login attempt for username: {Username}", credentials.Username);
                ViewData["ValidateMessage"] = "Bad Credentials. Username or password is invalid";
                return View(credentials);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during login");
                ViewData["ValidateMessage"] = ex.Message;
                return View(credentials);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            logger.LogInformation("User {UserName} logged out", username);
            return RedirectToAction("Login", "User");
        }
    }
}