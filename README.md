# İnteraktif Kredi - Web Şube 2.0

Modern ve güvenli bir FinTech Web Şube uygulaması. Kullanıcıların interaktif bir şekilde kredi başvurusu yapabildiği, dijital şube deneyimi sunan bir platform.

## 🚀 Özellikler

- ✅ **Onboarding Süreci**: TCKN-GSM doğrulama, OTP ile kimlik doğrulama, KVKK onayı
- 📊 **Dashboard**: Kredi hesaplama, profil tamamlama durumu, hızlı erişim
- 👤 **Profil Yönetimi**: Adres, meslek, gelir ve eş bilgileri yönetimi
- 📈 **Raporlar**: Kredi başvuru raporları ve detaylı görüntüleme
- ❓ **SSS**: Sık sorulan sorular ve yanıtları
- 🎨 **Modern UI**: Custom SCSS ile framework-free, responsive tasarım
- 🔒 **Güvenlik**: CSRF koruması, XSS koruması, hassas veri maskeleme
- ♿ **Erişilebilirlik**: ARIA labels, keyboard navigation, screen reader desteği

## 🛠️ Teknoloji Yığını

- **Backend**: ASP.NET Core 8.0 (Razor Pages)
- **Frontend**: Vanilla JavaScript + jQuery
- **Styling**: SCSS (Custom, framework-free)
- **Para Hesaplamaları**: decimal tipi (float/double kullanılmaz)

## 📋 Gereksinimler

- .NET 8.0 SDK
- Node.js 16+ (SCSS derleme için)
- npm veya yarn

## 🔧 Kurulum

### 1. Repository'yi klonlayın

```bash
git clone https://github.com/yourusername/interaktif-kredi.git
cd interaktif-kredi/InteraktifKredi
```

### 2. Bağımlılıkları yükleyin

```bash
# .NET paketleri
dotnet restore

# Node.js paketleri (SCSS derleme için)
npm install
```

### 3. Yapılandırma dosyalarını oluşturun

```bash
# Development ortamı için
cp appsettings.Development.json.example appsettings.Development.json

# Production ortamı için (opsiyonel)
cp appsettings.Production.json.example appsettings.Production.json
```

### 4. API ayarlarını yapılandırın

`appsettings.Development.json` dosyasını açın ve API endpoint'lerini ve code'ları güncelleyin:

```json
{
  "ApiSettings": {
    "CustomersApiUrl": "https://customers-api.azurewebsites.net",
    "IdcApiUrl": "https://api-idc.azurewebsites.net",
    "CustomersApiCode": "YOUR_API_CODE",
    // ... diğer ayarlar
  }
}
```

### 5. SCSS dosyalarını derleyin

```bash
npm run scss:build
```

veya watch modu için:

```bash
npm run scss:watch
```

### 6. Uygulamayı çalıştırın

```bash
dotnet run
```

Uygulama `http://localhost:5000` veya `https://localhost:5001` adresinde çalışacaktır.

## 📁 Proje Yapısı

```
InteraktifKredi/
├── Pages/                  # Razor Pages
│   ├── Onboarding/         # Giriş modülü
│   ├── Dashboard/          # Ana panel
│   ├── Profile/            # Profil yönetimi
│   └── Raporlar/           # Raporlar
├── Styles/                 # SCSS dosyaları
│   ├── abstracts/          # Değişkenler, mixins
│   ├── base/               # Reset, typography
│   ├── components/         # Bileşenler
│   ├── layout/             # Layout stilleri
│   └── pages/              # Sayfa stilleri
├── Services/               # API servisleri
├── Models/                 # Data modelleri
└── wwwroot/                # Statik dosyalar
```

## 🎨 SCSS Derleme

SCSS dosyalarını derlemek için:

```bash
# Tek seferlik derleme
npm run scss:build

# Watch modu (değişiklikleri otomatik derle)
npm run scss:watch
```

Derlenmiş CSS dosyası `wwwroot/css/main.css` konumunda oluşturulur.

## 🏗️ Production Build

Production için build almak için:

**PowerShell:**
```powershell
.\build-production.ps1
```

**Bash (Linux/macOS):**
```bash
./build-production.sh
```

veya manuel olarak:

```bash
npm run scss:build
dotnet publish -c Release -o ./publish
```

## 🔒 Güvenlik

- ✅ CSRF token koruması
- ✅ XSS koruması (Html.Raw kullanılmaz)
- ✅ Hassas veri maskeleme (TCKN, GSM, vb.)
- ✅ Security headers (HSTS, CSP, X-Frame-Options, vb.)
- ✅ Session güvenliği
- ✅ Input validation (client-side ve server-side)

## 📝 Kod Standartları

- **CSS/SCSS**: BEM + snake_case metodolojisi
- **JavaScript**: jQuery ile selector caching, event delegation
- **C#**: Razor Pages, decimal tipi para hesaplamaları için
- **Yorumlar**: Türkçe
- **Değişken/Fonksiyon İsimleri**: İngilizce

Detaylı kurallar için `.cursorrules` dosyasına bakın.

## 🧪 Test

```bash
# Tüm testleri çalıştır
dotnet test

# Belirli bir test projesi
dotnet test --project Tests/InteraktifKredi.Tests
```

## 📚 Dokümantasyon

- [Deployment Checklist](DEPLOYMENT_CHECKLIST.md)
- [Production Ready](PRODUCTION_READY.md)
- [Optimization Summary](OPTIMIZATION_SUMMARY.md)

## 🤝 Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit edin (`git commit -m 'Add some amazing feature'`)
4. Push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

## 📄 Lisans

Bu proje özel bir projedir. Tüm hakları saklıdır.

## 👥 Ekip

- İnteraktif Kredi Danışmanlık A.Ş.

## 📞 İletişim

Sorularınız için issue açabilir veya doğrudan iletişime geçebilirsiniz.

---

**Not**: Production ortamında kullanmadan önce `appsettings.Production.json` dosyasını yapılandırmayı ve tüm güvenlik ayarlarını kontrol etmeyi unutmayın.

