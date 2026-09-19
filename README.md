# 🏢 Mini B2B E-Ticaret ve Bayi Sipariş Yönetim Portalı

Bu proje; kurumsal firmalar ile bayileri (B2B) arasındaki ürün listeleme, dinamik grid konfigürasyonu, sepete hızlı sipariş aktarımı, stok kontrol mekanizmaları, anlık fiyat snapshot'ı ve sipariş onay/ret süreçlerini uçtan uca dijitalleştiren modern bir **B2B E-Ticaret Web Portalıdır**.

Proje, kurumsal yazılım geliştirme standartlarına uygun olarak **.NET 8 MVC**, **Entity Framework Core** ve **Microsoft SQL Server** mimarisi üzerinde inşa edilmiştir.

---

## 📑 İçindekiler
1. [Geliştirme Ortamı ve Kullanılan Teknolojiler](#-geliştirme-ortamı-ve-kullanılan-teknolojiler)
2. [Paketler ve Kütüphaneler (NuGet)](#-paketler-ve-kütüphaneler-nuget)
3. [Öne Çıkan Mimari ve İşlevsel Özellikler](#-öne-çıkan-mimari-ve-işlevsel-özellikler)
4. [Kullanıcı Rolleri ve Yetki Matrisi](#-kullanıcı-rolleri-ve-yetki-matrisi)
5. [Varsayılan Test Hesapları](#-varsayılan-test-hesapları)
6. [Adım Adım Kurulum ve Çalıştırma Rehberi](#-adım-adım-kurulum-ve-çalıştırma-rehberi)
7. [Veritabanı Şeması ve Tablo Yapısı](#-veritabanı-şeması-ve-tablo-yapısı)
8. [Güvenlik ve Validasyon Yaklaşımı](#-güvenlik-ve-validasyon-yaklaşımı)

---

## 🛠️ Geliştirme Ortamı ve Kullanılan Teknolojiler

Bu proje aşağıdaki ortam ve araçlar üzerinde bizzat geliştirilmiş ve test edilmiştir:

* **İşletim Sistemi:** Windows 11
* **Geliştirme Ortamı (IDE):** Visual Studio 2022 (Insiders)
* **Çalışma Zamanı (Runtime):** .NET 8.0 SDK
* **Veritabanı Motoru:** Microsoft SQL Server (MSSQL)
* **Veritabanı Yönetim Aracı:** SQL Server Management Studio (SSMS)
* **Frontend Kütüphaneleri:** Razor Pages, HTML5, CSS3, Bootstrap 5, FontAwesome 6, JavaScript (Fetch API)

---

## 📦 Paketler ve Kütüphaneler (NuGet)

Proje içerisinde Entity Framework Core ve SQL Server bağlantısı için aşağıdaki resmi Microsoft kütüphaneleri kullanılmıştır:

| Paket Adı | Sürüm | Kullanım Amacı |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | `8.0` | Çekirdek ORM altyapısı ve LINQ sorgu desteği |
| `Microsoft.EntityFrameworkCore.SqlServer` | `8.0` | Microsoft SQL Server veritabanı sürücüsü |
| `Microsoft.EntityFrameworkCore.Tools` | `8.0` | Migration ve veritabanı komut desteği |
| `Microsoft.EntityFrameworkCore.Design` | `8.0` | EF Core tasarım zamanı araçları |

---

## 🌟 Öne Çıkan Mimari ve İşlevsel Özellikler

### 1. Dinamik Grid Motoru
* Bayilerin yüzlerce ürünü hızlıca inceleyip doğrudan adet girerek sipariş verebilmesi için tablo (satır bazlı grid) yapısı kurgulanmıştır.
* Grid kolonları sabit HTML kodları değildir; veritabanında `GridColumnConfigs` tablosunda saklanır.
* Yönetici, panel üzerinden hangi kolonun gösterileceğini, kolon sırasını, başlık adını, genişliğini ve hizalamasını tek bir satır kod değiştirmeden dinamik olarak yönetebilir.

### 2. Akıllı ve Dinamik Kritik Stok Göstergesi
* Bayilere ham stok rakamları yerine kurumsal göstergeler sunulur:
  * 🟢 **Var (Yeşil Rozet):** Stok miktarı ürünün kritik eşiğinin üzerindeyse.
  * 🟡 **Kritik (Sarı Rozet):** Stok miktarı kritik eşiğin altına inmişse ancak 0'dan büyükse.
  * 🔴 **Yok (Kırmızı Rozet):** Stok sıfır veya negatif ise.
* Kritik stok seviyesi sistem genelinde sabit bir sayı değildir; her ürünün kendine özel kritik stok eşiği (`CriticalStockThreshold`) üzerinden bağımsız olarak değerlendirilir.

### 3. Fiyat ve Ürün Snapshot (Anlık Değer) Koruması
* Bayi bir sipariş oluşturduğunda; sipariş anındaki birim fiyat, ürün kodu ve ürün adı sipariş kalemlerine kalıcı olarak kaydedilir.
* Yönetici ürünün liste fiyatını daha sonra değiştirse bile, bayinin geçmiş sipariş faturasındaki tutar bozulmaz, satın alındığı andaki fiyat üzerinden korunur.

### 4. Backend Çift Katmanlı Stok Doğrulaması
* Stok kontrolü yalnızca arayüzde yapılmaz. Sipariş onayı aşamasında sunucu tarafında veritabanındaki güncel stok anlık olarak kontrol edilir.
* Eğer sepetteki adet güncel stoktan fazlaysa sipariş engellenir ve bayiye mevcut stok miktarını belirten açıklayıcı bir uyarı gösterilir. Başarılı siparişte stok veritabanından anında düşülür.

### 5. Yönetilebilir Kampanya ve Slider Motoru
* Ana sayfada bayileri karşılayan duyuru ve reklam afişleri yönetim panelinden canlı olarak eklenebilir, düzenlenebilir, pasife alınabilir veya silinebilir.

---

## 👥 Kullanıcı Rolleri ve Yetki Matrisi

Uygulama **Rol Tabanlı Erişim Kontrolü (RBAC)** ile korunmaktadır:

| Yetki / İşlem | Ziyaretçi | Bayi (`Dealer`) | Yönetici (`Admin`) |
|---|:---:|:---:|:---:|
| Ana Sayfa ve Kampanyaları Görme | ✅ | ✅ | ✅ |
| Ürün Kataloğu ve Detay Popup İnceleme | ✅ | ✅ | ✅ |
| Hızlı Sepete Ekleme ve Adet Güncelleme | ✅ | ✅ | ❌ |
| Sipariş Oluşturma ve Sipariş Geçmişi | ❌ | ✅ | ❌ |
| Ürün Ekleme, Düzenleme, Silme | ❌ | ❌ | ✅ |
| Dinamik Grid Kolonlarını Yapılandırma | ❌ | ❌ | ✅ |
| Siparişleri Onaylama veya Reddetme | ❌ | ❌ | ✅ |
| Bayi Kullanıcılarını Yönetme ve Rol Değiştirme | ❌ | ❌ | ✅ |
| Kampanya Bannerlarını Yönetme | ❌ | ❌ | ✅ |

---

## 🔑 Varsayılan Test Hesapları

Sistem ilk ayağa kalktığında testlerin anında yapılabilmesi için otomatik olarak aşağıdaki kullanıcıları tanımlar:

| Rol | Kullanıcı Adı | Şifre | Erişim Adresi | Açıklama |
|---|---|---|---|---|
| **Yönetici** | `admin` | `123456` | `/Admin` | Dashboard & Ciro Takibi, Ürün Yönetimi, Kullanıcı Yönetimi, İlan & Slider Yönetimi, Kolon Ayarları ve Tüm Siparişlerin Onay/Red Süreçleri  |
| **Bayi** | `bayi1` | `123456` | `/` | Ürün sipariş girişi, sepet ve Siparişlerim ekranı |

> 🔒 *Güvenlik: Veritabanında hiçbir şifre açık metin olarak tutulmaz. Şifreler tek yönlü **SHA-256 Hash** algoritmasıyla şifrelenmektedir.*

---

## 🚀 Adım Adım Kurulum ve Çalıştırma Rehberi

Projeyi çalıştırmak için 2 farklı yöntem sunulmuştur:

### Yöntem 1: Otomatik Kurulum (Önerilen)
SQL Server'da hiçbir tablo olmasa dahi proje ilk çalıştırmada kendi veritabanını oluşturur:

1. Projeyi bilgisayarınıza indirin veya klonlayın:  
   `git clone https://github.com/kvsrustun/MiniB2B.Web.git`
2. Projeyi **Visual Studio 2022** ile açın.
3. `appsettings.json` dosyasındaki SQL Server bağlantı dizenizi kontrol edin:  
   `"Server=localhost;Database=MiniB2BDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"`
4. Klavyeden **F5** tuşuna basarak projeyi başlatın.
5. Veritabanı yoksa SQL Server'da `MiniB2BDb` adıyla anında oluşturulur; kullanıcılar, kategoriler, grid kolonları, ürünler ve slider afişleri otomatik olarak yüklenir.
6. Tarayıcıda açılan ekranda doğrudan test etmeye başlayabilirsiniz.

---

### Yöntem 2: Manuel SQL Script ile Kurulum
1. SQL Server Management Studio (SSMS) uygulamasını açın.
2. Proje kök dizininde yer alan **`MiniB2B_DbScript.sql`** dosyasını SSMS ekranına sürükleyin.
3. **Execute (F5)** butonuna basarak scripti çalıştırın.
4. Tüm veritabanı, tablolar, indeksler ve başlangıç kayıtları oluşturulacaktır.
5. Projeyi Visual Studio üzerinden **F5** ile çalıştırın.

---

## 📊 Veritabanı Şeması ve Tablo Yapısı

İlişkisel veritabanı (MSSQL) üzerinde aşağıdaki 7 temel tablo kurgulanmıştır:

* **`Users`:** Bayi ve yönetici kimlik bilgileri, iletişim numaraları, rolleri (`Admin`/`Dealer`) ve SHA-256 şifre hashleri.
* **`Categories`:** Ürün kategorileri.
* **`Products`:** ERP ve B2B standartlarına uygun alanlar (`ProductCode`, `Name`, `Brand`, `ManufacturerCode`, `SpecialCode1`, `SpecialCode2`, `CriticalStockThreshold`, `StockQuantity`, `Price`, `ImageUrl`).
* **`GridColumnConfigs`:** Dinamik tablo kolon ayarları (`FieldName`, `HeaderTitle`, `DisplayOrder`, `RenderType`, `Width`, `Alignment`, `IsVisible`).
* **`Carts` & `CartItems`:** Bayiye özel sepet başlığı ve sepetteki ürün adetleri.
* **`Orders` & `OrderItems`:** Kesinleşmiş siparişler ve anlık fiyat korumalı snapshot kalemleri.
* **`Banners`:** Ana sayfa kampanya ve duyuru slider kayıtları.

---

## 🛡️ Güvenlik ve Validasyon Yaklaşımı

1. **SQL Injection Koruması:** Entity Framework Core LINQ sorgulama altyapısı kullanılarak tüm parametreler güvenli hale getirilmiştir; ham SQL birleştirmelerinden kaçınılmıştır.
2. **XSS (Cross-Site Scripting) Koruması:** ASP.NET Core Razor motorunun otomatik HTML-Encoding özelliği sayesinde zararlı script enjeksiyonları engellenir.
3. **Şifre Güvenliği:** Kullanıcı şifreleri açık metin olarak değil, tek yönlü **SHA-256 Hash** algoritması ile kriptolanarak saklanır.
4. **Yetkilendirme Güvenliği (Authorization Filter):** `AdminController` üzerinde çalışan global filtre ile yetkisiz veya oturumsuz kullanıcıların URL manipülasyonuyla yönetim paneline girmesi engellenir.
5. **IDOR Koruması:** Sipariş listeleme ve detay sorgularında kullanıcı kimlik doğrulaması yapılarak hiçbir bayinin başka bir bayiye ait siparişi görmesine izin verilmez.
6. **Security Headers:** Clickjacking ve MIME-sniffing saldırılarına karşı `X-Frame-Options: SAMEORIGIN`, `X-Content-Type-Options: nosniff` başlıkları HTTP yanıtlarına eklenmiştir.
