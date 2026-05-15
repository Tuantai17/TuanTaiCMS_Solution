using Microsoft.AspNetCore.Mvc;
using CMS.Data.Entities;
using System.Collections.Generic;

namespace CMS.Backend.Controllers
{
    public class PostController : Controller
    {
        public IActionResult Index()
        {
            var list = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    Title = "Lộ trình học ASP.NET",
                    Content = "Tìm hiểu các bước học ASP.NET Core MVC từ cơ bản đến nâng cao.",
                    ImageUrl = "https://via.placeholder.com/300x180?text=ASP.NET"
                },
                new Post
                {
                    Id = 2,
                    Title = "Cài đặt ReactJS",
                    Content = "Hướng dẫn chuẩn bị môi trường và tạo ứng dụng ReactJS đầu tiên.",
                    ImageUrl = "https://via.placeholder.com/300x180?text=ReactJS"
                }
            };

            return View(list);
        }
    }
}