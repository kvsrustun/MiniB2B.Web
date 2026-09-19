using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniB2B.Web.Data;
using MiniB2B.Web.Models;

namespace MiniB2B.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Lütfen kullanıcı adı ve şifrenizi giriniz!";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("FullName", $"{user.FirstName} {user.LastName}".Trim());

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model, string Password, string ConfirmPassword)
        {
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(model.Email))
            {
                ViewBag.Error = "Kullanıcı adı, e-posta ve şifre alanları zorunludur!";
                return View(model);
            }

            if (Password != ConfirmPassword)
            {
                ViewBag.Error = "Girdiğiniz şifreler birbiriyle eşleşmiyor! Lütfen şifrenizi tekrar kontrol edin.";
                return View(model);
            }

            if (Password.Length < 6)
            {
                ViewBag.Error = "Şifreniz en az 6 karakter uzunluğunda olmalıdır!";
                return View(model);
            }

            bool exists = await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email);
            if (exists)
            {
                ViewBag.Error = "Bu kullanıcı adı veya e-posta adresi sistemde zaten kayıtlı!";
                return View(model);
            }

            model.Role = "Dealer";
            model.IsActive = true;

            model.PasswordHash = HashPassword(Password);

            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("UserId", model.Id);
            HttpContext.Session.SetString("Username", model.Username);
            HttpContext.Session.SetString("Role", model.Role);
            HttpContext.Session.SetString("FullName", $"{model.FirstName} {model.LastName}".Trim());

            TempData["Success"] = "Bayilik kaydınız başarıyla tamamlandı, hoş geldiniz!";
            return RedirectToAction("Index", "Home");
        }

        private static string HashPassword(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower(); 
        }

        private static bool VerifyPassword(string enteredPassword, string? storedPasswordHash)
        {
            if (string.IsNullOrEmpty(enteredPassword) || string.IsNullOrEmpty(storedPasswordHash))
                return false;

            var enteredHash = HashPassword(enteredPassword);

            return enteredHash.Equals(storedPasswordHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}