using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AddressesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/addresses/customer/{customerId}
        /// Lấy danh sách địa chỉ của khách hàng
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            try
            {
                var addresses = await _context.CustomerAddresses
                    .Where(a => a.CustomerId == customerId)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return Ok(addresses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi tải danh sách địa chỉ", detail = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/addresses
        /// Thêm mới địa chỉ nhận hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAddressInputDTO input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                var customer = await _context.Customers.FindAsync(input.CustomerId);
                if (customer == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin tài khoản" });
                }

                // Kiểm tra xem khách hàng đã có địa chỉ nào chưa
                var hasAnyAddress = await _context.CustomerAddresses.AnyAsync(a => a.CustomerId == input.CustomerId);
                
                // Nếu là địa chỉ đầu tiên hoặc được yêu cầu đặt làm mặc định
                bool shouldBeDefault = !hasAnyAddress || input.IsDefault;

                if (shouldBeDefault)
                {
                    // Bỏ mặc định ở các địa chỉ cũ
                    var defaultAddresses = await _context.CustomerAddresses
                        .Where(a => a.CustomerId == input.CustomerId && a.IsDefault)
                        .ToListAsync();
                    foreach (var addr in defaultAddresses)
                    {
                        addr.IsDefault = false;
                        addr.UpdatedAt = DateTime.Now;
                    }
                }

                var newAddress = new CustomerAddress
                {
                    CustomerId = input.CustomerId,
                    RecipientName = input.RecipientName.Trim(),
                    PhoneNumber = input.PhoneNumber.Trim(),
                    ProvinceName = input.ProvinceName.Trim(),
                    DistrictName = input.DistrictName.Trim(),
                    WardName = input.WardName.Trim(),
                    AddressLine = input.AddressLine.Trim(),
                    AddressType = input.AddressType.Trim(),
                    IsDefault = shouldBeDefault,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.CustomerAddresses.Add(newAddress);
                await _context.SaveChangesAsync();

                // Đồng bộ địa chỉ mặc định mới sang bảng Customers nếu đây là mặc định
                if (shouldBeDefault)
                {
                    string fullAddressStr = $"{newAddress.AddressLine}, {newAddress.WardName}, {newAddress.DistrictName}, {newAddress.ProvinceName}";
                    customer.Address = fullAddressStr;
                    _context.Customers.Update(customer);
                    await _context.SaveChangesAsync();
                }

                return StatusCode(201, newAddress);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi thêm địa chỉ mới", detail = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/addresses/{id}
        /// Cập nhật địa chỉ nhận hàng
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAddressInputDTO input)
        {
            if (input == null || id != input.Id)
            {
                return BadRequest(new { message = "Dữ liệu yêu cầu không hợp lệ" });
            }

            try
            {
                var address = await _context.CustomerAddresses.FindAsync(id);
                if (address == null)
                {
                    return NotFound(new { message = "Không tìm thấy địa chỉ này" });
                }

                // Bảo mật: kiểm tra quyền sở hữu
                if (address.CustomerId != input.CustomerId)
                {
                    return StatusCode(403, new { message = "Bạn không có quyền sửa địa chỉ này!" });
                }

                var customer = await _context.Customers.FindAsync(input.CustomerId);
                if (customer == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin tài khoản" });
                }

                if (input.IsDefault && !address.IsDefault)
                {
                    // Bỏ mặc định ở các địa chỉ cũ
                    var defaultAddresses = await _context.CustomerAddresses
                        .Where(a => a.CustomerId == input.CustomerId && a.IsDefault && a.Id != id)
                        .ToListAsync();
                    foreach (var addr in defaultAddresses)
                    {
                        addr.IsDefault = false;
                        addr.UpdatedAt = DateTime.Now;
                    }
                }

                address.RecipientName = input.RecipientName.Trim();
                address.PhoneNumber = input.PhoneNumber.Trim();
                address.ProvinceName = input.ProvinceName.Trim();
                address.DistrictName = input.DistrictName.Trim();
                address.WardName = input.WardName.Trim();
                address.AddressLine = input.AddressLine.Trim();
                address.AddressType = input.AddressType.Trim();
                address.IsDefault = input.IsDefault || address.IsDefault; // Không cho phép bỏ mặc định nếu đang là mặc định duy nhất
                address.UpdatedAt = DateTime.Now;

                _context.CustomerAddresses.Update(address);
                await _context.SaveChangesAsync();

                // Đồng bộ sang Customers.Address nếu là địa chỉ mặc định
                if (address.IsDefault)
                {
                    string fullAddressStr = $"{address.AddressLine}, {address.WardName}, {address.DistrictName}, {address.ProvinceName}";
                    customer.Address = fullAddressStr;
                    _context.Customers.Update(customer);
                    await _context.SaveChangesAsync();
                }

                return Ok(address);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi cập nhật địa chỉ", detail = ex.Message });
            }
        }

        /// <summary>
        /// DELETE: api/addresses/{id}
        /// Xóa địa chỉ nhận hàng
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] int customerId)
        {
            if (customerId <= 0)
            {
                return BadRequest(new { message = "Mã tài khoản khách hàng không hợp lệ" });
            }

            try
            {
                var address = await _context.CustomerAddresses.FindAsync(id);
                if (address == null)
                {
                    return NotFound(new { message = "Không tìm thấy địa chỉ cần xóa" });
                }

                // Bảo mật: kiểm tra quyền sở hữu
                if (address.CustomerId != customerId)
                {
                    return StatusCode(403, new { message = "Bạn không có quyền xóa địa chỉ này!" });
                }

                bool wasDefault = address.IsDefault;

                _context.CustomerAddresses.Remove(address);
                await _context.SaveChangesAsync();

                // Nếu xóa địa chỉ mặc định, tự động chuyển mặc định sang địa chỉ khác (nếu có)
                if (wasDefault)
                {
                    var nextAddress = await _context.CustomerAddresses
                        .Where(a => a.CustomerId == customerId)
                        .OrderByDescending(a => a.CreatedAt)
                        .FirstOrDefaultAsync();

                    var customer = await _context.Customers.FindAsync(customerId);

                    if (nextAddress != null && customer != null)
                    {
                        nextAddress.IsDefault = true;
                        nextAddress.UpdatedAt = DateTime.Now;
                        _context.CustomerAddresses.Update(nextAddress);

                        string fullAddressStr = $"{nextAddress.AddressLine}, {nextAddress.WardName}, {nextAddress.DistrictName}, {nextAddress.ProvinceName}";
                        customer.Address = fullAddressStr;
                        _context.Customers.Update(customer);
                        await _context.SaveChangesAsync();
                    }
                    else if (customer != null)
                    {
                        // Không còn địa chỉ nào khác
                        customer.Address = null;
                        _context.Customers.Update(customer);
                        await _context.SaveChangesAsync();
                    }
                }

                return Ok(new { message = "Xóa địa chỉ thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi xóa địa chỉ", detail = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/addresses/{id}/default
        /// Đặt một địa chỉ làm mặc định
        /// </summary>
        [HttpPut("{id}/default")]
        public async Task<IActionResult> SetDefault(int id, [FromQuery] int customerId)
        {
            if (customerId <= 0)
            {
                return BadRequest(new { message = "Mã tài khoản khách hàng không hợp lệ" });
            }

            try
            {
                var address = await _context.CustomerAddresses.FindAsync(id);
                if (address == null)
                {
                    return NotFound(new { message = "Không tìm thấy địa chỉ này" });
                }

                // Bảo mật: kiểm tra quyền sở hữu
                if (address.CustomerId != customerId)
                {
                    return StatusCode(403, new { message = "Bạn không có quyền thao tác trên địa chỉ này!" });
                }

                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    return NotFound(new { message = "Không tìm thấy tài khoản tương ứng" });
                }

                // Bỏ mặc định ở các địa chỉ cũ
                var otherAddresses = await _context.CustomerAddresses
                    .Where(a => a.CustomerId == customerId && a.Id != id && a.IsDefault)
                    .ToListAsync();
                foreach (var addr in otherAddresses)
                {
                    addr.IsDefault = false;
                    addr.UpdatedAt = DateTime.Now;
                    _context.CustomerAddresses.Update(addr);
                }

                // Đặt địa chỉ này làm mặc định
                address.IsDefault = true;
                address.UpdatedAt = DateTime.Now;
                _context.CustomerAddresses.Update(address);

                // Đồng bộ địa chỉ mặc định sang bảng Customers
                string fullAddressStr = $"{address.AddressLine}, {address.WardName}, {address.DistrictName}, {address.ProvinceName}";
                customer.Address = fullAddressStr;
                _context.Customers.Update(customer);

                await _context.SaveChangesAsync();

                return Ok(new { message = "Đặt địa chỉ mặc định thành công!", address });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi thiết lập địa chỉ mặc định", detail = ex.Message });
            }
        }
    }

    public class CreateAddressInputDTO
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RecipientName { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProvinceName { get; set; }

        [Required]
        [MaxLength(100)]
        public string DistrictName { get; set; }

        [Required]
        [MaxLength(100)]
        public string WardName { get; set; }

        [Required]
        [MaxLength(255)]
        public string AddressLine { get; set; }

        [Required]
        [MaxLength(50)]
        public string AddressType { get; set; }

        public bool IsDefault { get; set; }
    }

    public class UpdateAddressInputDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RecipientName { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProvinceName { get; set; }

        [Required]
        [MaxLength(100)]
        public string DistrictName { get; set; }

        [Required]
        [MaxLength(100)]
        public string WardName { get; set; }

        [Required]
        [MaxLength(255)]
        public string AddressLine { get; set; }

        [Required]
        [MaxLength(50)]
        public string AddressType { get; set; }

        public bool IsDefault { get; set; }
    }
}
