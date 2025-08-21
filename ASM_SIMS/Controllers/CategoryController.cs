using ASM_SIMS.Models;
using Microsoft.AspNetCore.Mvc;
using ASM_SIMS.Helpers;
using ASM_SIMS.DB;
using Microsoft.EntityFrameworkCore;
using ASM_SIMS.Filters;

namespace ASM_SIMS.Controllers
{
    public class CategoryController : Controller
    {
        private readonly SimsDataContext _dbcontext;
        public CategoryController(SimsDataContext context)
        {
            _dbcontext = context;
        }

        [HttpGet]
        [RoleAuthorize("Category", "Index")] // Thêm [RoleAuthorize] để giới hạn quyền truy cập.
        public IActionResult Index()
        {

            // Tao list de hien thi du lieu
            CategoryViewModel categoryModel = new CategoryViewModel
            {
                categoryList = _dbcontext.Categories
                    .Where(c => c.DeletedAt == null)
                    .Select(m => new CategoryDetail
                    {
                        Id = m.Id,
                        NameCategory = m.NameCategory,
                        Description = m.Description,
                        Avartar = m.Avatar,
                        Status = m.Status,
                        UpdatedAt = m.UpdatedAt,
                        CreatedAt = m.CreatedAt
                    }).ToList()
            };
            ViewData["title"] = "Category";
            return View(categoryModel);
        }

        [HttpGet]
        [RoleAuthorize("Category", "Create")]
        public IActionResult Create()
        {

            return View(new CategoryDetail());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Category", "Create")]
        public async Task<IActionResult> Create(CategoryDetail model, IFormFile ViewAvatar)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    UploadFile uploadFile = new UploadFile(ViewAvatar);
                    string fileAvatar = uploadFile.Upload("images");
                    var dataCreate = new Categories()
                    {
                        NameCategory = model.NameCategory,
                        Description = model.Description,
                        Avatar = fileAvatar,
                        Status = "Active",
                        CreatedAt = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),

                    };
                    _dbcontext.Categories.Add(dataCreate);
                    await _dbcontext.SaveChangesAsync();
                    TempData["save"] = true;

                }
                catch (Exception ex)
                {
                    TempData["save"] = false;

                }
                return RedirectToAction("Index", "Category");
            }
            return View(model);

        }

        // GET: Hiển thị form chỉnh sửa danh mục
        [HttpGet]
        [RoleAuthorize("Category", "Edit")]
        public IActionResult Edit(int id)
        {
            var category = _dbcontext.Categories
                .FirstOrDefault(c => c.Id == id && c.DeletedAt == null);

            if (category == null)
            {
                return NotFound();
            }

            var model = new CategoryDetail
            {
                Id = category.Id,
                NameCategory = category.NameCategory,
                Description = category.Description,
                Avartar = category.Avatar,
                Status = category.Status
            };

            return View(model);
        }

        // POST: Cập nhật danh mục
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Category", "Edit")]
        public async Task<IActionResult> Edit(CategoryDetail model, IFormFile ViewAvatar)
        {
            // Không yêu cầu ModelState.IsValid hoàn toàn, chỉ cần kiểm tra dữ liệu cần thiết
            if (model.Id > 0) // Kiểm tra Id hợp lệ
            {
                try
                {
                    var category = _dbcontext.Categories
                        .FirstOrDefault(c => c.Id == model.Id && c.DeletedAt == null);

                    if (category == null)
                    {
                        return NotFound();
                    }

                    // Trường hợp 1: Chỉ cập nhật ảnh nếu có file mới
                    if (ViewAvatar != null)
                    {
                        var uploadFile = new UploadFile(ViewAvatar);
                        category.Avatar = uploadFile.Upload("images");
                    }
                    // Trường hợp 2: Nếu không có file mới, giữ nguyên ảnh cũ (category.Avatar không thay đổi)

                    // Cập nhật các trường khác nếu có thay đổi
                    category.NameCategory = model.NameCategory;
                    category.Description = model.Description;
                    category.Status = model.Status;
                    category.UpdatedAt = DateTime.Now;

                    _dbcontext.Categories.Update(category);
                    await _dbcontext.SaveChangesAsync();
                    TempData["save"] = true;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["save"] = false;
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid category ID");
            }
            return View(model);
        }

        // POST: Xóa danh mục (soft delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Category", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = _dbcontext.Categories
                .FirstOrDefault(c => c.Id == id && c.DeletedAt == null);

            if (category == null)
            {
                TempData["save"] = false;
                TempData["message"] = "Category not found";
                return NotFound();
            }

            try
            {
                // Kiểm tra xem danh mục có được tham chiếu bởi Courses không
                bool isReferenced = await _dbcontext.Courses.AnyAsync(c => c.CategoryId == id && c.DeletedAt == null);

                if (isReferenced)
                {
                    TempData["save"] = false;
                    TempData["message"] = "Delete Fail: This category is being used by one or more courses";
                    return RedirectToAction(nameof(Index));
                }

                category.DeletedAt = DateTime.Now;
                category.Status = "Deleted";
                _dbcontext.Categories.Remove(category);
                await _dbcontext.SaveChangesAsync();

                TempData["save"] = true;
                TempData["message"] = "Category deleted successfully";
            }
            catch (Exception)
            {
                TempData["save"] = false;
                TempData["message"] = "Delete Fail: An error occurred";
            }

            return RedirectToAction(nameof(Index));
        }
    }

}
