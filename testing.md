# Test Rehberi

## 1. Veritabanı Tabloları

Uygulama başladığında tablolar **otomatik oluşturulur** ve örnek veri **otomatik eklenir**. Elle SQL çalıştırmana gerek yok.

---

## 2. Connection String

`src/AdessoWorldLeague.API/appsettings.json` dosyasındaki bağlantı bilgisini kendi PostgreSQL ayarlarınla güncelle:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=ŞIFREN"
}
```

---

## 3. Uygulamayı Başlat

```bash
cd src/AdessoWorldLeague.API
dotnet run
```

---

## 4. API Explorer'ı Aç

| Araç | URL |
|---|---|
| **Scalar** | http://localhost:5116/scalar |
| **Swagger** | http://localhost:5116/swagger |

---

## 5. İstek At

`POST /api/draw` endpoint'ine aşağıdaki gibi bir istek gönder:

### 8 Gruplu Kura (grup başına 4 takım)

```json
{
  "groupCount": 8,
  "drawerFirstName": "Ahmet",
  "drawerLastName": "Yılmaz"
}
```

### 4 Gruplu Kura (grup başına 8 takım)

```json
{
  "groupCount": 4,
  "drawerFirstName": "Mehmet",
  "drawerLastName": "Kaya"
}
```

> Swagger'da: `Try it out` → `Execute` butonuna bas

---

## 6. Beklenen Yanıt

```json
{
  "drawerName": "Ahmet Yılmaz",
  "drawnAt": "2026-02-23T10:00:00Z",
  "groups": [
    {
      "groupName": "A",
      "teams": [
        { "name": "Adesso İstanbul" },
        { "name": "Adesso Berlin" },
        { "name": "Adesso Marsilya" },
        { "name": "Adesso Anvers" }
      ]
    },
    {
      "groupName": "B",
      "teams": [
        { "name": "Adesso İzmir" },
        { "name": "Adesso Dortmund" },
        { "name": "Adesso Barselona" },
        { "name": "Adesso Venedik" }
      ]
    }
  ]
}
```

**Kontrol edilecekler:**
- Her grupta aynı ülkeden 2 takım olmamalı
- `groupCount: 8` → her grupta 4 takım
- `groupCount: 4` → her grupta 8 takım
- Toplam 32 takımın tamamı gruplara dağılmış olmalı

---

## 7. Hata Durumları

**Geçersiz grup sayısı:**
```json
{ "groupCount": 5, "drawerFirstName": "Ali", "drawerLastName": "Veli" }
```
→ `400 Bad Request`: `"Grup sayısı 4 veya 8 olmalıdır."`

**İsim boş bırakıldığında:**
```json
{ "groupCount": 8, "drawerFirstName": "", "drawerLastName": "Veli" }
```
→ `400 Bad Request`: `"Kurayı çeken kişinin ad ve soyad bilgisi zorunludur."`

**İngilizce hata mesajı için** `Accept-Language: en` header'ı ekle:
→ `"Group count must be 4 or 8."`

---

## 8. Veritabanını Kontrol Et

```sql
-- Tüm kuralar
SELECT * FROM draw_sessions;

-- Belirli bir kuranın grupları ve takımları
SELECT g.name AS grup, t.name AS takim, t.country AS ulke
FROM groups g
JOIN teams t ON t.group_id = g.id
WHERE g.draw_session_id = '<session-uuid>'
ORDER BY g.name;
```
