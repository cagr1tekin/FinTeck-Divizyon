using InteraktifKredi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages.SSS;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public List<FaqItem> FaqItems { get; set; } = new();

    public void OnGet()
    {
        // Örnek SSS içerikleri
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
            }
        };

        _logger.LogInformation("SSS sayfası yüklendi: {Count} soru gösteriliyor", FaqItems.Count);
    }
}
