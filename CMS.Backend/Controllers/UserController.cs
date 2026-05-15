using CMS.Data.Entities;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Backend.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            var list = new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "admin_thai",
                    FullName = "Nguyễn Cao Thái",
                    Role = "Administrator"
                },
                new User
                {
                    Id = 2,
                    Username = "editor_01",
                    FullName = "Trần Văn Biên Tập",
                    Role = "Editor"
                },
                new User
                {
                    Id = 3,
                    Username = "author_minh",
                    FullName = "Lê Quang Minh",
                    Role = "Author"
                }
            };

            return View(list);
        }
    }
}
