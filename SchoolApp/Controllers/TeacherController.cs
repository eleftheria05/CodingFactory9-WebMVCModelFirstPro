using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApp.Core;
using SchoolApp.DTO;
using SchoolApp.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Error = SchoolApp.Core.Error;

namespace SchoolApp.Controllers
{
    public class TeacherController : Controller
    {
        private readonly IApplicationService _applicationService;
        public List<Error> ErrorArray { get; set; } = [];

        public TeacherController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        [HttpGet]
        [Authorize(Roles = "TEACHER")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Signup(TeacherSignupDTO teacherSignupDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(teacherSignupDTO);
            }

            try
            {
                await _applicationService.TeacherService.SignUpUserAsync(teacherSignupDTO);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ErrorArray.Add(new Error("", ex.Message, ""));
                ViewData["ErrorArray"] = ErrorArray;
                return View(teacherSignupDTO);
            }
        }



    }
}