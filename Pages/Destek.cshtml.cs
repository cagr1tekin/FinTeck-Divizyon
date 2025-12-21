using InteraktifKredi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages;

public class DestekModel : PageModel
{
    private readonly ILogger<DestekModel> _logger;

    public DestekModel(ILogger<DestekModel> logger)
    {
        _logger = logger;
    }

    [BindProperty]
    public string Subject { get; set; } = string.Empty;

    [BindProperty]
    public string Message { get; set; } = string.Empty;

    public List<FaqItem> FaqItems { get; set; } = new();

    public void OnGet()
    {
        // SSS sayfasındaki tüm soruları buraya getir
        FaqItems = new List<FaqItem>
        {
            new FaqItem
            {
                Id = 1,
                Question = "Kredi başvurusu nasıl yapılır?",
                Answer = "Kredi başvurusu yapmak için öncelikle TC Kimlik Numaranız ve GSM numaranız ile giriş yapmanız gerekmektedir. Ardından KVKK aydınlatma metnini okuyup onaylamanız, SMS ile gönderilen OTP kodunu doğrulamanız gerekmektedir. Bu adımları tamamladıktan sonra profil bilgilerinizi doldurarak kredi başvurunuzu tamamlayabilirsiniz.",
                Order = 1
            },
            new FaqItem
            {
                Id = 2,
                Question = "Başvuru sonucumu nasıl öğrenebilirim?",
                Answer = "Kredi başvurunuzun sonucunu 'Raporlarım' sayfasından görüntüleyebilirsiniz. Başvurunuzun durumu (Bekliyor, Onaylandı, Reddedildi) bu sayfada görüntülenir. Ayrıca başvuru sonucunuz SMS ile de bildirilir.",
                Order = 2
            },
            new FaqItem
            {
                Id = 3,
                Question = "KVKK kapsamında bilgilerim nasıl korunuyor?",
                Answer = "Kişisel verileriniz 6698 sayılı Kişisel Verilerin Korunması Kanunu (KVKK) kapsamında korunmaktadır. Tüm verileriniz şifreli olarak saklanır ve yalnızca yetkili personel tarafından erişilebilir. Verileriniz, başvuru sürecinin tamamlanması ve yasal saklama sürelerinin dolmasına kadar güvenli bir şekilde muhafaza edilir.",
                Order = 3
            },
            new FaqItem
            {
                Id = 4,
                Question = "OTP kodu gelmediyse ne yapmalıyım?",
                Answer = "OTP kodunuz gelmediyse, öncelikle telefon numaranızın doğru olduğundan emin olun. Eğer numara doğruysa, 'Tekrar Gönder' butonuna tıklayarak yeni bir OTP kodu talep edebilirsiniz. OTP kodları 2 dakika geçerlidir. Hala kod gelmiyorsa, operatörünüzle iletişime geçmeniz veya müşteri hizmetlerimizi aramanız önerilir.",
                Order = 4
            },
            new FaqItem
            {
                Id = 5,
                Question = "Profil bilgilerimi nasıl güncellerim?",
                Answer = "Profil bilgilerinizi güncellemek için 'Profilim' menüsünden ilgili sayfaya gidebilirsiniz. Burada adres bilgileri, meslek bilgileri, gelir bilgileri ve eş bilgileri gibi tüm profil bilgilerinizi güncelleyebilirsiniz. Değişikliklerinizi kaydettikten sonra bilgileriniz güncellenmiş olacaktır.",
                Order = 5
            },
            new FaqItem
            {
                Id = 6,
                Question = "Kredi hesaplama nasıl çalışır?",
                Answer = "Kredi hesaplama aracı, girdiğiniz kredi tutarı ve vade bilgilerine göre aylık taksit tutarınızı, toplam geri ödeme miktarınızı ve toplam faiz tutarınızı hesaplar. Hesaplama, güncel faiz oranları ve anüite formülü kullanılarak yapılır. Hesaplama sonuçları tahmini olup, gerçek kredi koşulları başvuru değerlendirmesi sonrasında belirlenir.",
                Order = 6
            },
            new FaqItem
            {
                Id = 7,
                Question = "Kredi notum neden düştü?",
                Answer = "Kredi notunuz, ödeme geçmişiniz, borç yükünüz, kredi kullanım süreniz ve yeni kredi başvurularınız gibi birçok faktöre bağlı olarak değişebilir. Geç ödemeler, yüksek borç oranları ve sık kredi başvuruları kredi notunuzu düşürebilir. Detaylı bilgi için 'Raporlarım' sayfasından kredi raporunuzu inceleyebilirsiniz.",
                Order = 7
            },
            new FaqItem
            {
                Id = 8,
                Question = "Şifremi nasıl değiştiririm?",
                Answer = "Şu anda sistemimiz TCKN ve GSM numarası ile giriş yapmaktadır. Şifre değiştirme özelliği yakında eklenecektir. Güvenlik endişeniz varsa, lütfen müşteri hizmetlerimiz ile iletişime geçin.",
                Order = 8
            },
            new FaqItem
            {
                Id = 9,
                Question = "Rapor güncelleme sıklığı nedir?",
                Answer = "Kredi raporunuz her ay otomatik olarak güncellenir. Yeni bir kredi başvurusu yaptığınızda veya profil bilgilerinizde önemli bir değişiklik olduğunda raporunuz anında güncellenir. Güncel raporunuzu 'Raporlarım' sayfasından görüntüleyebilirsiniz.",
                Order = 9
            },
            new FaqItem
            {
                Id = 10,
                Question = "Kredi başvurum ne kadar sürede sonuçlanır?",
                Answer = "Kredi başvurunuz, tüm gerekli belgelerin tamamlanmasından sonra genellikle 1-3 iş günü içinde sonuçlanır. Başvuru durumunuzu 'Raporlarım' sayfasından takip edebilir ve sonucu SMS ile de bildirilirsiniz.",
                Order = 10
            }
        };

        _logger.LogInformation("Destek sayfası yüklendi: {Count} soru gösteriliyor", FaqItems.Count);
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(Subject) || string.IsNullOrWhiteSpace(Message))
        {
            TempData["ErrorMessage"] = "Lütfen tüm alanları doldurun.";
            return Page();
        }

        _logger.LogInformation("Destek formu gönderildi: Konu={Subject}, Mesaj uzunluğu={Length}", Subject, Message.Length);
        
        TempData["SuccessMessage"] = "Talebiniz alınmıştır. En kısa sürede size dönüş yapacağız.";
        return Page();
    }
}

