using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Helpers;
using System.Linq;

namespace CMS.Backend.Helpers
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Đảm bảo database đã được tạo
            context.Database.EnsureCreated();

            // 1. Seed dữ liệu bảng Users (Quản trị viên / Nhân viên) nếu trống
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User
                    {
                        Username = "admin",
                        PasswordHash = PasswordHelper.HashPassword("admin123"),
                        FullName = "Nguyễn Tuấn Tài (Admin)",
                        Role = "Admin"
                    },
                    new User
                    {
                        Username = "staff",
                        PasswordHash = PasswordHelper.HashPassword("staff123"),
                        FullName = "Nhân viên bán hàng (Staff)",
                        Role = "Staff"
                    }
                );
                context.SaveChanges();
            }

            // 1.5. Seed dữ liệu bảng Menus nếu trống
            if (!context.Menus.Any())
            {
                context.Menus.AddRange(
                    new Menu { Title = "Trang chủ", Url = "/", Order = 1, IsActive = true },
                    new Menu { Title = "Sản phẩm", Url = "/products", Order = 2, IsActive = true },
                    new Menu { Title = "Tin tức", Url = "/blog", Order = 3, IsActive = true }
                );
                context.SaveChanges();
            }

            // 1.7. Seed dữ liệu bảng Banners nếu trống
            if (!context.Banners.Any())
            {
                context.Banners.AddRange(
                    new Banner { Title = "Độc quyền Website", ImageUrl = "/uploads/banner1.webp", DisplayOrder = 1, IsVisible = true, TargetUrl = "/products" },
                    new Banner { Title = "Freeship đơn hàng", ImageUrl = "/uploads/banner2.webp", DisplayOrder = 2, IsVisible = true, TargetUrl = "/products" },
                    new Banner { Title = "Lego Playground 2026", ImageUrl = "/uploads/banner3.webp", DisplayOrder = 3, IsVisible = true, TargetUrl = "/products" },
                    new Banner { Title = "Children Day 2026", ImageUrl = "/uploads/banner4.webp", DisplayOrder = 4, IsVisible = true, TargetUrl = "/products" },
                    new Banner { Title = "Xoay nắn sáng tạo Tangle", ImageUrl = "/uploads/banner5.webp", DisplayOrder = 5, IsVisible = true, TargetUrl = "/products" },
                    new Banner { Title = "Art Puzzles", ImageUrl = "/uploads/banner6.webp", DisplayOrder = 6, IsVisible = true, TargetUrl = "/products" },
                    new Banner { Title = "Lego Preschool", ImageUrl = "/uploads/banner7.jpg", DisplayOrder = 7, IsVisible = true, TargetUrl = "/products" },
                    new Banner { Title = "Clever Hippo Collection", ImageUrl = "/uploads/banner8.jpg", DisplayOrder = 8, IsVisible = true, TargetUrl = "/products" }
                );
                context.SaveChanges();
            }
            else
            {
                var nullUrlBanners = context.Banners.Where(b => b.TargetUrl == null).ToList();
                if (nullUrlBanners.Any())
                {
                    foreach (var banner in nullUrlBanners)
                    {
                        banner.TargetUrl = "/products";
                    }
                    context.SaveChanges();
                }
            }

            /*
            // 2. Seed dữ liệu bảng CategoryProduct (Danh mục sản phẩm) nếu trống
            if (!context.CategoriesProducts.Any())
            {
                var categories = new CategoryProduct[]
                {
                    new CategoryProduct { Name = "Đồ chơi lắp ráp (LEGO)", Description = "Các bộ lắp ráp LEGO chính hãng kích thích trí sáng tạo và thông minh cho bé.", ImageUrl = "https://images.unsplash.com/photo-1587590227264-0ac64ce63ce8?w=500" },
                    new CategoryProduct { Name = "Búp bê & Thú bông", Description = "Thế giới búp bê Barbie đáng yêu và thú bông mềm mại, an toàn.", ImageUrl = "https://images.unsplash.com/photo-1559251606-c623743a6d76?w=500" },
                    new CategoryProduct { Name = "Xe mô hình & Điều khiển", Description = "Siêu xe Hot Wheels đúc nguyên khối, xe đua điều khiển từ xa tốc độ cao.", ImageUrl = "https://images.unsplash.com/photo-1581235720704-06d3acfcb36f?w=500" },
                    new CategoryProduct { Name = "Đồ chơi giáo dục & Sáng tạo", Description = "Đồ chơi phát triển kỹ năng toán học, hội họa, ngoại ngữ cho bé.", ImageUrl = "https://images.unsplash.com/photo-1566140967404-b8b393ed7866?w=500" }
                };

                context.CategoriesProducts.AddRange(categories);
                context.SaveChanges();

                // 3. Seed dữ liệu bảng Products (Sản phẩm) tương ứng với các danh mục trên
                var legoCat = context.CategoriesProducts.FirstOrDefault(c => c.Name == "Đồ chơi lắp ráp (LEGO)");
                var dollCat = context.CategoriesProducts.FirstOrDefault(c => c.Name == "Búp bê & Thú bông");
                var carCat = context.CategoriesProducts.FirstOrDefault(c => c.Name == "Xe mô hình & Điều khiển");
                var eduCat = context.CategoriesProducts.FirstOrDefault(c => c.Name == "Đồ chơi giáo dục & Sáng tạo");

                if (legoCat != null)
                {
                    context.Products.AddRange(
                         new Product
                         {
                             Name = "LEGO City Xe Cảnh Sát Tuần Tra 60239",
                             Description = "Mô hình xe tuần tra cảnh sát cực chất giúp bé thỏa thích đóng vai người hùng giữ gìn an ninh thành phố.",
                             Price = 249000,
                             StockQuantity = 50,
                             ImageUrl = "https://images.unsplash.com/photo-1587590227264-0ac64ce63ce8?w=500",
                             CategoryProductId = legoCat.Id
                         },
                         new Product
                         {
                             Name = "LEGO Ninjago Chiến Giáp Rồng Của Kai 71707",
                             Description = "Mô hình chiến giáp rồng dũng mãnh biến hình linh hoạt, khơi gợi trí tưởng tượng phong phú.",
                             Price = 599000,
                             StockQuantity = 30,
                             ImageUrl = "https://images.unsplash.com/photo-1566140967404-b8b393ed7866?w=500",
                             CategoryProductId = legoCat.Id
                         }
                    );
                }

                if (dollCat != null)
                {
                    context.Products.AddRange(
                         new Product
                         {
                             Name = "Búp bê Barbie Thời Trang Dạ Hội",
                             Description = "Búp bê Barbie chính hãng khoác lên mình bộ cánh lấp lánh kèm các phụ kiện vương miện, vòng cổ cực sang trọng.",
                             Price = 349000,
                             StockQuantity = 40,
                             ImageUrl = "https://images.unsplash.com/photo-1559251606-c623743a6d76?w=500",
                             CategoryProductId = dollCat.Id
                         },
                         new Product
                         {
                             Name = "Gấu Bông Teddy Brown Siêu Mịn 50cm",
                             Description = "Gấu Teddy bông nhập khẩu mềm mại, không xơ rụng lông, an toàn tuyệt đối cho làn da nhạy cảm của bé.",
                             Price = 199000,
                             StockQuantity = 60,
                             ImageUrl = "https://images.unsplash.com/photo-1559251606-c623743a6d76?w=500",
                             CategoryProductId = dollCat.Id
                         }
                    );
                }

                if (carCat != null)
                {
                    context.Products.AddRange(
                         new Product
                         {
                             Name = "Set 5 Xe Kim Loại Hot Wheels Siêu Tốc Độ",
                             Description = "Bộ 5 siêu xe đúc nguyên khối bằng kim loại chịu va đập mạnh, tỷ lệ 1:64 sắc nét chuẩn quốc tế.",
                             Price = 185000,
                             StockQuantity = 100,
                             ImageUrl = "https://images.unsplash.com/photo-1581235720704-06d3acfcb36f?w=500",
                             CategoryProductId = carCat.Id
                         },
                         new Product
                         {
                             Name = "Xe Đua Địa Hình Điều Khiển Từ Xa 4WD",
                             Description = "Siêu xe leo núi điều khiển từ xa, lò xo giảm xóc cực đỉnh, tần số 2.4GHz không trùng sóng.",
                             Price = 450000,
                             StockQuantity = 25,
                             ImageUrl = "https://images.unsplash.com/photo-1581235720704-06d3acfcb36f?w=500",
                             CategoryProductId = carCat.Id
                         }
                    );
                }

                if (eduCat != null)
                {
                    context.Products.AddRange(
                         new Product
                         {
                             Name = "Bảng Gỗ Ghép Chữ Cái Và Số Đa Năng",
                             Description = "Sản phẩm giúp bé vừa học vừa chơi thông qua việc nhận diện màu sắc, chữ viết tiếng Việt và các con số cơ bản.",
                             Price = 120000,
                             StockQuantity = 80,
                             ImageUrl = "https://images.unsplash.com/photo-1566140967404-b8b393ed7866?w=500",
                             CategoryProductId = eduCat.Id
                         }
                    );
                }
                context.SaveChanges();
            }

            // 4. Seed dữ liệu bảng Customers (Khách hàng) nếu trống
            if (!context.Customers.Any())
            {
                context.Customers.AddRange(
                    new Customer
                    {
                        FullName = "Nguyễn Văn Khách",
                        Email = "customer@gmail.com",
                        Phone = "0987654321",
                        Address = "123 Đường Ba Tháng Hai, Quận 10, TP. Hồ Chí Minh",
                        Password = "123456" // Sẽ được tự động hash trong Program.cs
                    }
                );
                context.SaveChanges();
            }

            // 5. Seed dữ liệu bảng Categories (Danh mục bài viết) nếu trống
            if (!context.Categories.Any())
            {
                var postCategories = new Category[]
                {
                    new Category { Name = "Tin khuyến mãi", Description = "Cập nhật các chương trình ưu đãi, giảm giá và quà tặng hấp dẫn." },
                    new Category { Name = "Cẩm nang cho mẹ và bé", Description = "Bí quyết chọn đồ chơi an toàn và phương pháp giáo dục hiện đại." }
                };

                context.Categories.AddRange(postCategories);
                context.SaveChanges();

                // 6. Seed dữ liệu bài viết (Posts) nếu trống
                var promoCat = context.Categories.FirstOrDefault(c => c.Name == "Tin khuyến mãi");
                var tipCat = context.Categories.FirstOrDefault(c => c.Name == "Cẩm nang cho mẹ và bé");

                if (promoCat != null)
                {
                    context.Posts.Add(
                        new Post
                        {
                            Title = "Bùng Nổ Siêu Sale Quốc Tế Thiếu Nhi 1/6 - Giảm Đến 50%",
                            Content = "Nhân ngày 1/6, MyKingdom tưng bừng khuyến mãi các mặt hàng đồ chơi trẻ em yêu thích như LEGO, Búp bê Barbie, Hot Wheels với mức giảm khủng lên đến 50%. Tặng kèm bóng bay và sticker xinh xắn cho mọi đơn hàng tại hệ thống.",
                            ImageUrl = "https://images.unsplash.com/photo-1513151233558-d860c5398176?w=500",
                            CreatedDate = System.DateTime.Now,
                            CategoryId = promoCat.Id
                        }
                    );
                }

                if (tipCat != null)
                {
                    context.Posts.Add(
                        new Post
                        {
                            Title = "Cách Chọn Đồ Chơi Phù Hợp Cho Trẻ Từ 1 - 3 Tuổi",
                            Content = "Giai đoạn này bé cần phát triển các kỹ năng vận động tinh và tư duy hình học đơn giản. Bố mẹ nên chọn các loại đồ chơi hình khối bằng gỗ, sách vải thông minh hoặc đất nặn an toàn để kích thích trí tò mò của trẻ.",
                            ImageUrl = "https://images.unsplash.com/photo-1515488042361-404e9250afef?w=500",
                            CreatedDate = System.DateTime.Now,
                            CategoryId = tipCat.Id
                        }
                    );
                }
                context.SaveChanges();
            }
            */
        }
    }
}
