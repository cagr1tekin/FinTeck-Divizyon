# 🚀 Production Deployment Checklist

## Pre-Deployment

### 1. Build & Compile ✅
- [x] SCSS derleme ve minification: `npm run scss:build`
- [x] .NET projesi build: `dotnet build -c Release`
- [x] .NET publish: `dotnet publish -c Release -o ./publish`

### 2. Environment Variables ✅
- [ ] `ASPNETCORE_ENVIRONMENT=Production` ayarlandı
- [ ] `appsettings.Production.json` dosyası oluşturuldu
- [ ] API URL'leri production için güncellendi
- [ ] Production token ayarlandı
- [ ] Connection strings (varsa) ayarlandı

### 3. Security Configuration ✅
- [x] HTTPS redirection aktif
- [x] HSTS aktif (1 yıl max-age)
- [x] Security headers eklendi:
  - [x] X-Content-Type-Options: nosniff
  - [x] X-Frame-Options: DENY
  - [x] X-XSS-Protection: 1; mode=block
  - [x] Referrer-Policy: strict-origin-when-cross-origin
  - [x] Content-Security-Policy (production)
- [x] Session cookie SecurePolicy: Always (production)
- [x] CSRF protection aktif (Razor Pages default)

## Functional Testing

### 4. Sayfa Testleri
- [ ] Ana sayfa (`/`) yükleniyor
- [ ] Onboarding sayfaları:
  - [ ] TCKN/GSM giriş (`/Onboarding/TcknGsm`)
  - [ ] KVKK Onay (`/Onboarding/KvkkOnay`)
  - [ ] OTP Doğrulama (`/Onboarding/OtpDogrula`)
- [ ] Dashboard (`/Dashboard`)
- [ ] Profil sayfaları:
  - [ ] Adres Bilgileri (`/Profile/Adres`)
  - [ ] Meslek Bilgileri (`/Profile/MeslekBilgileri`)
  - [ ] Gelir Bilgileri (`/Profile/GelirBilgileri`)
  - [ ] Eş Bilgileri (`/Profile/EsBilgileri`)
- [ ] Rapor sayfaları:
  - [ ] Rapor Listesi (`/Raporlar/Liste`)
  - [ ] Rapor Detay (`/Raporlar/Detay`)
- [ ] SSS (`/SSS`)
- [ ] Error sayfaları:
  - [ ] 404 (`/NotFound`)
  - [ ] 500 (`/Error`)

### 5. API Entegrasyonları
- [ ] TCKN/GSM doğrulama API
- [ ] KVKK metni çekme API
- [ ] KVKK onay kaydetme API
- [ ] OTP generate API
- [ ] OTP send SMS API
- [ ] OTP verify API
- [ ] Adres çekme/kaydetme API
- [ ] Meslek bilgileri API
- [ ] Gelir bilgileri API
- [ ] Eş bilgileri API
- [ ] Rapor listesi API
- [ ] Rapor detay API

### 6. Form Validasyonları
- [ ] TCKN validation (11 hane, sadece rakam)
- [ ] GSM validation (10-11 hane, sadece rakam)
- [ ] KVKK checkbox validation
- [ ] OTP validation (6 hane)
- [ ] Adres form validation
- [ ] Meslek form validation
- [ ] Gelir form validation (decimal para formatı)
- [ ] Eş bilgileri form validation
- [ ] Double submit prevention çalışıyor

### 7. Responsive Tasarım
- [ ] Mobile (320px - 767px) - Tüm sayfalar
- [ ] Tablet (768px - 1023px) - Tüm sayfalar
- [ ] Desktop (1024px+) - Tüm sayfalar
- [ ] Hamburger menü çalışıyor (mobile)
- [ ] Form'lar responsive
- [ ] Kartlar responsive

### 8. Error Handling
- [ ] 404 sayfası gösteriliyor
- [ ] 500 sayfası gösteriliyor
- [ ] API hataları yakalanıyor
- [ ] Network hataları yakalanıyor
- [ ] Session timeout handling çalışıyor
- [ ] Loading states gösteriliyor
- [ ] Error mesajları Türkçe

