using Microsoft.AspNetCore.Mvc;
using TaskManager.CoreMVC.Database;
using TaskManager.CoreMVC.Models;
using TaskManager.CoreMVC.Repositories;

namespace TaskManager.CoreMVC.Controllers
{
    /// <summary>
    /// Login Page Controller
    /// </summary>
    public class LoginController : Controller
    {
        private readonly IEmployeeRepo _empRepo;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IEmployeeRepo employeeRepo, ILogger<LoginController> logger)
        {
            _empRepo = employeeRepo;
            _logger = logger;
        }

        /// <summary>
        /// Show Login Page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Log In (submit)
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogIn(LoginModel loginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ResponseModel response = new ResponseModel();
                    Employee? emp = _empRepo.GetEmployee(loginModel.EmployeeCode);
                    if (emp == null)
                    {
                        response.Success = false;
                        response.ErrorMessage = "Invalid employee code !";
                        return Ok(response);
                    }

                    bool isSuccess = ServiceExtensions.Argon2.VerifyHash(loginModel.Password, Convert.FromBase64String(emp.PwSalt), Convert.FromBase64String(emp.Password));
                    if (!isSuccess)
                    {
                        response.Success = false;
                        response.ErrorMessage = "Invalid employee code or password !";
                        return Ok(response);
                    }

                    // Store login User in session
                    HttpContext.Session.SetString("Id", emp.Id);
                    HttpContext.Session.SetString("Admin", emp.IsAdmin ? "Admin" : "");

                    response.Success = true;
                    response.Url = Url.Action("Index", "Dashboard");
                    return Ok(response);
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Log Out
        /// </summary>
        /// <returns></returns>
        public IActionResult LogOut()
        {
            try
            {
                // Clear session
                HttpContext.Session.Remove("Id");
                HttpContext.Session.Remove("Admin");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}