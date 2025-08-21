using ASM_SIMS.DB;
using ASM_SIMS.Filters;
using ASM_SIMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using BCrypt.Net;

namespace ASM_SIMS.Controllers
{
    public class RegisterController : Controller
    {
        private readonly SimsDataContext _dbContext;

        public RegisterController(SimsDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Hiển thị trang đăng ký
        [HttpGet]
        public IActionResult Index()
        {
            // Kiểm tra nếu đã đăng nhập thì chuyển hướng về trang chủ
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegisterViewModel());
        }

        // POST: Xử lý đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Chỉ cho phép tạo tài khoản với role Student hoặc Teacher
                if (model.Role != "Student" && model.Role != "Teacher")
                {
                    ModelState.AddModelError("Role", "Only Student or Teacher roles can be registered.");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại chưa
                if (await _dbContext.Accounts.AnyAsync(a => a.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }

                // Kiểm tra username đã tồn tại chưa
                if (await _dbContext.Accounts.AnyAsync(a => a.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "This username is already taken.");
                    return View(model);
                }

                // Kiểm tra số điện thoại đã tồn tại chưa
                if (await _dbContext.Accounts.AnyAsync(a => a.Phone == model.Phone))
                {
                    ModelState.AddModelError("Phone", "This phone number is already registered.");
                    return View(model);
                }

                try
                {
                    // Tạo tài khoản Account
                    var account = new Account
                    {
                        RoleId = model.Role switch
                        {
                            "Student" => 3, // Giả định RoleId cho Student là 3
                            "Teacher" => 2, // Giả định RoleId cho Teacher là 2
                            _ => throw new InvalidOperationException("Invalid role")
                        },
                        Username = model.Username,
                        Password = BCrypt.Net.BCrypt.HashPassword(model.Password), // Băm mật khẩu
                        Email = model.Email,
                        Phone = model.Phone,
                        Address = model.Address ?? "",
                        CreatedAt = DateTime.Now
                    };
                    _dbContext.Accounts.Add(account);
                    await _dbContext.SaveChangesAsync();

                    // Tạo bản ghi Student hoặc Teacher
                    if (model.Role == "Student")
                    {
                        var student = new Student
                        {
                            AccountId = account.Id,
                            FullName = model.FullName,
                            Email = model.Email,
                            Phone = model.Phone,
                            Address = model.Address,
                            Status = "Active",
                            CreatedAt = DateTime.Now
                        };
                        _dbContext.Students.Add(student);
                        await _dbContext.SaveChangesAsync();

                        // Đăng ký thành công, chuyển hướng đến trang login với thông báo
                        TempData["Message"] = "Registration successful! You can now login.";
                        return RedirectToAction("Index", "Login");
                    }
                    else // model.Role == "Teacher"
                    {
                        var teacher = new Teacher
                        {
                            AccountId = account.Id,
                            FullName = model.FullName,
                            Email = model.Email,
                            Phone = model.Phone,
                            Address = model.Address,
                            Status = "Active",
                            CreatedAt = DateTime.Now
                        };
                        _dbContext.Teachers.Add(teacher);
                        await _dbContext.SaveChangesAsync();

                        // Đăng ký thành công, chuyển hướng đến trang login với thông báo
                        TempData["Message"] = "Registration successful! You can now login.";
                        return RedirectToAction("Index", "Login");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error registering account: {ex.Message}");
                    return View(model);
                }
            }

            // Nếu ModelState không hợp lệ, trả về view với model để hiển thị lỗi validation
            return View(model);
        }
    }
}