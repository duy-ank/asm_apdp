using ASM_SIMS.Controllers;
using ASM_SIMS.DB;
using ASM_SIMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace SIMS.Tests.Controllers
{
    public class LoginControllerTests
    {
        private readonly SimsDataContext _dbContext;

        public LoginControllerTests()
        {
            var options = new DbContextOptionsBuilder<SimsDataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid().ToString())
                .Options;
            _dbContext = new SimsDataContext(options);
        }

        private void ClearDatabase()
        {
            if (_dbContext.Accounts.Any())
            {
                _dbContext.Accounts.RemoveRange(_dbContext.Accounts);
                _dbContext.SaveChanges();
            }
        }

        [Fact]
        public async Task Index_Post_ValidCredentials_RedirectsToDashboard()
        {
            // Arrange
            ClearDatabase();
            var account = new Account
            {
                Email = "admin@sims.com",
                Password = "admin123",
                RoleId = 1,
                Username = "admin",
                Phone = "1234567890",
                Address = "Admin Address",
                CreatedAt = DateTime.Now
            };
            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();

            var sessionMock = new Mock<ISession>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(x => x.Session).Returns(sessionMock.Object);
            var controller = new LoginController(_dbContext)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContextMock.Object
                }
            };
            var model = new LoginViewModel
            {
                Email = "admin@sims.com",
                Password = "admin123"
            };

            // Giả lập Set cho ISession
            sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) =>
                {
                    if (key == "UserId") Assert.Equal(Encoding.UTF8.GetString(value), account.Id.ToString());
                    if (key == "Username") Assert.Equal(Encoding.UTF8.GetString(value), "admin");
                });

            // Act
            var result = await controller.Index(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Dashboard", result.ControllerName);
            sessionMock.Verify(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()), Times.AtLeast(2)); // Kiểm tra Set được gọi ít nhất 2 lần
        }

        [Fact]
        public async Task Index_Post_InvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            ClearDatabase();
            _dbContext.Accounts.Add(new Account
            {
                
                Email = "admin@sims.com",
                Password = "admin123",
                RoleId = 1,
                Username = "admin",
                Phone = "1234567890",
                Address = "Admin Address",
                CreatedAt = DateTime.Now
            });
            await _dbContext.SaveChangesAsync();

            var controller = new LoginController(_dbContext);
            var model = new LoginViewModel
            {
                Email = "admin@sims.com",
                Password = "wrongpassword"
            };

            // Act
            var result = await controller.Index(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid email or password", result.ViewData["MessageLogin"]);
            Assert.Equal(model, result.Model);
        }

        [Fact]
        public void Logout_ClearsSession_RedirectsToLogin()
        {
            // Arrange
            var sessionMock = new Mock<ISession>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(x => x.Session).Returns(sessionMock.Object);
            var controller = new LoginController(_dbContext)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContextMock.Object
                }
            };

            // Act
            var result = controller.Logout() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Login", result.ControllerName);
            sessionMock.Verify(s => s.Clear(), Times.Once());
        }
    }
}