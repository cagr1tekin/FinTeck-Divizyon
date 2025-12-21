# **Kredi İzleme ve Ödeme Performansı Veri Dokümantasyonu**

Bu döküman, KKB (Kredi Kayıt Bürosu) entegrasyonu üzerinden gelen verilerin parametre karşılıklarını, görselleştirme kurallarını ve veri formatlama standartlarını tanımlar.

---

### **1\. Ürün Tip Kodları (bkKrediTuru)**

Sistemden gelen sayısal veya kısa kodların hangi kredi ürününü temsil ettiğini belirtir:

| Parametre Değeri | Ürün Açıklaması |
| :---- | :---- |
| **23** | Kredi Kartı |
| **02** | İhtiyaç Kredisi |
| **03** | Konut Kredisi |
| **TK** | Taşıt Kredisi |
| **26** | KMH (Kredili Mevduat Hesabı) |

---

### **2\. Ödeme Performans Göstergeleri**

Müşterinin ödeme alışkanlıkları ve geçmiş risk analizi bu parametrelerle takip edilir.

* **Tarihsel En Kötü Durum (bkWorstPaymetStatusEver):** Müşterinin tüm kredi geçmişi boyunca ulaştığı en yüksek gecikme seviyesini ifade eder (Örn: "3" değeri, 3 dönemlik bir gecikmenin geçmişte yaşandığını gösterir).  
* **Aylık Ödeme Performans Dizisi (bkOdemePerformansiTarihcesi):** Her bir karakter bir ayı temsil eder. Dizinin en sağı en güncel ayı gösterir.

**Durum Kodları ve UI (Arayüz) Karşılıkları:**

* **Başarılı ('0'):** Ödeme zamanında yapılmıştır.  
  * *Görünüm:* Yeşil Renk \+ Check İkonu (✅)  
* **Gecikme ('K' veya 0'dan büyük bir sayı):** Ödemede gecikme yaşanmıştır. Rakamlar gecikme periyodunu/ay sayısını temsil eder.  
  * *Görünüm:* Kırmızı Renk \+ Çarpı İkonu (❌)  
* **Bilinmiyor ('X'):** İşlem yapılmamış, veri henüz oluşmamış veya bilgi eksik.  
  * *Görünüm:* Gri Renk \+ Soru İşareti İkonu (❓)

  ---

### **3\. Risk ve Limit Detay Parametreleri**

Kredi bazlı finansal durumu anlamak için kullanılan temel metrikler:

* **Toplam Limit (bkToplamLimit):** Müşterinin tanımlı tüm kredi sınırlarının toplamı.  
* **Toplam Risk (bkToplamRisk):** Müşterinin güncel borç ve risk bakiyesi toplamı.  
* **Gecikmedeki Bakiye (bkGecikmedekiBakiye):** İlgili kredi kaleminde ödemesi sarkan güncel tutar.  
* **Toplam Geciktirilmiş Ödeme Sayısı (bkToplamGeciktirilmisOdemeSayisi):** Kredinin açılışından itibaren kaç kez gecikme yaşandığı.  
* **Limit Kullanım Oranı (bkLimitKullanimOrani):** Mevcut limitin ne kadarının kullanıldığını gösteren rasyo (Örn: 0.07 \= %7).  
  ---

### **4\. Sistem ve Kayıt Referansları**

Veritabanı eşleşmeleri ve teknik takip için kullanılan kimlik bilgileri:

* **Rapor Detay No:** reportDetailsId  
* **Müşteri No:** customerId  
* **Bireysel Tekil No:** bireyselId  
* **Kayıt Referans No:** bkKayitReferansNo (KKB tarafındaki benzersiz işlem numarası).  
* **Varlık Türü:** varlikTuru (1: Bireysel, 2: Ticari gibi ayrımı belirtir).  
  ---

### **5\. Veri Formatlama Standartları**

Tarih Formatı (bkAcilisTarihi, bkKapanisTarihi, bkEnSonGuncellemeTarihi)

Sistemden gelen tarihler bitişik nizamda YYYYMMDD (YılAyGün) formatındadır. Kullanıcıya gösterilmeden önce formatlanmalıdır.

* **Ham Veri:** 20250123  
* **Formatlanmış Çıktı:** 23.01.2025

**Döviz Kodu (bkDovizKodu)**

* Bakiyelerin hangi para birimi üzerinden okunması gerektiğini belirtir (Örn: "TL").


  
