using Microsoft.AspNetCore.Mvc;
using TaskManager.CoreMVC.Enums;
using TaskManager.CoreMVC.Exceptions;
using TaskManager.CoreMVC.Models;
using TaskManager.CoreMVC.Repositories;

namespace TaskManager.CoreMVC.Controllers
{
    /// <summary>
    /// Employee Page Controller
    /// </summary>
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepo _empRepo;
        private readonly ILogger<EmployeeController> _logger;
        public EmployeeController(IEmployeeRepo empRepo, ILogger<EmployeeController> logger)
        {
            _empRepo = empRepo;
            _logger = logger;
        }

        #region Controller Action Methods

        /// <summary>
        /// Show Employee Page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                IEnumerable<EmployeeModel> model = new List<EmployeeModel>();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get data for Employees table (server side filter)
        /// </summary>
        /// <returns></returns>
        public IActionResult GetEmployees()
        {
            try
            {
                // Getting table filter info
                int draw = Convert.ToInt32(Request.Form["draw"]);
                int startIndex = Convert.ToInt32(Request.Form["start"]);
                int pageSize = Convert.ToInt32(Request.Form["length"]);
                int sortCol = Convert.ToInt32(Request.Form["order[0][column]"]);
                string sortDir = Request.Form["order[0][dir]"];
                string? searchField = Request.Form["search[value]"].FirstOrDefault()?.Trim();

                IEnumerable<EmployeeModel> list = _empRepo.GetAllEmployees();
                int totalCount = list.Count();

                // Perform search
                list = Search(searchField, list);

                // Perform sort
                list = Sort(sortCol, sortDir, list);

                int resultCount = list.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalCount,
                    recordsFiltered = resultCount,
                    data = list.Skip(startIndex).Take(pageSize).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Show Create Employee Or Edit Employee dialog
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult CreateOrEdit(string? id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    string employeeCode = _empRepo.GenerateEmployeeCode();
                    return View(new EmployeeModel() { EmployeeCode = employeeCode });
                }

                EmployeeModel? employeeModel = _empRepo.GetEmployeeById(id);
                if (employeeModel == null)
                {
                    return NotFound();
                }

                return View(employeeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Create or Edit Employee (Submit)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(EmployeeModel model)
        {
            try
            {
                string successMessage = string.Empty;
                // Create
                if (string.IsNullOrWhiteSpace(model.Id))
                {
                    Guid id = Guid.NewGuid();

                    byte[] salt = ServiceExtensions.Argon2.CreateSalt();
                    byte[] password = ServiceExtensions.Argon2.HashPassword(model.Password, salt);

                    await _empRepo.CreateEmployee(id.ToString(), model, Convert.ToBase64String(salt), Convert.ToBase64String(password));
                    successMessage = "Successfully created !";
                }
                // Edit
                else
                {
                    _empRepo.UpdateEmployee(model);
                    successMessage = "Successfully updated !";
                }
                await _empRepo.SaveChangesAsync();
                TempData["success"] = successMessage;

                return RedirectToAction("Index");
            }
            catch (TaskBoardException e)
            {
                TempData["error"] = e.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                TempData["error"] = "Unexpected error occurred !";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Show Delete Employee dialog
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest();
                }

                EmployeeModel? employeeModel = _empRepo.GetEmployeeById(id);
                if (employeeModel == null)
                {
                    return NotFound();
                }

                return View(employeeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Delete Employee (submit)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(EmployeeModel model)
        {
            try
            {
                _empRepo.DeleteEmployee(model.Id);
                await _empRepo.SaveChangesAsync();
                TempData["success"] = "Successfully deleted !";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                TempData["error"] = "Unexpected error occurred !";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Show change password dialog
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult ChangePassword(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest();
                }

                EmployeeModel? employeeModel = _empRepo.GetEmployeeById(id);
                if (employeeModel == null)
                {
                    return NotFound();
                }

                return View(employeeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Change Password (Submit)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(EmployeeModel model)
        {
            try
            {
                byte[] salt = ServiceExtensions.Argon2.CreateSalt();
                byte[] password = ServiceExtensions.Argon2.HashPassword(model.Password, salt);


                _empRepo.ChangePassword(model.Id, Convert.ToBase64String(salt), Convert.ToBase64String(password));
                await _empRepo.SaveChangesAsync();
                TempData["success"] = "Successfully updated !";

                return RedirectToAction("Index");
            }
            catch (TaskBoardException e)
            {
                TempData["error"] = e.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                TempData["error"] = "Unexpected error occurred !";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Search according to search field
        /// </summary>
        /// <param name="searchValue"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IEnumerable<EmployeeModel> Search(string? searchValue, IEnumerable<EmployeeModel> list)
        {
            if (!string.IsNullOrEmpty(searchValue))
            {
                list = list.Where(x =>
                    x.EmployeeCode.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    || x.Name.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    || x.RegistDate.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    || x.UpdateDate != null && x.UpdateDate.Contains(searchValue, StringComparison.OrdinalIgnoreCase));
            }
            return list;
        }

        /// <summary>
        /// Sort by sort column and order
        /// </summary>
        /// <param name="orderCol"></param>
        /// <param name="orderDir"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IEnumerable<EmployeeModel> Sort(int orderCol, string orderDir, IEnumerable<EmployeeModel> list)
        {
            Func<EmployeeModel, object> orderFunc = null;
            switch (orderCol)
            {
                case (int)EmpTableColOrder.Code:
                    orderFunc = x => x.EmployeeCode;
                    break;
                case (int)EmpTableColOrder.Name:
                    orderFunc = x => x.Name;
                    break;
                case (int)EmpTableColOrder.Admin:
                    orderFunc = x => x.IsAdmin;
                    break;
                case (int)EmpTableColOrder.RegisteredDate:
                    orderFunc = x => x.RegistDate;
                    break;
                case (int)EmpTableColOrder.UpdatedDate:
                    orderFunc = x => x.UpdateDate;
                    break;
            }

            if (orderDir != null && orderFunc != null)
            {
                if (orderDir.ToLower() == "desc")
                {
                    list = list.OrderByDescending(orderFunc);
                }
                else
                {
                    list = list.OrderBy(orderFunc);
                }
            }

            return list;
        }

        #endregion
    }
}
