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

        public async Task<IActionResult> Index()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 2; 

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

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 2; 

            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.StockQuantity <= 0)
            {
                return Json(new { success = false, message = "Ürün tükendi veya stokta yok!" });
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                if (existingItem.Quantity + quantity <= product.StockQuantity)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    return Json(new { success = false, message = "Mevcut stok miktarını aşamazsınız!" });
                }
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

            int totalCount = cart.Items.Sum(i => i.Quantity);
            HttpContext.Session.SetInt32("CartCount", totalCount);

            return Json(new { success = true, cartCount = totalCount, message = $"'{product.Name}' sepete eklendi!" });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var item = await _context.CartItems.FindAsync(itemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            int userId = HttpContext.Session.GetInt32("UserId") ?? 2;
            var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            int cartCount = cart?.Items.Sum(i => i.Quantity) ?? 0;
            HttpContext.Session.SetInt32("CartCount", cartCount);

            return RedirectToAction(nameof(Index));
        }

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

            int userId = HttpContext.Session.GetInt32("UserId") ?? 2;
            var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            int cartCount = cart?.Items.Sum(i => i.Quantity) ?? 0;
            HttpContext.Session.SetInt32("CartCount", cartCount);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            int? loggedUserId = HttpContext.Session.GetInt32("UserId");

            if (!loggedUserId.HasValue)
            {
                TempData["Error"] = "Siparişi tamamlayabilmek için lütfen önce bayilik hesabınıza giriş yapın.";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Cart" });
            }

            int userId = loggedUserId.Value;

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if ((cart == null || !cart.Items.Any()) && userId != 2)
            {
                var guestCart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == 2);

                if (guestCart != null && guestCart.Items.Any())
                {
                    if (cart == null)
                    {
                        cart = new Cart { UserId = userId };
                        _context.Carts.Add(cart);
                        await _context.SaveChangesAsync();
                    }

                    foreach (var gItem in guestCart.Items.ToList())
                    {
                        cart.Items.Add(new CartItem
                        {
                            ProductId = gItem.ProductId,
                            Quantity = gItem.Quantity
                        });
                        _context.CartItems.Remove(gItem);
                    }
                    await _context.SaveChangesAsync();
                }
            }

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Sepetiniz boş!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var item in cart.Items)
            {
                if (item.Product == null || item.Quantity > item.Product.StockQuantity)
                {
                    TempData["Error"] = $"Stok yetersiz! '{item.Product?.Name}' için mevcut stok: {item.Product?.StockQuantity}";
                    return RedirectToAction(nameof(Index));
                }
            }

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
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductCodeSnapshot = item.Product!.ProductCode,
                    ProductNameSnapshot = item.Product.Name,
                    UnitPriceSnapshot = item.Product.Price,
                    Quantity = item.Quantity,
                    TotalPrice = item.Quantity * item.Product.Price
                });

                item.Product.StockQuantity -= item.Quantity;
            }

            _context.CartItems.RemoveRange(cart.Items);
            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("CartCount", 0);

            TempData["Success"] = $"Tebrikler! Siparişiniz başarıyla alındı. Sipariş No: {order.OrderNumber}";
            return RedirectToAction(nameof(Index));
        }

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

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 2;
            var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            int count = cart?.Items.Sum(i => i.Quantity) ?? 0;
            HttpContext.Session.SetInt32("CartCount", count);
            return Json(new { count = count });
        }
    }
}