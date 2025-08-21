using ASM_SIMS.DB;
using ASM_SIMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace ASM_SIMS.Controllers
{
    public class LoginController : Controller
    {
        private readonly SimsDataContext _dbContext;

        // DIP: Tiêm SimsDataContext qua constructor
        public LoginController(SimsDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Index()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Home");
            }
            //// Kiểm tra xem database có tài khoản nào không
            //ViewBag.CanCreateFirstAdmin = !_dbContext.Accounts.Any();
            return View(new LoginViewModel());
        }


        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = await _dbContext.Accounts
                    .Include(a => a.Role)
                    .FirstOrDefaultAsync(a => a.Email == model.Email && a.DeletedAt == null);

                if (account != null && BCrypt.Net.BCrypt.Verify(model.Password, account.Password))
                {
                    HttpContext.Session.SetString("UserId", account.Id.ToString());
                    HttpContext.Session.SetString("Username", account.Username);
                    HttpContext.Session.SetString("UserRole", account.Role.Name);
                    return RedirectToAction("Index", "Dashboard");
                }

                ViewData["MessageLogin"] = "Invalid email or password";
            }
            return View(model);
        }



        // POST: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa session
            return RedirectToAction("Index", "Login");
        }
    }
}