### 9. Loading States
- [ ] Global loading spinner çalışıyor
- [ ] Sayfa bazlı loading state'leri çalışıyor
- [ ] AJAX çağrılarında loading gösteriliyor
- [ ] Form submit'te loading gösteriliyor

### 10. Security
- [ ] HTTPS zorunlu (HTTP -> HTTPS redirect)
- [ ] Security headers gönderiliyor
- [ ] CSRF token form'larda mevcut
- [ ] XSS protection aktif (Html.Raw kullanılmadı)
- [ ] Session cookie HttpOnly
- [ ] Session cookie Secure (production)
- [ ] Hassas veriler loglarda maskeleniyor

## Performance Testing

### 11. Performance
- [ ] Sayfa yükleme süreleri < 3 saniye
- [ ] CSS minified ve compressed
- [ ] JavaScript dosyaları cache'leniyor
- [ ] Static files cache'leniyor (1 yıl)
- [ ] Font preconnect çalışıyor
- [ ] Görseller optimize edildi (opsiyonel)

## Accessibility Testing

### 12. Accessibility
- [ ] Keyboard navigation çalışıyor (Tab, Enter, Space)
- [ ] Focus indicators görünür
- [ ] ARIA labels mevcut
- [ ] Screen reader test (NVDA/JAWS)
- [ ] Color contrast WCAG AA uyumlu
- [ ] Skip to main content link çalışıyor

## Cross-Browser Testing

### 13. Browser Compatibility
- [ ] Chrome (latest) - Tüm özellikler çalışıyor
- [ ] Firefox (latest) - Tüm özellikler çalışıyor
- [ ] Edge (latest) - Tüm özellikler çalışıyor
- [ ] Safari (latest) - Tüm özellikler çalışıyor
- [ ] Mobile browsers (Chrome, Safari) - Responsive çalışıyor

## Post-Deployment

### 14. Monitoring
- [ ] Application logs kontrol edildi
- [ ] Error tracking aktif (opsiyonel)
- [ ] Performance monitoring aktif (opsiyonel)
- [ ] Analytics entegrasyonu (opsiyonel)

### 15. Documentation
- [ ] API dokümantasyonu güncel
- [ ] Deployment guide hazır
- [ ] Environment variables dokümante edildi
- [ ] Troubleshooting guide hazır

## Production Build Komutları

```bash
# 1. SCSS derleme (minified)
npm run scss:build

# 2. .NET build (Release)
dotnet build -c Release

# 3. .NET publish
dotnet publish -c Release -o ./publish

# 4. Publish klasörü içeriği deployment için hazır
```

## Environment Variables (Production)

Aşağıdaki environment variables'ları production ortamında ayarlayın:

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://yourdomain.com
```

`appsettings.Production.json` dosyasında:
- `ApiSettings:CustomersApi` - Production API URL
- `ApiSettings:IdcApi` - Production IDC API URL
- `ApiSettings:DefaultToken` - Production API token
- `Session:IdleTimeout` - Session timeout (dakika)
- `Session:CookieSecure` - Cookie secure policy (true/false)
- `Hsts:MaxAge` - HSTS max age (saniye)
- `Hsts:IncludeSubDomains` - HSTS include subdomains (true/false)
- `Hsts:Preload` - HSTS preload (true/false)

## Notlar

1. **Security Headers**: Production'da CSP (Content-Security-Policy) aktif. Gerekirse CDN veya external script'ler için ayarlanmalı.

2. **Session Timeout**: Production'da 30 dakika. Gerekirse `appsettings.Production.json`'dan değiştirilebilir.

3. **API Token**: Production token'ı kesinlikle `appsettings.Production.json`'a eklenmeli veya environment variable olarak ayarlanmalı.

4. **HTTPS**: Production'da HTTPS zorunlu. HTTP istekleri otomatik olarak HTTPS'e yönlendirilir.

5. **Static Files Cache**: Production'da static files 1 yıl cache'lenir. Değişiklik yapıldığında cache busting için `asp-append-version="true"` kullanılır.

