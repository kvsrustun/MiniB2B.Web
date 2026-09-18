using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniB2B.Web.Data;
using MiniB2B.Web.Models;

namespace MiniB2B.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Sepet Sayfası
        public async Task<IActionResult> Index()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 2; // Varsayılan: Bayi 1

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return View(cart);
        }

        // 2. Sepetten Ürün Silme
        [HttpPost]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var item = await _context.CartItems.FindAsync(itemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 3. Sepette Miktar Güncelleme
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int itemId, int quantity)
        {
            var item = await _context.CartItems.Include(i => i.Product).FirstOrDefaultAsync(i => i.Id == itemId);
            if (item != null && quantity > 0)
            {
                if (quantity <= item.Product!.StockQuantity)
                {
                    item.Quantity = quantity;
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // 4. Siparişi Tamamla & Onayla (Ödev Kuralı: Snapshot Fiyat ve Stok Düşümü)
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 2;

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Sepetiniz boş!";
                return RedirectToAction(nameof(Index));
            }

            // Stok kontrolü yapalım
            foreach (var item in cart.Items)
            {
                if (item.Product == null || item.Quantity > item.Product.StockQuantity)
                {
                    TempData["Error"] = $"Stok yetersiz! '{item.Product?.Name}' için mevcut stok: {item.Product?.StockQuantity}";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Sipariş Oluştur
            var order = new Order
            {
                UserId = userId,
                OrderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"),
                OrderDate = DateTime.Now,
                Status = "Onaylandı",
                TotalAmount = cart.Items.Sum(i => i.Quantity * i.Product!.Price)
            };

            foreach (var item in cart.Items)
            {
                // Snapshot: Sipariş anındaki fiyat ve ürün bilgisi kopyalanır
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductCodeSnapshot = item.Product!.ProductCode,
                    ProductNameSnapshot = item.Product.Name,
                    UnitPriceSnapshot = item.Product.Price,
                    Quantity = item.Quantity,
                    TotalPrice = item.Quantity * item.Product.Price
                });

                // Stok miktarını anında düşür
                item.Product.StockQuantity -= item.Quantity;
            }

            // Sepeti temizle
            _context.CartItems.RemoveRange(cart.Items);
            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Siparişiniz başarıyla alındı! Sipariş No: {order.OrderNumber}";
            return RedirectToAction(nameof(Index));
        }
        // 5. Bayinin Geçmiş Siparişleri (Kendi sipariş geçmişi)
        public async Task<IActionResult> MyOrders()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Cart/MyOrders" });
            }

            var myOrders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId.Value)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(myOrders);
        }
    }
    
    }