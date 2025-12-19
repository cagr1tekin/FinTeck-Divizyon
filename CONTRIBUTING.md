# Katkıda Bulunma Rehberi

İnteraktif Kredi projesine katkıda bulunmak istediğiniz için teşekkürler! Bu rehber, projeye nasıl katkıda bulunabileceğinizi açıklar.

## 🚀 Başlangıç

1. Projeyi fork edin
2. Repository'yi klonlayın: `git clone https://github.com/yourusername/interaktif-kredi.git`
3. Feature branch oluşturun: `git checkout -b feature/amazing-feature`
4. Değişikliklerinizi yapın
5. Commit edin: `git commit -m 'Add some amazing feature'`
6. Push edin: `git push origin feature/amazing-feature`
7. Pull Request açın

## 📋 Kod Standartları

### C# / Razor Pages

- **Para hesaplamaları**: `decimal` tipi kullanın (float/double YASAK)
- **İş mantığı**: Sadece PageModel (.cshtml.cs) dosyalarında
- **View dosyaları**: Sadece görüntüleme mantığı
- **Html.Raw()**: Kullanmayın (XSS riski)
- **Log maskeleme**: Hassas veriler maskelenmeli

### SCSS

- **İsimlendirme**: BEM + snake_case
- **Nesting**: 3 seviyeyi geçmemeli
- **!important**: Kullanmayın
- **Tag selector**: Kullanmayın (reset hariç)

### JavaScript/jQuery

- **Selector caching**: Zorunlu
- **Event delegation**: Dinamik elementler için
- **Double submit prevention**: Zorunlu
- **console.log**: Production'da temizlenmeli

## 🧪 Test

Değişikliklerinizi yapmadan önce:

1. SCSS derlemesini kontrol edin: `npm run scss:build`
2. .NET build'i kontrol edin: `dotnet build`
3. Uygulamayı test edin: `dotnet run`

## 📝 Commit Mesajları

Açıklayıcı commit mesajları yazın:

```
feat: Yeni özellik eklendi
fix: Bug düzeltildi
docs: Dokümantasyon güncellendi
style: Kod formatı düzenlendi
refactor: Kod refactor edildi
test: Test eklendi
chore: Build/config değişiklikleri
```

## 🔍 Pull Request Süreci

1. PR açmadan önce son değişiklikleri çekin: `git pull origin main`
2. Tüm testlerin geçtiğinden emin olun
3. Linter hatalarını düzeltin
4. Açıklayıcı bir PR açıklaması yazın
5. İlgili issue'ları referans edin

## ❓ Sorular

Sorularınız için issue açabilir veya doğrudan iletişime geçebilirsiniz.

