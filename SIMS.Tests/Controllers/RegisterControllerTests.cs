using ASM_SIMS.Controllers;
using ASM_SIMS.DB;
using ASM_SIMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace SIMS.Tests.Controllers
{
    public class RegisterControllerTests
    {
        private readonly SimsDataContext _dbContext;

        public RegisterControllerTests()
        {
            var options = new DbContextOptionsBuilder<SimsDataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid().ToString())
                .Options;
            _dbContext = new SimsDataContext(options);
        }

        private void ClearDatabase()
        {
            if (_dbContext.Accounts != null && _dbContext.Accounts.Any())
            {
                _dbContext.Accounts.RemoveRange(_dbContext.Accounts);
                _dbContext.SaveChanges();
            }
        }

        [Fact]
        public async Task Index_Post_ValidData_RedirectsToLogin()
        {
            // Arrange
            ClearDatabase();
            Assert.NotNull(_dbContext); // Debug: Kiểm tra _dbContext
            Assert.NotNull(_dbContext.Accounts); // Debug: Kiểm tra Accounts
            var controller = new RegisterController(_dbContext);
            var model = new RegisterViewModel
            {
                Username = "newuser",
                Email = "newuser@sims.com",
                Password = "password123",
                ConfirmPassword = "password123",
                Phone = "0987654321",
                Address = "User Address",
                Role = "Student"
            };

            // Act
            var result = await controller.Index(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Login", result.ControllerName);
            Assert.Single(_dbContext.Accounts);
            var account = _dbContext.Accounts.First();
            Assert.Equal("newuser", account.Username);
            Assert.Equal(3, account.RoleId); // Student
        }

        [Fact]
        public async Task Index_Post_DuplicateEmail_ReturnsViewWithError()
        {
            // Arrange
            ClearDatabase();
            _dbContext.Accounts.Add(new Account
            {
                Email = "test@sims.com",
                Password = "test",
                RoleId = 1,
                Username = "testuser",
                Phone = "1234567890",
                Address = "Test Address"
            });
            await _dbContext.SaveChangesAsync();
            var controller = new RegisterController(_dbContext);
            var model = new RegisterViewModel
            {
                Username = "newuser",
                Email = "test@sims.com",
                Password = "password123",
                ConfirmPassword = "password123",
                Phone = "0987654321",
                Address = "New Address",
                Role = "Student"
            };

            // Act
            var result = await controller.Index(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Equal("This email is already registered.", controller.ModelState["Email"].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task Index_Post_DuplicatePhone_ReturnsViewWithError()
        {
            // Arrange
            ClearDatabase();
            _dbContext.Accounts.Add(new Account
            {
                Email = "test@sims.com",
                Phone = "0987654321",
                Password = "test",
                RoleId = 1,
                Username = "testuser",
                Address = "Test Address"
            });
            await _dbContext.SaveChangesAsync();
            var controller = new RegisterController(_dbContext);
            var model = new RegisterViewModel
            {
                Username = "newuser",
                Email = "newuser@sims.com",
                Password = "password123",
                ConfirmPassword = "password123",
                Phone = "0987654321",
                Address = "New Address",
                Role = "Student"
            };

            // Act
            var result = await controller.Index(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Equal("This phone number is already registered.", controller.ModelState["Phone"].Errors[0].ErrorMessage);
        }
    }
}