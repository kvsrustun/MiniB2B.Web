using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using MiniB2B.Web.Data;
using MiniB2B.Web.Models;

namespace MiniB2B.Web.Controllers
{
    public class AdminController : Controller
    {

        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = "/Admin" });
            }
            else if (role != "Admin")
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }

            base.OnActionExecuting(context);
        }

        public async Task<IActionResult> Index()
        {
            var criticalProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity <= p.CriticalStockThreshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);
            ViewBag.CriticalCount = criticalProducts.Count;

            return View(criticalProducts);
        }

        public async Task<IActionResult> Columns()
        {
            var columns = await _context.GridColumnConfigs
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return View(columns);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleColumn(int id)
        {
            var col = await _context.GridColumnConfigs.FindAsync(id);
            if (col != null)
            {
                col.IsVisible = !col.IsVisible;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Columns));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateColumn(int id, string headerTitle, int displayOrder, string alignment)
        {
            var col = await _context.GridColumnConfigs.FindAsync(id);
            if (col != null)
            {
                col.HeaderTitle = headerTitle;
                col.DisplayOrder = displayOrder;
                col.Alignment = alignment;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Columns));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock(int productId, int newStock, int newThreshold)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.StockQuantity = newStock;
                product.CriticalStockThreshold = newThreshold;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"'{product.Name}' stoku {newStock} olarak güncellendi.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Sipariş #{order.OrderNumber} durumu '{status}' olarak güncellendi.";
            }

            return RedirectToAction(nameof(Orders));
        }

        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name) || string.IsNullOrWhiteSpace(product.ProductCode))
            {
                ViewBag.Error = "Ürün Adı ve Ürün Kodu zorunludur!";
                ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View(product);
            }

            product.IsActive = true;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{product.Name}' başarıyla sisteme eklendi!";
            return RedirectToAction(nameof(Products));
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product)
        {
            var existing = await _context.Products.FindAsync(product.Id);
            if (existing == null) return NotFound();

            existing.Name = product.Name;
            existing.ProductCode = product.ProductCode;
            existing.Brand = product.Brand;
            existing.CategoryId = product.CategoryId;
            existing.Price = product.Price;
            existing.StockQuantity = product.StockQuantity;
            existing.CriticalStockThreshold = product.CriticalStockThreshold;
            existing.ImageUrl = product.ImageUrl;
            existing.IsActive = product.IsActive;

            existing.Description = product.Description;
            existing.ManufacturerCode = product.ManufacturerCode;
            existing.SpecialCode1 = product.SpecialCode1;
            existing.SpecialCode2 = product.SpecialCode2;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{existing.Name}' bilgileri güncellendi!";
            return RedirectToAction(nameof(Products));
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.OrderByDescending(u => u.Id).ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(User model)
        {
            var user = await _context.Users.FindAsync(model.Id);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Role = model.Role;
            user.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"'{user.Username}' kullanıcısının bilgileri güncellendi.";
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Banners()
        {
            var banners = await _context.Banners.OrderBy(b => b.DisplayOrder).ToListAsync();
            return View(banners);
        }

        [HttpGet]
        public IActionResult CreateBanner()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBanner(Banner banner)
        {
            if (string.IsNullOrWhiteSpace(banner.Title))
            {
                ViewBag.Error = "Kampanya başlığı zorunludur!";
                return View(banner);
            }

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Yeni kampanya ilanı başarıyla yayınlandı!";
            return RedirectToAction(nameof(Banners));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                _context.Banners.Remove(banner);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kampanya ilanı silindi.";
            }
            return RedirectToAction(nameof(Banners));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                banner.IsActive = !banner.IsActive;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"İlan durumu '{(banner.IsActive ? "Aktif" : "Pasif")}' yapıldı.";
            }
            return RedirectToAction(nameof(Banners));
        }

        [HttpGet]
        public async Task<IActionResult> EditBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();

            return View(banner);
        }

        [HttpPost]
        public async Task<IActionResult> EditBanner(Banner banner)
        {
            var existing = await _context.Banners.FindAsync(banner.Id);
            if (existing == null) return NotFound();

            if (string.IsNullOrWhiteSpace(banner.Title))
            {
                ViewBag.Error = "Kampanya başlığı zorunludur!";
                return View(banner);
            }

            existing.Title = banner.Title;
            existing.Description = banner.Description;
            existing.BadgeText = banner.BadgeText;
            existing.BadgeColor = banner.BadgeColor;
            existing.BackgroundGradient = banner.BackgroundGradient;
            existing.ImageUrl = banner.ImageUrl;
            existing.DisplayOrder = banner.DisplayOrder;
            existing.IsActive = banner.IsActive;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"'{existing.Title}' kampanyası başarıyla güncellendi!";
            return RedirectToAction(nameof(Banners));
        }
    }
}