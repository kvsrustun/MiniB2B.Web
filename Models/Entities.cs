using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniB2B.Web.Models
{
    // Kullanıcı Modeli
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = "Dealer"; // "Admin" veya "Dealer"
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }

    // Kategori Modeli
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    // Ürün Modeli (Ödev gereksinimleri: Kod, Ad, Marka, Özel Kodlar, Stok, Kritik Eşik vb.)
    public class Product
    {
        public int Id { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string? ManufacturerCode { get; set; }
        public string? SpecialCode1 { get; set; }
        public string? SpecialCode2 { get; set; }
        public string? ImageUrl { get; set; }
        public int StockQuantity { get; set; }
        public int CriticalStockThreshold { get; set; } = 10;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // Dinamik Grid Kolon Konfigürasyonu (Ödev Sayfa 4-5)
    public class GridColumnConfig
    {
        public int Id { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string HeaderTitle { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string RenderType { get; set; } = "Text"; // Image, Text, StockBadge, Price, ActionCart
        public string Width { get; set; } = "auto";
        public bool IsVisible { get; set; } = true;
        public string Alignment { get; set; } = "left";
    }

    // Sepet Kalemi
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // Sepet
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }

    // Sipariş
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Onay Bekliyor";
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    // Sipariş Kalemi (Snapshot Kuralı: Sipariş anındaki fiyatı saklar)
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductCodeSnapshot { get; set; } = string.Empty;
        public string ProductNameSnapshot { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPriceSnapshot { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
    }
}