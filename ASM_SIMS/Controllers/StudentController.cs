using ASM_SIMS.DB;
using ASM_SIMS.Filters;
using ASM_SIMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM_SIMS.Controllers
{
    public class StudentController : Controller
    {
        private readonly SimsDataContext _dbContext;

        public StudentController(SimsDataContext dbContext)
        {
            _dbContext = dbContext; // DIP: Dependency Injection to reduce direct dependency
        }

        // Display the list of students
        [HttpGet]
        [RoleAuthorize("Student", "Index")]
        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Login");
            }

            var students = _dbContext.Students
                .Where(s => s.DeletedAt == null)
                .Include(s => s.ClassRoom)
                .Include(s => s.Course)
                .Select(s => new StudentViewModel
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    Phone = s.Phone,
                    Address = s.Address,
                    ClassRoomId = s.ClassRoomId,
                    CourseId = s.CourseId,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                }).ToList();

            ViewBag.ClassRooms = _dbContext.ClassRooms.ToList();
            ViewBag.Courses = _dbContext.Courses.ToList();
            ViewData["Title"] = "Students";

            // Lấy thông tin tài khoản vừa tạo từ TempData
            if (TempData["NewAccount"] != null)
            {
                ViewBag.NewAccount = Newtonsoft.Json.JsonConvert.DeserializeObject(TempData["NewAccount"].ToString());
            }
            return View(students);
        }

        // Display the form to add a student
        [HttpGet]
        [RoleAuthorize("Student", "Create")]
        public IActionResult Create()
        {
            ViewBag.ClassRooms = _dbContext.ClassRooms.ToList();
            ViewBag.Courses = _dbContext.Courses.ToList();
            return View(new StudentViewModel());
        }

        // Handle the creation of a student
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Student", "Create")]
        public IActionResult Create(StudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate Email and Phone within the same ClassRoomId
                if (_dbContext.Students.Any(s => s.Email == model.Email && s.ClassRoomId == model.ClassRoomId && s.DeletedAt == null))
                {
                    ModelState.AddModelError("Email", "Email already exists in this class.");
                }
                if (_dbContext.Students.Any(s => s.Phone == model.Phone && s.ClassRoomId == model.ClassRoomId && s.DeletedAt == null))
                {
                    ModelState.AddModelError("Phone", "Phone number already exists in this class.");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.ClassRooms = _dbContext.ClassRooms.ToList();
                    ViewBag.Courses = _dbContext.Courses.ToList();
                    return View(model);
                }

                try
                {
                    // Create an Account
                    var account = new Account
                    {
                        RoleId = 1, // Assume the role is Student
                        Username = model.Email.Split('@')[0],
                        Password = "defaultPassword123", // Should encrypt the password in practice
                        Email = model.Email,
                        Phone = model.Phone,
                        Address = model.Address ?? "",
                        CreatedAt = DateTime.Now
                    };
                    _dbContext.Accounts.Add(account);
                    _dbContext.SaveChanges();

                    // Create a Student
                    var student = new Student
                    {
                        AccountId = account.Id,
                        FullName = model.FullName,
                        Email = model.Email,
                        Phone = model.Phone,
                        Address = model.Address,
                        ClassRoomId = model.ClassRoomId,
                        CourseId = model.CourseId,
                        Status = "Active", // Default to Active when creating a new student
                        CreatedAt = DateTime.Now
                    };
                    _dbContext.Students.Add(student);
                    _dbContext.SaveChanges();

                    TempData["save"] = true;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["save"] = false;
                    ModelState.AddModelError("", $"Error adding student: {ex.Message} | Inner: {ex.InnerException?.Message}");
                }
            }
            ViewBag.ClassRooms = _dbContext.ClassRooms.ToList();
            ViewBag.Courses = _dbContext.Courses.ToList();
            return View(model);
        }

        // Display the form to edit a student
        [HttpGet]
        [RoleAuthorize("Student", "Edit")]
        public IActionResult Edit(int id)
        {
            var student = _dbContext.Students
                .Include(s => s.ClassRoom)
                .Include(s => s.Course)
                .FirstOrDefault(s => s.Id == id && s.DeletedAt == null);

            if (student == null)
            {
                return NotFound();
            }

            var model = new StudentViewModel
            {
                Id = student.Id,
                FullName = student.FullName,
                Email = student.Email,
                Phone = student.Phone,
                Address = student.Address,
                ClassRoomId = student.ClassRoomId,
                CourseId = student.CourseId,
                Status = student.Status
            };
            ViewBag.ClassRooms = _dbContext.ClassRooms.ToList();
            ViewBag.Courses = _dbContext.Courses.ToList();
            return View(model);
        }

        // Handle the editing of a student
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Student", "Edit")]
        public IActionResult Edit(StudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate Email and Phone within the same ClassRoomId, excluding the current record
                if (_dbContext.Students.Any(s => s.Email == model.Email && s.ClassRoomId == model.ClassRoomId && s.Id != model.Id && s.DeletedAt == null))
                {
                    ModelState.AddModelError("Email", "Email already exists in this class.");
                }
                if (_dbContext.Students.Any(s => s.Phone == model.Phone && s.ClassRoomId == model.ClassRoomId && s.Id != model.Id && s.DeletedAt == null))
                {
                    ModelState.AddModelError("Phone", "Phone number already exists in this class.");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.ClassRooms = _dbContext.ClassRooms.ToList();
                    ViewBag.Courses = _dbContext.Courses.ToList();
                    return View(model);
                }

                try
                {
                    var student = _dbContext.Students
                        .FirstOrDefault(s => s.Id == model.Id && s.DeletedAt == null);

                    if (student == null)
                    {
                        return NotFound();
                    }

                    // Update student information
                    student.FullName = model.FullName;
                    student.Email = model.Email;
                    student.Phone = model.Phone;
                    student.Address = model.Address;
                    student.ClassRoomId = model.ClassRoomId;
                    student.CourseId = model.CourseId;
                    student.Status = model.Status;
                    student.UpdatedAt = DateTime.Now;

                    _dbContext.Students.Update(student);
                    _dbContext.SaveChanges();

                    TempData["save"] = true;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["save"] = false;
                    ModelState.AddModelError("", $"Error editing student: {ex.Message} | Inner: {ex.InnerException?.Message}");
                }
            }
            ViewBag.ClassRooms = _dbContext.ClassRooms.ToList();
            ViewBag.Courses = _dbContext.Courses.ToList();
            return View(model);
        }

        // Handle the deletion of a student (soft delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Student", "Delete")]
        public IActionResult Delete(int id)
        {
            var student = _dbContext.Students
                .FirstOrDefault(s => s.Id == id && s.DeletedAt == null);

            if (student == null)
            {
                return NotFound();
            }

            try
            {
                student.DeletedAt = DateTime.Now;
                student.Status = "Deleted";
                _dbContext.Students.Remove(student);
                _dbContext.SaveChanges();
                TempData["save"] = true;
            }
            catch (Exception ex)
            {
                TempData["save"] = false;
                ModelState.AddModelError("", $"Error deleting student: {ex.Message}");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}