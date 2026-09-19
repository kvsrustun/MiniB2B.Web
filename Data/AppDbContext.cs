using Microsoft.EntityFrameworkCore;
using MiniB2B.Web.Models;

namespace MiniB2B.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<GridColumnConfig> GridColumnConfigs { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Banner> Banners { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Banner>().HasData(
                new Banner
                {
                    Id = 1,
                    BadgeText = "ÖZEL BAYİ KAMPANYASI",
                    BadgeColor = "warning",
                    Title = "Endüstriyel Motor ve Şalterlerde %15 Toptan İndirim",
                    Description = "Bu aya özel tüm Siemens ve Schneider şalt grubu siparişlerinde geçerli avantajlı fiyatlar.",
                    BackgroundGradient = "linear-gradient(135deg, #1e3c72 0%, #2a5298 100%)",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new Banner
                {
                    Id = 2,
                    BadgeText = "YENİ STOK GİRİŞİ",
                    BadgeColor = "success",
                    Title = "SKF & FAG Yüksek Hızlı Rulman Serileri Depolarımızda!",
                    Description = "Kritik seviyedeki tüm yedek parça ve rulman grubu depolarımızda güncellendi.",
                    BackgroundGradient = "linear-gradient(135deg, #0f2027 0%, #203a43 50%, #2c5364 100%)",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new Banner
                {
                    Id = 3,
                    BadgeText = "ÖNEMLİ DUYURU",
                    BadgeColor = "danger",
                    Title = "Saat 16:00'a Kadar Verilen Siparişler Aynı Gün Kargoda",
                    Description = "B2B portalımız üzerinden siparişinizi tamamlayın, anında sevk hazırlığı başlasın.",
                    BackgroundGradient = "linear-gradient(135deg, #373b44 0%, #4286f4 100%)",
                    DisplayOrder = 3,
                    IsActive = true
                }
            );
        }
    }
}