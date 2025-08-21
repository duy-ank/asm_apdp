using Microsoft.AspNetCore.Authorization;

namespace ASM_SIMS.Services
{
    public interface IAuthorizationService
    {
        // kiểm tra xem userRole có được phép thực hiện hành động trên controller hay không.
        bool CanPerformAction(string userRole, string controller, string action);
    }

    public class AuthorizationService : IAuthorizationService
    {
        private readonly Dictionary<string, List<(string Controller, string Action)>> _rolePermissions;

        public AuthorizationService()
        {
            // Khởi tạo quyền cho từng vai trò
            _rolePermissions = new Dictionary<string, List<(string Controller, string Action)>>
            {
                ["Admin"] = new List<(string, string)>
                {
                    ("Category", "Index"), ("Category", "Create"), ("Category", "Edit"), ("Category", "Delete"),
                    ("Course", "Index"), ("Course", "Create"), ("Course", "Edit"), ("Course", "Delete"),
                    ("ClassRoom", "Index"), ("ClassRoom", "Create"), ("ClassRoom", "Edit"), ("ClassRoom", "Delete"),
                    ("ClassRoom", "AddStudentToClass"), ("ClassRoom", "Details"),
                    ("Student", "Index"), ("Student", "Create"), ("Student", "Edit"), ("Student", "Delete"),
                    ("Teacher", "Index"), ("Teacher", "Create"), ("Teacher", "Edit"), ("Teacher", "Delete"),
                    ("Register", "Index") // Admin có quyền đăng ký tài khoản
                },
                ["Student"] = new List<(string, string)>
                {
                    ("ClassRoom", "Index"), ("ClassRoom", "Details"),
                    ("Student", "Index"),
                    ("Course", "Index")
                },
                ["Teacher"] = new List<(string, string)>
                {
                    ("Course", "Index"),
                    ("Student", "Index"),
                    ("Teacher", "Index"),
                    ("ClassRoom", "Index"), ("ClassRoom", "Details")
                }
            };
        }

        // kiểm tra quyền truy cập.
        public bool CanPerformAction(string userRole, string controller, string action)
        {
            // Kiểm tra xem vai trò có trong _rolePermissions 
            if (!_rolePermissions.ContainsKey(userRole)) return false;
            return _rolePermissions[userRole].Any(p => p.Controller == controller && p.Action == action);
        }
    }
}
