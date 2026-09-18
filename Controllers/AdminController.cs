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

        // 🔒 GÜVENLİK KİLİDİ: Sadece 'Admin' rolündeki kullanıcılar girebilir!
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
            {
                // Giriş yapmamışsa doğrudan Login sayfasına yönlendir
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = "/Admin" });
            }
            else if (role != "Admin")
            {
                // Giriş yapmış ama rolü Admin değilse (örn. Bayi ise) erişim engeli ver
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }

            base.OnActionExecuting(context);
        }

        // 1. Admin Ana Sayfa / Dashboard (Kritik Stok Uyarısı ve İstatistikler)
        public async Task<IActionResult> Index()
        {
            // Kritik ve tükenen ürünler (Ödev İsteri)
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

        // 2. Dinamik Grid Kolon Konfigürasyonu Ekranı (Ödev Sayfa 4-5)
        public async Task<IActionResult> Columns()
        {
            var columns = await _context.GridColumnConfigs
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return View(columns);
        }

        // Kolon Görünürlüğünü Değiştir (Aç / Kapat)
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

        // Kolon Sırasını ve Başlığını Güncelle
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

        // 3. Hızlı Stok Güncelleme (Admin panelinden tek tıkla)
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

        // 4. Siparişler Listesi (Admin)
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}