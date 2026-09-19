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

        public async Task<IActionResult> Index(string? search, string? brand, string? stockFilter, int? categoryId)
        {
            ViewBag.Banners = await _context.Banners.Where(b => b.IsActive).OrderBy(b => b.DisplayOrder).ToListAsync();

            var columns = await _context.GridColumnConfigs
                .Where(c => c.IsVisible)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var query = _context.Products.Include(p => p.Category).Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.ProductCode.ToLower().Contains(search) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(search)) ||
                    (p.Description != null && p.Description.ToLower().Contains(search)) ||
                    (p.ManufacturerCode != null && p.ManufacturerCode.ToLower().Contains(search)) ||
                    (p.SpecialCode1 != null && p.SpecialCode1.ToLower().Contains(search)) ||
                    (p.SpecialCode2 != null && p.SpecialCode2.ToLower().Contains(search))
                );
            }

            if (!string.IsNullOrWhiteSpace(brand))
            {
                query = query.Where(p => p.Brand == brand);
            }

            if (!string.IsNullOrWhiteSpace(stockFilter))
            {
                if (stockFilter == "in_stock")
                    query = query.Where(p => p.StockQuantity > p.CriticalStockThreshold);
                else if (stockFilter == "critical")
                    query = query.Where(p => p.StockQuantity > 0 && p.StockQuantity <= p.CriticalStockThreshold);
                else if (stockFilter == "out_of_stock")
                    query = query.Where(p => p.StockQuantity == 0);
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query.ToListAsync();

            ViewBag.Brands = await _context.Products.Select(p => p.Brand).Distinct().ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Columns = columns;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentBrand = brand;
            ViewBag.CurrentStockFilter = stockFilter;
            ViewBag.CurrentCategoryId = categoryId;

            return View(products);
        }

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

            int userId = HttpContext.Session.GetInt32("UserId") ?? 2;

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
            
            int cartCount = cart.Items.Count;
            HttpContext.Session.SetInt32("CartCount", cartCount);

            return Json(new { success = true, message = $"{product.Name} başarıyla sepete eklendi!", cartCount = cartCount });
        }
    }
}