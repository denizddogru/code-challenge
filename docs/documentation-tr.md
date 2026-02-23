# World League API — Teknik Dokümantasyon (TR)

## 1. Teknoloji Seçimleri

| Teknoloji | Versiyon | Seçim Gerekçesi |
|---|---|---|
| .NET | 10 | Güncel sürüm, minimal overhead, güçlü async desteği |
| ASP.NET Core Web API | 10 | Endüstri standardı, yerleşik DI, güçlü routing |
| Entity Framework Core | 10 | Type-safe ORM, temiz DbContext soyutlaması, Npgsql uyumlu |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10 | PostgreSQL için resmi EF Core sağlayıcısı |
| PostgreSQL | — | Güvenilir, ACID uyumlu, UUID native destekli |
| Scalar.AspNetCore | 2.x | Swagger UI'ın yerini alan modern, temiz API explorer |
| Swashbuckle.AspNetCore | 10.x | Scalar ile birlikte klasik Swagger UI |
| Microsoft.AspNetCore.OpenApi | 10 | Native .NET OpenAPI spec üretimi |
| Microsoft.Extensions.Localization | 10 | `IStringLocalizer<T>` ile yerleşik .NET lokalizasyon |
| Tailwind CSS CDN | 3.x | UI stili — build adımı yok, CDN üzerinden yüklenir |

---

## 2. Mimari

Proje, tek domainli bir API için **kasıtlı olarak sade tutulmuş katmanlı bir mimari** kullanmaktadır.

```
HTTP İsteği
    │
    ▼
┌───────────────────────────┐
│  GlobalExceptionHandler   │  ← Tüm exception'ları istemciye ulaşmadan yakalar
└─────────────┬─────────────┘
              │
              ▼
┌─────────────────────────────┐
│       DrawController        │  ← İnce HTTP adaptörü. try-catch yok, iş mantığı yok.
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│        DrawService          │  ← Tüm iş mantığı: validasyon, kura algoritması,
│       (IDrawService)        │    kalıcılık orkestrasyonu
└──────┬──────────┬───────────┘
       │          │
       ▼          ▼
┌──────────┐  ┌───────────────────┐
│AppDbCtx  │  │ ITeamDataProvider │  ← Bağımlılığı tersine çevrilmiş veri kaynağı
│(EF Core) │  │ (StaticTeamData   │    DrawService'e dokunmadan DB destekli ile değiştirilebilir
└──────────┘  │  Provider)        │
       │      └───────────────────┘
       ▼
  PostgreSQL
```

### Klasör Yapısı

```
src/WorldLeague.API/
├── Abstractions/           → ITeamDataProvider + TeamRecord
├── Controllers/            → DrawController (yalnızca HTTP katmanı)
├── Data/                   → AppDbContext + EF entity konfigürasyonu
├── DTOs/                   → DrawRequest, DrawResponse, GroupDto, TeamDto
├── Entities/               → DrawSession, Group, Team
├── Infrastructure/         → StaticTeamDataProvider, DbSeeder
├── Middleware/             → GlobalExceptionHandler
├── Migrations/             → EF Core migration'ları (başlangıçta otomatik uygulanır)
├── Resources/              → DrawMessages.resx (TR), DrawMessages.en.resx (EN)
├── Services/               → IDrawService, DrawService
├── wwwroot/                → index.html (Kura UI — "/" adresinde servis edilir)
├── DrawMessages.cs         → Localizer marker sınıfı
├── Program.cs              → Başlangıç, DI kayıtları
└── appsettings.json        → Konfigürasyon (connection string)
```

### URL Haritası

| URL | Açıklama |
|---|---|
| `http://localhost:5116/` | Kura UI |
| `http://localhost:5116/scalar` | Scalar API explorer |
| `http://localhost:5116/swagger` | Swagger UI |
| `http://localhost:5116/api/draw` | REST endpoint (POST) |

### Veritabanı Şeması

```
draw_sessions
  id                (UUID PK)
  drawer_first_name (VARCHAR 100)
  drawer_last_name  (VARCHAR 100)
  drawn_at          (TIMESTAMPTZ)
  group_count       (INT)

groups
  id               (UUID PK)
  name             (VARCHAR 1)    → 'A'dan 'H'e
  draw_session_id  (UUID FK → draw_sessions.id, CASCADE)

teams
  id        (UUID PK)
  name      (VARCHAR 100)
  country   (VARCHAR 50)
  group_id  (UUID FK → groups.id, CASCADE)
```

---

## 3. Yazılım Geliştirme Pratikleri

### Dependency Injection
Tüm bağımlılıklar `Program.cs` içinde ASP.NET Core'un yerleşik DI container'ı ile kayıt edilir:
- `ITeamDataProvider` → Singleton (veri değişmez, istek arası güvenle paylaşılır)
- `IDrawService` → Scoped (istek başına `AppDbContext` yaşam döngüsü ile uyumlu)
- `AppDbContext` → Scoped (EF Core varsayılanı — istek başına bir instance)

### Interface Tabanlı Programlama
Her önemli bağımlılık bir interface arkasına alınmıştır:
- `IDrawService` — controller somut `DrawService`'i bilmez
- `ITeamDataProvider` — service takımların nereden geldiğini bilmez

