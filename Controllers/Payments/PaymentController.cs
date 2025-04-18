using FliesProject.Data;
using FliesProject.Models.Entities;
using FliesProject.VNpayLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using System.Text;


namespace YourProject.Controllers
{
    [Authorize] // Đảm bảo người dùng đã đăng nhập
    public class PaymentController : Controller
    {
        private readonly FiliesContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(FiliesContext context, IConfiguration configuration, ILogger<PaymentController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // Hiển thị form chọn số tiền nạp
        public IActionResult Deposit()
        {
            return View();
        }

        // Xử lý khi người dùng đã chọn số tiền và muốn thanh toán
        [HttpPost]
        public async Task<IActionResult> ProcessDeposit(decimal amount)
        {
            if (amount <= 0)
            {
                TempData["Error"] = "Số tiền phải lớn hơn 0";
                return RedirectToAction("Deposit");
            }

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Tạo đơn hàng trong database
            var order = new PaymentOrder
            {
                OrderId = DateTime.Now.Ticks.ToString(),
                UserId = userId,
                Amount = amount,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.PaymentOrders.Add(order);
            await _context.SaveChangesAsync();

            // Tạo yêu cầu thanh toán VNPay
            var vnpayUrl = CreateVnPayRequest(order.OrderId, amount);

            return Redirect(vnpayUrl);
        }

        // Xử lý khi VNPay gọi về callback URL
        public async Task<IActionResult> PaymentCallback()
        {
            var vnpayData = HttpContext.Request.Query;
            string vnp_ResponseCode = vnpayData["vnp_ResponseCode"];
            string orderId = vnpayData["vnp_TxnRef"];
            string transactionNo = vnpayData["vnp_TransactionNo"];
            string payDate = vnpayData["vnp_PayDate"];
            string vnp_SecureHash = vnpayData["vnp_SecureHash"];

            // Xác thực chữ ký từ VNPay (giản lược trong ví dụ này)
            bool isValidSignature = ValidateVnPayCallback(vnpayData);

            if (isValidSignature && vnp_ResponseCode == "00")
            {
                // Tìm thông tin đơn hàng
                var order = await _context.PaymentOrders.FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order != null && order.Status == "Pending")
                {
                    // Cập nhật thông tin đơn hàng
                    order.Status = "Completed";
                    order.TransactionRef = transactionNo;
                    order.UpdatedAt = DateTime.Now;

                    // Cập nhật số dư người dùng
                    var user = await _context.Users.FindAsync(order.UserId);
                    if (user != null)
                    {
                        user.Balance = (user.Balance ?? 0) + order.Amount;
                        user.UpdatedAt = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();

                    ViewBag.Success = true;
                    ViewBag.Message = "Nạp tiền thành công. Số dư đã được cập nhật.";
                    ViewBag.Amount = order.Amount;
                    return View("PaymentResult");
                }
            }
            else
            {
                // Thanh toán thất bại
                ViewBag.Success = false;
                ViewBag.Message = "Thanh toán không thành công. Vui lòng thử lại sau.";
                return View("PaymentResult");
            }

            return RedirectToAction("Deposit");
        }

        // Tạo URL để chuyển hướng đến cổng thanh toán VNPay
        private string CreateVnPayRequest(string orderId, decimal amount)
        {
            // Đọc thông tin cấu hình VNPay từ appsettings.json
            string vnp_Returnurl = _configuration["VnPay:ReturnUrl"];
            string vnp_Url = _configuration["VnPay:PaymentUrl"];
            string vnp_TmnCode = _configuration["VnPay:TmnCode"];
            string vnp_HashSecret = _configuration["VnPay:HashSecret"];
            string vnp_Version = _configuration["VnPay:Version"];
            string vnp_Command = "pay";
            string vnp_OrderInfo = "Nạp tiền vào tài khoản";
            string vnp_OrderType = "billpayment";
            string vnp_Locale = "vn";
            string vnp_CurrCode = "VND";
            string vnp_IpAddr = HttpContext.Connection.RemoteIpAddress.ToString();

            // Chuyển đổi số tiền sang định dạng VNPay (x100)
            long amountInVND = (long)(amount * 100);

            // Tạo các tham số cho URL
            var vnpParams = new Dictionary<string, string>
            {
                { "vnp_Version", vnp_Version },
                { "vnp_Command", vnp_Command },
                { "vnp_TmnCode", vnp_TmnCode },
                { "vnp_Amount", amountInVND.ToString() },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", vnp_CurrCode },
                { "vnp_IpAddr", vnp_IpAddr },
                { "vnp_Locale", vnp_Locale },
                { "vnp_OrderInfo", vnp_OrderInfo },
                { "vnp_OrderType", vnp_OrderType },
                { "vnp_ReturnUrl", vnp_Returnurl },
                { "vnp_TxnRef", orderId }
            };

            // Sắp xếp các tham số theo thứ tự tăng dần theo tên
            vnpParams = vnpParams
                .OrderBy(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Tạo chuỗi ký tự để tạo chữ ký
            StringBuilder hashData = new StringBuilder();
            foreach (var kvp in vnpParams)
            {
                hashData.Append(WebUtility.UrlEncode(kvp.Key) + "=" + WebUtility.UrlEncode(kvp.Value) + "&");
            }

            string queryString = hashData.ToString();
            queryString = queryString.Remove(queryString.Length - 1, 1); // Xóa dấu & cuối cùng

            // Tạo chữ ký HMAC-SHA512
            string vnp_SecureHash = VnPayLibrary.HmacSHA512(vnp_HashSecret, queryString);
            queryString += "&vnp_SecureHash=" + vnp_SecureHash;

            string paymentUrl = vnp_Url + "?" + queryString;
            return paymentUrl;
        }

        // Xác thực callback từ VNPay
        private bool ValidateVnPayCallback(IQueryCollection collection)
        {
            string vnp_HashSecret = _configuration["VnPay:HashSecret"];

            // Lấy danh sách tham số
            var vnpParams = new Dictionary<string, string>();
            foreach (var key in collection.Keys)
            {
                // Bỏ qua tham số chữ ký
                if (!key.StartsWith("vnp_SecureHash"))
                {
                    // Lấy giá trị đầu tiên của mỗi key
                    string value = collection[key].FirstOrDefault() ?? string.Empty;
                    vnpParams.Add(key, value);
                }
            }

            // Sắp xếp các tham số theo thứ tự tăng dần theo tên
            vnpParams = vnpParams
                .OrderBy(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Tạo chuỗi ký tự để kiểm tra chữ ký
            StringBuilder hashData = new StringBuilder();
            foreach (var kvp in vnpParams)
            {
                hashData.Append(WebUtility.UrlEncode(kvp.Key) + "=" + WebUtility.UrlEncode(kvp.Value) + "&");
            }

            string queryString = hashData.ToString();
            queryString = queryString.Remove(queryString.Length - 1, 1);

            string vnp_SecureHash = collection["vnp_SecureHash"].FirstOrDefault() ?? string.Empty;
            string calculatedHash = VnPayLibrary.HmacSHA512(vnp_HashSecret, queryString);

            return calculatedHash.Equals(vnp_SecureHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}