using InteraktifKredi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages.KrediRehberi;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public List<BlogPost> FeaturedPosts { get; set; } = new();
    public List<BlogPost> MostReadPosts { get; set; } = new();

    public void OnGet()
    {
        // Public sayfa - login kontrolü yok
        // Mock blog yazıları
        LoadMockBlogPosts();
    }

    private void LoadMockBlogPosts()
    {
        var allPosts = new List<BlogPost>
        {
            new BlogPost
            {
                Id = 1,
                Title = "Yılbaşı Kredisi İçin En Doğru Zamanı Kaçırmayın",
                Slug = "yilbasi-kredisi-icin-en-dogru-zamani-kacirmayin",
                Excerpt = "Yılbaşı yaklaşırken kredi ihtiyacınız için en uygun zamanı belirlemek önemlidir. Bu rehberde, yılbaşı kredisi başvuruları için en doğru zamanı ve dikkat edilmesi gerekenleri öğreneceksiniz.",
                Content = GetBlogContent1(),
                Author = "Kredi Uzmanı",
                ImageUrl = "https://images.unsplash.com/photo-1554224155-6726b3ff858f?w=800&h=600&fit=crop",
                PublishedDate = DateTime.Now.AddDays(-1),
                ReadCount = 1250,
                IsFeatured = true,
                Category = "Kredi Rehberi"
            },
            new BlogPost
            {
                Id = 2,
                Title = "Kredi Kapatma Rehberi: Tüm Sorular ve Cevaplar",
                Slug = "kredi-kapatma-rehberi-tum-sorular-ve-cevaplar",
                Excerpt = "Kredi kapatma sürecinde merak ettiğiniz tüm soruların cevaplarını bu kapsamlı rehberde bulabilirsiniz. Erken kapatma, faiz hesaplama ve dikkat edilmesi gerekenler.",
                Content = GetBlogContent2(),
                Author = "Kredi Uzmanı",
                ImageUrl = "https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?w=800&h=600&fit=crop",
                PublishedDate = DateTime.Now.AddDays(-5),
                ReadCount = 980,
                IsFeatured = false,
                Category = "Kredi Rehberi"
            },
            new BlogPost
            {
                Id = 3,
                Title = "Ticari Kredi Nedir? Firmanız İçin En Akıllı Kredi Nasıl Seçilir?",
                Slug = "ticari-kredi-nedir-firmaniz-icin-en-akilli-kredi-nasil-secilir",
                Excerpt = "Ticari krediler hakkında bilmeniz gereken her şey. İşletmeniz için en uygun kredi türünü seçerken dikkat etmeniz gereken kriterler ve ipuçları.",
                Content = GetBlogContent3(),
                Author = "Kredi Uzmanı",
                ImageUrl = "https://images.unsplash.com/photo-1460925895917-afdab827c52f?w=800&h=600&fit=crop",
                PublishedDate = DateTime.Now.AddDays(-21),
                ReadCount = 750,
                IsFeatured = false,
                Category = "Ticari Kredi"
            }
        };

        FeaturedPosts = allPosts.Take(3).ToList(); // İlk 3 yazıyı featured olarak al
        MostReadPosts = allPosts.OrderByDescending(p => p.ReadCount).Take(5).ToList();
    }

    private string GetBlogContent1()
    {
        return @"<h2>Yılbaşı Kredisi İçin En Doğru Zaman</h2>
<p>Yılbaşı yaklaşırken birçok kişi kredi ihtiyacı duyuyor. Ancak kredi başvurusu yapmak için en uygun zamanı belirlemek oldukça önemlidir. Bu rehberde, yılbaşı kredisi başvuruları için en doğru zamanı ve dikkat edilmesi gerekenleri detaylıca ele alacağız.</p>

<h3>1. Kredi Başvurusu İçin En İyi Zaman</h3>
<p>Yılbaşı öncesi dönem, kredi başvuruları için genellikle yoğun bir dönemdir. Bankalar bu dönemde özel kampanyalar düzenleyebilir ve daha esnek kredi koşulları sunabilir. Ancak, başvuru yapmadan önce şu faktörleri göz önünde bulundurmalısınız:</p>
<ul>
<li>Kredi notunuzun güncel durumu</li>
<li>Gelir durumunuzun istikrarlı olması</li>
<li>Mevcut borçlarınızın oranı</li>
<li>Bankaların kampanya dönemleri</li>
</ul>

<h3>2. Dikkat Edilmesi Gerekenler</h3>
<p>Yılbaşı kredisi başvurusu yaparken dikkat etmeniz gereken en önemli noktalar:</p>
<ul>
<li>Faiz oranlarını karşılaştırın</li>
<li>Gizli masrafları kontrol edin</li>
<li>Ödeme planınızı netleştirin</li>
<li>Erken ödeme koşullarını öğrenin</li>
</ul>

<h3>3. Sonuç</h3>
<p>Yılbaşı kredisi başvurusu yapmadan önce tüm seçenekleri değerlendirmeli ve size en uygun teklifi seçmelisiniz. İnteraktif Kredi platformu üzerinden tüm bankaların tekliflerini karşılaştırabilir ve en avantajlı seçeneği bulabilirsiniz.</p>";
    }

    private string GetBlogContent2()
    {
        return @"<h2>Kredi Kapatma Rehberi</h2>
<p>Kredi kapatma süreci, birçok kişi için karmaşık görünebilir. Bu kapsamlı rehberde, kredi kapatma ile ilgili merak ettiğiniz tüm soruların cevaplarını bulacaksınız.</p>

<h3>1. Erken Kredi Kapatma</h3>
<p>Erken kredi kapatma, kredinizin vadesinden önce tamamını ödemek anlamına gelir. Bu işlem için genellikle bir erken kapatma cezası uygulanır. Ancak bazı durumlarda bu ceza uygulanmayabilir.</p>

<h3>2. Faiz Hesaplama</h3>
<p>Kredi kapatırken ödemeniz gereken toplam tutar, anapara, faiz ve varsa erken kapatma cezasından oluşur. Doğru hesaplama yapmak için bankanızdan detaylı bir ödeme planı talep edebilirsiniz.</p>

<h3>3. Dikkat Edilmesi Gerekenler</h3>
<ul>
<li>Erken kapatma cezasını öğrenin</li>
<li>Kalan borç tutarını netleştirin</li>
<li>Kredi notunuzun etkilenip etkilenmeyeceğini sorun</li>
<li>Alternatif ödeme seçeneklerini değerlendirin</li>
</ul>";
    }

    private string GetBlogContent3()
    {
        return @"<h2>Ticari Kredi Rehberi</h2>
<p>Ticari krediler, işletmelerin finansal ihtiyaçlarını karşılamak için kullandıkları kredi türleridir. Bu rehberde, ticari krediler hakkında bilmeniz gereken her şeyi bulacaksınız.</p>

<h3>1. Ticari Kredi Türleri</h3>
<p>Ticari krediler genellikle şu kategorilere ayrılır:</p>
<ul>
<li>Nakit kredi</li>
<li>İşletme kredisi</li>
<li>Yatırım kredisi</li>
<li>Rotatif kredi</li>
</ul>

<h3>2. En Akıllı Kredi Seçimi</h3>
<p>İşletmeniz için en uygun krediyi seçerken dikkat etmeniz gereken kriterler:</p>
<ul>
<li>Faiz oranları</li>
<li>Vade seçenekleri</li>
<li>Gerekli teminatlar</li>
<li>Esnek ödeme seçenekleri</li>
</ul>

<h3>3. Başvuru Süreci</h3>
<p>Ticari kredi başvurusu için genellikle şu belgeler gerekir:</p>
<ul>
<li>İşletme belgeleri</li>
<li>Mali tablolar</li>
<li>Gelir belgeleri</li>
<li>Teminat belgeleri</li>
</ul>";
    }
}

