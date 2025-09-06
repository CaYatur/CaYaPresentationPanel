# CaYa Gösterim Paneli

Genel amaç: sahne, etkinlik veya gösterimlerde kolayca medya çalmak, ekranlara içerik göstermek ve uzaktan kontrol etmek için hazırlanmış, bir Windows programıdır.

Öne çıkan özellikler
- Ses, video, resim, GIF ve sunum dosyalarını oynatma.
- Yerel ağ üzerinden basit web arayüzüyle uzaktan kontrol (QR kod ile hızlı bağlantı).
- Her medya öğesi için ayrı ses düzeyi ayarı.
- Eğer ses kablosu (ör. virtual audio cable veya line-in) takılıysa, sadece bu panelde çalan sesleri o kablo üzerinden mikrofona aktarabilme.
- Katmanlı tam ekran gösterim: bir ekranda arka plan koyma, metin ve görseller ekleyip düzenleyebilme (taşıma, yeniden boyutlandırma, katman sırası).
- F2 tuşu ile program durumu görüntüleme: ses aygıtı, ekran bağlantı ve çözünürlük sorunlarını tespit etme ve basit düzeltmeler.
- Anlık ses görselleştirici (ses yüksekliği / frekans bazlı).
- Basit video ve ses düzenleme araçları (kırpma, kesme, birleştirme gibi temel işlemler).
- Çoklu ekran desteği: hangi ekranda göstereceğinizi seçebilirsiniz.
- Program tek dosya (single exe) olarak dağıtılmaya uygundur — kurulum gerektirmez.


Sistem gereksinimleri
- Windows 10 veya Windows 11
- .NET 8 çalıştırma zamanı (program tek dosya exe olarak paketlenirse kullanıcıların ek bir şey yapması genelde gerekmez)


## Kullanıcılar için Hızlı Başlangıç
> 1. Programı indirin (tek exe dosyası).
> 2. Yeni bir klasör oluşturun (örneğin "CaYaPanel") ve indirdiğiniz exe'yi bu klasöre koyun.
>   - ÖNEMLİ: Programı çalıştırmadan önce yeni klasör oluşturup exe'yi o klasörün içinden başlatmanızı kesinlikle öneririz. Program başlangıçta kendi yapılandırma dosyalarını (dataCY.json vb.) ve medya önbelleklerini çalıştığı klasöre oluşturur.
> 3. Exe'ye çift tıklayarak başlatın. Web kontrolü için yönetici izni istenirse onaylayabilirsiniz (kabul etmek genelde daha sorunsuz bağlantı sağlar).
> 4. Program otomatik olarak dataCY.json dosyasını oluşturur. İlk açılışta boş bir liste olacak; dosyayı manuel düzenlemenize gerek yok — içerikleri program içinden ekleyin.
> 5. Medya eklemek için arayüzdeki "Ekle" düğmelerini kullanın; her öğe için açıklama ve ses düzeyi ayarı yapılabilir.
> 6. Uzaktan kontrolü açmak isterseniz "Uzaktan Kontrol" bölümünden sunucuyu başlatın; aynı ağdaki cihazlarla QR kod veya adresle bağlanabilirsiniz.
> 7. Katmanlı gösterimler için "Arkaplan / Yazı Göster" düzenleyicisini kullanın, hedef ekranı seçin ve "Tam Ekran Göster" ile yayınlayın.


Güvenlik ve ağ
- Dahili web sunucusu basit kontrol amaçlıdır. Yerel ağda kullanırken güvenlik duvarı ayarlarınıza dikkat edin.
- Eğer programı halka açık bir ağa bağlamak istemiyorsanız, sadece güvenilir yerel ağlarda kullanın.

## Yardım ve destek
- Program içinde sık kullanılan yardım ipuçları ve sorun giderme bölümü bulunmaktadır. Sorun yaşarsanız öncelikle F2 ile durum bölümünü kontrol edin. Eğer sorunlarınız devam ederse lütfen Forum sayfamızdan sorununuzla ilgili talepte bulunun.


## Notlar
- Bu programın çalışması için .NET 8.0 gerekmektedir. (Programı açtıktan sonra yoksa indirmek için size otomatik şekilde sorar ve indirdikten sonra kurmanız gerekir.)