### `IStringLocalizer<T>` ile Lokalizasyon
Kullanıcıya gösterilen tüm hata mesajları `.resx` kaynak dosyalarında saklanır. `DrawService`, `IStringLocalizer<DrawMessages>` inject eder ve string anahtarları (`"GroupCountInvalid"`, `"DrawerNameRequired"`) kullanır. Yeni bir dil eklemek için yalnızca yeni bir `.resx` dosyası oluşturmak yeterlidir — sıfır kod değişikliği.

### Merkezi Exception Yönetimi
`GlobalExceptionHandler` (`IExceptionHandler` implementasyonu) tüm yakalanmamış exception'ları yakalar. Controller'lar fırlatır, handler yakalar ve response'u formatlar. Controller'larda try-catch bloğu bulunmaz.

### Sorumlulukların Ayrılması (Separation of Concerns)
- **DTO'lar** yalnızca HTTP sınırında kullanılır — entity'ler servis katmanının dışına çıkmaz
- **Entity'ler** iç kalıcılık modelleridir — JSON olarak serialize edilmez
- **Service'ler** tüm domain mantığını ve orkestrasyonu barındırır
- **Infrastructure** dış kaygıları (veri kaynakları) abstraction'lar arkasında izole eder

### Her Yerde Async/Await
Tüm veritabanı operasyonları EF Core'un async metodlarıyla (`SaveChangesAsync`) yapılır.

---

## 4. Tasarım Desenleri (Design Patterns)

### Service Pattern
İş mantığı `IDrawService` interface'i arkasındaki `DrawService` içinde kapsüllenir. Controller, kendi iş mantığı olmayan ince bir HTTP adaptörüdür.

### Data Transfer Object (DTO) Deseni
`DrawRequest` ve `DrawResponse`, API sözleşmesi olarak işlev görür. Entity'lerin HTTP katmanına sızmasını engeller ve ikisinin bağımsız olarak gelişebilmesini sağlar.

### Repository Deseni (EF Core DbContext Üzerinden)
`AppDbContext`, Unit of Work görevi üstlenir. `DbSet<T>`, örtük repository soyutlamasını sağlar. Tek-domain bir API için gereksiz dolaylılık yaratmamak adına ayrı bir explicit repository katmanı eklenmemiştir.

### Bağımlılık Tersine Çevirme İlkesi (DIP)
`DrawService`, `StaticTeamDataProvider`'a değil `ITeamDataProvider`'a bağımlıdır. Takımların bir veritabanından veya harici API'den gelmesi gerektiğinde, `Program.cs`'te yeni bir provider kayıt edilir — `DrawService`'e dokunulmaz.

### Strateji Deseni (Strategy Pattern — örtük)
`ITeamDataProvider` özünde takım verisi alma stratejisidir. Kura algoritması, giriş verisinin nereden geldiğinden bağımsız çalışır.

---

## 5. Yaratıcı Çözüm Yaklaşımı

İşte burada iş ilginçleşiyor.

Temel zorluk şuydu: **32 takımı gruplara rastgele dağıtırken aynı ülkeden iki takımın aynı grupta buluşmamasını nasıl garanti edersin?**

Bu, özünde bir **kısıtlı rastgele atama problemi**. Naif yaklaşım — tüm takımları karıştır ve sırayla ata — hemen başarısız olur. Rastgelelik körce uygulanırsa, Grup A'ya iki Türk takımı rahatlıkla düşebilir.

Burada kullanılan yaklaşım **anlık kısıt kontrolü ile açgözlü (greedy) round-robin kura**:

1. 32 takımın hepsi önce rastgele karıştırılır.
2. Kura, gereksinimde tam olarak açıklandığı gibi **slot slot** ilerler: Grup A'ya bir takım, ardından Grup B'ye, ardından Grup C'ye... en son gruba ulaşınca tekrar Grup A'nın ikinci slotuna geçilir. Ta ki tüm slotlar dolana kadar.
3. Bir gruba takım seçmeden önce algoritma, o grupta **hangi ülkelerin zaten temsil edildiğini** kontrol eder ve yalnızca **henüz yer almayan ülkelerden** takım seçer.

Bu, gerçek bir UEFA kurası gerilimini taklit eder: potadan bir top çekilir; ama bu top kısıtı ihlal edecekse (o grupta aynı ülke zaten varsa), geri konulur ve başka bir top çekilir. Bu problemin matematiksel yapısı (8 eşit büyüklükte ülke potu, tam bölünebilir grup boyutları) greedy yaklaşımın **hiçbir zaman çıkmaza girmeyeceğini** garanti eder — her adımda en az bir geçerli aday her zaman mevcuttur.

`ITeamDataProvider` abstraction'ı sayesinde algoritmanın kendisi takımların nereden geldiğinden tamamen habersizdir. Takım verisi düz bir liste olarak gelir (`IReadOnlyList<TeamRecord>`) — yapıda ülkeye göre gruplama gömülü değildir. Algoritma, ülke gruplamasını kura sırasında dinamik olarak her grup için canlı bir `HashSet<string>` kontrolüyle keşfeder. Temiz, bağımsız ve anlaşılması kolay.
