using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ASM_SIMS.Filters
{
    public class RoleAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string _controller;
        private readonly string _action;

        public RoleAuthorizeAttribute(string controller, string action)
        {
            // Khởi tạo controller và action mặc định.
            // Cho phép chỉ định controller và action khi sử dụng attribute.
            _controller = controller;
            _action = action;
        }


        // Kiểm tra xem người dùng có quyền thực hiện action hay không
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Sử dụng IAuthorizationService để kiểm tra quyền dựa trên vai trò, controller, và action
            var authService = context.HttpContext.RequestServices.GetService<ASM_SIMS.Services.IAuthorizationService>();
            // Xác định vai trò của người dùng hiện tại (Admin, Student, Teacher, hoặc null nếu chưa đăng nhập)
            var role = context.HttpContext.Session.GetString("UserRole");

            /* Kiểm tra quyền truy cập:
            Nếu role null (chưa đăng nhập) || Nếu authService xác định role không có quyền thực hiện (_controller, _action)
             thì chuyển hướng người dùng đến trang đăng nhập.
            */
            if (string.IsNullOrEmpty(role) || !authService.CanPerformAction(role, _controller, _action))
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
            }

            base.OnActionExecuting(context);
        }
    }
}