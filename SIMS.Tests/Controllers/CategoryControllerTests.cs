using ASM_SIMS.Controllers;
using ASM_SIMS.DB;
using ASM_SIMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace SIMS.Tests.Controllers
{
    public class CategoryControllerTests
    {
        private readonly SimsDataContext _dbContext;

        public CategoryControllerTests()
        {
            var options = new DbContextOptionsBuilder<SimsDataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid().ToString())
                .Options;
            _dbContext = new SimsDataContext(options);
        }

        private void ClearDatabase()
        {
            if (_dbContext.Categories.Any())
            {
                _dbContext.Categories.RemoveRange(_dbContext.Categories);
                _dbContext.SaveChanges();
            }
        }

        [Fact]
        public void Index_ReturnsViewWithCategoryList()
        {
            ClearDatabase();
            _dbContext.Categories.Add(new Categories
            {
                NameCategory = "Test Category",
                Description = "Test Description",
                Avatar = "test.jpg",
                Status = "Active",
                CreatedAt = DateTime.Now
            });
            _dbContext.SaveChanges();
            var controller = new CategoryController(_dbContext);

            var result = controller.Index() as ViewResult;
            var model = result.Model as CategoryViewModel;

            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Single(model.categoryList);
            Assert.Equal("Test Category", model.categoryList[0].NameCategory);
        }

        [Fact]
        public void Create_Get_ReturnsViewWithEmptyModel()
        {
            var controller = new CategoryController(_dbContext);

            var result = controller.Create() as ViewResult;
            var model = result.Model as CategoryDetail;

            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Null(model.NameCategory);
        }

        //[Fact]
        //public async Task Create_Post_ValidData_RedirectsToIndex()
        //{
        //    ClearDatabase();
        //    var controller = new CategoryController(_dbContext);
        //    var model = new CategoryDetail
        //    {
        //        NameCategory = "New Category",
        //        Description = "New Description",
        //        Avartar = "new.jpg" // Gán trực tiếp Avartar
        //    };

        //    var result = await controller.Create(model, null) as RedirectToActionResult;

        //    Assert.NotNull(result);
        //    Assert.Equal("Index", result.ActionName);
        //    Assert.Equal("Category", result.ControllerName);
        //    Assert.Single(_dbContext.Categories);
        //    var category = _dbContext.Categories.First();
        //    Assert.Equal("New Category", category.NameCategory);
        //    Assert.Equal("new.jpg", category.Avatar);
        //    Assert.Equal("Active", category.Status);
        //}

        [Fact]
        public void Edit_Get_ValidId_ReturnsViewWithModel()
        {
            ClearDatabase();
            _dbContext.Categories.Add(new Categories
            {
                Id = 1,
                NameCategory = "Test Category",
                Description = "Test Description",
                Avatar = "test.jpg",
                Status = "Active",
                CreatedAt = DateTime.Now
            });
            _dbContext.SaveChanges();
            var controller = new CategoryController(_dbContext);

            var result = controller.Edit(1) as ViewResult;
            var model = result.Model as CategoryDetail;

            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(1, model.Id);
            Assert.Equal("Test Category", model.NameCategory);
        }

        //[Fact]
        //public async Task Edit_Post_ValidData_RedirectsToIndex()
        //{
        //    ClearDatabase();
        //    _dbContext.Categories.Add(new Categories
        //    {
        //        Id = 1,
        //        NameCategory = "Old Category",
        //        Description = "Old Description",
        //        Avatar = "old.jpg",
        //        Status = "Active",
        //        CreatedAt = DateTime.Now
        //    });
        //    _dbContext.SaveChanges();
        //    var controller = new CategoryController(_dbContext);
        //    var model = new CategoryDetail
        //    {
        //        Id = 1,
        //        NameCategory = "Updated Category",
        //        Description = "Updated Description",
        //        Status = "Active",
        //        Avartar = "updated.jpg" // Gán trực tiếp Avartar
        //    };

        //    var result = await controller.Edit(model, null) as RedirectToActionResult;

        //    Assert.NotNull(result);
        //    Assert.Equal("Index", result.ActionName);
        //    var category = _dbContext.Categories.First();
        //    Assert.Equal("Updated Category", category.NameCategory);
        //    Assert.Equal("Updated Description", category.Description);
        //    Assert.Equal("updated.jpg", category.Avatar);
        //}

        [Fact]
        public async Task Delete_Post_ValidId_RemovesCategoryAndRedirects()
        {
            ClearDatabase();
            _dbContext.Categories.Add(new Categories
            {
                Id = 1,
                NameCategory = "Test Category",
                Description = "Test Description",
                Avatar = "test.jpg",
                Status = "Active",
                CreatedAt = DateTime.Now
            });
            _dbContext.SaveChanges();
            var controller = new CategoryController(_dbContext);

            var result = await controller.Delete(1) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Empty(_dbContext.Categories);
        }
    }
}