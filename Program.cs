using Microsoft.EntityFrameworkCore;
using MiniB2B.Web.Data;
using MiniB2B.Web.Models;

try
{
    Console.WriteLine("--> Uygulama başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllersWithViews();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(60);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();

            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Endüstriyel Parçalar", IsActive = true },
                    new Category { Name = "Hırdavat & Civata", IsActive = true },
                    new Category { Name = "Elektrik & Otomasyon", IsActive = true }
                );
                context.SaveChanges();
            }

            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User
                    {
                        Username = "admin",
                        Email = "admin@b2b.com",
                        PasswordHash = "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92", // 123456 SHA-256
                        FirstName = "Sistem",
                        LastName = "Yöneticisi",
                        PhoneNumber = "05551112233",
                        Role = "Admin",
                        IsActive = true
                    },
                    new User
                    {
                        Username = "bayi1",
                        Email = "bayi1@b2b.com",
                        PasswordHash = "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92", // 123456 SHA-256
                        FirstName = "Ahmet",
                        LastName = "Yılmaz",
                        PhoneNumber = "05554445566",
                        Role = "Dealer",
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            if (!context.GridColumnConfigs.Any())
            {
                context.GridColumnConfigs.AddRange(
                    new GridColumnConfig { FieldName = "ImageUrl", HeaderTitle = "Görsel", DisplayOrder = 1, RenderType = "Image", Width = "70px", Alignment = "center", IsVisible = true },
                    new GridColumnConfig { FieldName = "ProductCode", HeaderTitle = "Ürün Kodu", DisplayOrder = 2, RenderType = "Text", Width = "120px", Alignment = "left", IsVisible = true },
                    new GridColumnConfig { FieldName = "Name", HeaderTitle = "Ürün Adı", DisplayOrder = 3, RenderType = "Text", Width = "auto", Alignment = "left", IsVisible = true },
                    new GridColumnConfig { FieldName = "Brand", HeaderTitle = "Marka", DisplayOrder = 4, RenderType = "Text", Width = "110px", Alignment = "left", IsVisible = true },
                    new GridColumnConfig { FieldName = "StockStatus", HeaderTitle = "Stok Durumu", DisplayOrder = 5, RenderType = "StockBadge", Width = "110px", Alignment = "center", IsVisible = true },
                    new GridColumnConfig { FieldName = "Price", HeaderTitle = "Fiyat", DisplayOrder = 6, RenderType = "Price", Width = "120px", Alignment = "right", IsVisible = true },
                    new GridColumnConfig { FieldName = "Action", HeaderTitle = "Sipariş Girişi", DisplayOrder = 7, RenderType = "ActionCart", Width = "150px", Alignment = "center", IsVisible = true }
                );
                context.SaveChanges();
            }

            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new Product
                    {
                        ProductCode = "PRD-1001",
                        Name = "Rulman 6204-2RS C3 Yüksek Devir",
                        Description = "Yüksek devirli elektrik motorları ve endüstriyel redüktörler için çift tarafı kauçuk kapaklı (2RS), C3 radyal boşluklu, aşınmaya ve yüksek ısıya dayanıklı premium bilyalı rulman.",
                        Brand = "SKF",
                        ManufacturerCode = "SKF-6204-2RSH/C3",
                        SpecialCode1 = "MEK-RLM",
                        SpecialCode2 = "RAF-A-04",
                        ImageUrl = "https://images.unsplash.com/photo-1581092160607-ee22621dd758?w=200",
                        StockQuantity = 45,
                        CriticalStockThreshold = 10,
                        Price = 185.50m,
                        CategoryId = 1,
                        IsActive = true
                    },
                    new Product
                    {
                        ProductCode = "PRD-1002",
                        Name = "Pnömatik Silindir 32x100 Çift Etkili",
                        Description = "ISO 15552 standartlarına uygun, 32 mm piston çapı ve 100 mm strok kapasiteli, ayarlanabilir yastıklamalı ve manyetik pistonlu çift etkili endüstriyel otomasyon silindiri.",
                        Brand = "Festo",
                        ManufacturerCode = "FESTO-DSBC-32-100-PPVA",
                        SpecialCode1 = "PNO-AKT",
                        SpecialCode2 = "RAF-B-12",
                        ImageUrl = "https://images.unsplash.com/photo-1581092335397-9583fe92d232?w=200",
                        StockQuantity = 4, 
                        CriticalStockThreshold = 10,
                        Price = 1450.00m,
                        CategoryId = 1,
                        IsActive = true
                    },
                    new Product
                    {
                        ProductCode = "PRD-1003",
                        Name = "Endüktif Sensör M12 PNP NO",
                        Description = "4 mm algılama mesafeli, M12 pirinç nikel kaplamalı gövde, IP67 su ve toz sızdırmazlık korumalı, LED durum göstergeli PNP normalde açık (NO) endüstriyel yaklaşım anahtarı.",
                        Brand = "Omron",
                        ManufacturerCode = "OMR-E2B-M12KS04-WP-B1",
                        SpecialCode1 = "ELEK-SNS",
                        SpecialCode2 = "RAF-C-01",
                        ImageUrl = "https://images.unsplash.com/photo-1518770660439-4636190af475?w=200",
                        StockQuantity = 0, 
                        CriticalStockThreshold = 5,
                        Price = 620.00m,
                        CategoryId = 3,
                        IsActive = true
                    },
                    new Product
                    {
                        ProductCode = "PRD-1004",
                        Name = "M8x40 İmbus Civata 8.8 Kalite",
                        Description = "DIN 912 normuna uygun, silindir başlı altı köşe soketli (alyan başlı), 8.8 kalite yüksek çekme mukavemetli çelikten imal edilmiş, siyah oksit kaplamalı endüstriyel bağlantı elemanı.",
                        Brand = "Norm Civata",
                        ManufacturerCode = "NRM-DIN912-88-M840",
                        SpecialCode1 = "BAG-HRD",
                        SpecialCode2 = "KUTU-18",
                        ImageUrl = "https://images.unsplash.com/photo-1530124566582-a618bc2615dc?w=200",
                        StockQuantity = 120,
                        CriticalStockThreshold = 20,
                        Price = 310.00m,
                        CategoryId = 2,
                        IsActive = true
                    },
                    new Product
                    {
                        ProductCode = "PRD-1005",
                        Name = "Motor Koruma Şalteri 4-6.3A",
                        Description = "3 kutuplu, 4.0 ile 6.3 Amper arası termik ve manyetik ayarlı açma sahasına sahip, buton kumandalı, aşırı akım ve kısa devreye karşı yüksek koruma sağlayan kompakt motor koruma şalteri.",
                        Brand = "Schneider",
                        ManufacturerCode = "SCH-GV2ME10-63A",
                        SpecialCode1 = "PANO-SLT",
                        SpecialCode2 = "RAF-D-08",
                        ImageUrl = "https://images.unsplash.com/photo-1581092580497-e0d23cbdf1dc?w=200",
                        StockQuantity = 12,
                        CriticalStockThreshold = 5,
                        Price = 890.00m,
                        CategoryId = 3,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            if (!context.Banners.Any())
            {
                context.Banners.AddRange(
                    new Banner
                    {
                        Title = "Endüstriyel Motor ve Şalterlerde %15 Toptan İndirim",
                        Description = "Bu aya özel tüm Siemens ve Schneider şalt grubu siparişlerinde geçerli avantajlı fiyatlar.",
                        BadgeText = "ÖZEL BAYİ KAMPANYASI",
                        BadgeColor = "warning",
                        BackgroundGradient = "linear-gradient(135deg, #1e3c72 0%, #2a5298 100%)",
                        ImageUrl = "https://images.unsplash.com/photo-1581092160607-ee22621dd758?w=500&auto=format&fit=crop&q=80",
                        DisplayOrder = 1,
                        IsActive = true
                    },
                    new Banner
                    {
                        Title = "SKF & FAG Yüksek Hızlı Rulman Serileri Depolarımızda!",
                        Description = "Kritik seviyedeki tüm yedek parça ve rulman grubu depolarımızda güncellendi.",
                        BadgeText = "YENİ STOK GİRİŞİ",
                        BadgeColor = "success",
                        BackgroundGradient = "linear-gradient(135deg, #0f2027 0%, #203a43 50%, #2c5364 100%)",
                        ImageUrl = "https://images.unsplash.com/photo-1581092335397-9583fe92d232?w=500&auto=format&fit=crop&q=80",
                        DisplayOrder = 2,
                        IsActive = true
                    },
                    new Banner
                    {
                        Title = "Saat 16:00'a Kadar Verilen Siparişler Aynı Gün Kargoda",
                        Description = "B2B portalımız üzerinden siparişinizi tamamlayın, anında sevk hazırlığı başlasın.",
                        BadgeText = "ÖNEMLİ DUYURU",
                        BadgeColor = "danger",
                        BackgroundGradient = "linear-gradient(135deg, #373b44 0%, #4286f4 100%)",
                        ImageUrl = "https://images.unsplash.com/photo-1616401784845-180882ba9ba8?w=500&auto=format&fit=crop&q=80",
                        DisplayOrder = 3,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }

            Console.WriteLine("--> Veritabanı kontrolü ve tohumlama başarıyla tamamlandı.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Veritabanı başlatma hatası: {ex.Message}");
        }
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        await next();
    });

    app.UseSession();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    Console.WriteLine("--> Sunucu dinlemeye başlıyor...");
    app.Run();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("================ HATA BULUNDU! ================");
    Console.WriteLine(ex.ToString());
    Console.WriteLine("================================================");
    Console.ResetColor();
    Console.WriteLine("Çıkmak için bir tuşa basın...");
    Console.ReadLine();
}