using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniB2B.Web.Data;
using MiniB2B.Web.Models;

namespace MiniB2B.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // Ana Sayfa: Ürün Listesi, Arama, Filtreleme ve Dinamik Kolonlar
        public async Task<IActionResult> Index(string? search, string? brand, string? stockFilter, int? categoryId)
        {
            // 1. Dinamik Grid Kolon Konfigürasyonunu Çek (Görünür olanlar ve sıralı)
            var columns = await _context.GridColumnConfigs
                .Where(c => c.IsVisible)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            // 2. Ürün Sorgusu Hazırla
            var query = _context.Products.Include(p => p.Category).Where(p => p.IsActive);

            // Arama filtresi (Kod, Ad, Marka)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term)
                                      || p.ProductCode.ToLower().Contains(term)
                                      || p.Brand.ToLower().Contains(term));
            }

            // Marka filtresi
            if (!string.IsNullOrWhiteSpace(brand))
            {
                query = query.Where(p => p.Brand == brand);
            }

            // Stok Durumu Filtresi (Hepsi / Stokta Var / Kritik Stok / Stokta Yok)
            if (!string.IsNullOrWhiteSpace(stockFilter))
            {
                if (stockFilter == "in_stock")
                    query = query.Where(p => p.StockQuantity > p.CriticalStockThreshold);
                else if (stockFilter == "critical")
                    query = query.Where(p => p.StockQuantity > 0 && p.StockQuantity <= p.CriticalStockThreshold);
                else if (stockFilter == "out_of_stock")
                    query = query.Where(p => p.StockQuantity == 0);
            }

            // Kategori Filtresi
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query.ToListAsync();

            // Filtre dropdown'ları için listeler
            ViewBag.Brands = await _context.Products.Select(p => p.Brand).Distinct().ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Columns = columns;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentBrand = brand;
            ViewBag.CurrentStockFilter = stockFilter;
            ViewBag.CurrentCategoryId = categoryId;

            return View(products);
        }

        // Sepete Hızlı Ekleme (AJAX / Form)
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            if (quantity <= 0) quantity = 1;

            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.StockQuantity <= 0)
            {
                return Json(new { success = false, message = "Ürün tükendiği için sepete eklenemez!" });
            }

            if (quantity > product.StockQuantity)
            {
                return Json(new { success = false, message = $"Yetersiz stok! En fazla {product.StockQuantity} adet sipariş verebilirsiniz." });
            }

            // Basitlik ve demo için 1 numaralı kullanıcı sepeti (Bayi)
            int userId = HttpContext.Session.GetInt32("UserId") ?? 2; // Varsayılan: Bayi 1

            var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                if (existingItem.Quantity + quantity > product.StockQuantity)
                {
                    return Json(new { success = false, message = "Sepetinizdeki miktar ile birlikte mevcut stoku aşıyorsunuz!" });
                }
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"{product.Name} başarıyla sepete eklendi!" });
        }
    }
}