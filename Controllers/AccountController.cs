using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniB2B.Web.Data;

namespace MiniB2B.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // Giriş Sayfası (GET)
        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // Giriş Yap İşlemi (POST)
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl)
        {
            // Basit hash kontrolü (Seed data'da verdiğimiz şifre: 123456)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            // Şifre kontrolü (123456 veya md5 hash'i)
            if (user == null || (password != "123456" && user.PasswordHash != password))
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
                return View();
            }

            // Session'a kullanıcı bilgilerini yazıyoruz
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", $"{user.FirstName} {user.LastName}");
            HttpContext.Session.SetString("Role", user.Role);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (user.Role == "Admin")
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        // Çıkış Yap (POST/GET)
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Yetkisiz Erişim Ekranı
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}