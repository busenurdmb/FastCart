# 📂 FastCart - .NET 9 + Redis + RabbitMQ + Elasticsearch + Kibana + Serilog Entegrasyonu

Modern mimariyle geliştirilen FastCart projesi, .NET 9 kullanılarak Redis cache, RabbitMQ mesajlaşma, Elasticsearch loglama ve Kibana görselleştirme entegrasyonlarını içeren kapsamlı bir alışveriş sepeti sistemidir.

---

## 🧠 Redis Nedir?

**Redis**, verileri geçici olarak saklayan, çok hızlı çalışan bir **veritabanıdır**. Ama klasik SQL ya da NoSQL veritabanlarından farklı olarak:

- Bellek (RAM) üzerinde çalışır
- Disk yerine RAM kullandığı için milisaniyelik hızda veri okuma/yazma sağlar
- Genellikle cache (önbellekleme), gerçek zamanlı veri yönetimi ve geçici veri saklama için kullanılır

**Ne zaman kullanılır?**
- Oturum (session) verisi tutmak
- Ziyaretçi sayaçları
- Sepet/favori listesi gibi kullanıcıya özel geçici veriler

Bu projede Redis şu şekilde kullanıldı:
- Her kullanıcı için `cart:{userId}` formatında eşsiz Redis anahtarları oluşturuldu
- Sepet verileri Redis'e yazıldı, 2 saat süreyle saklandı (TTL özelliği)
- Gerçek zamanlı, hızlı ve geçici bir yapı sağlandı

---

## 🧪 Unit Test Nedir?

**Unit Test**, bir uygulamanın en küçük parçalarının (örneğin bir fonksiyon, bir servis) doğru çalışıp çalışmadığını test eden otomatik kontrollerdir.

**Neden önemlidir?**
- Yazdığın kodun doğru çalışıp çalışmadığını kontrol eder
- Kod değişikliklerinde hata oluşup oluşmadığını erken tespit etmeyi sağlar
- Geliştirme sürecinde güven kazandırır ve hata ayıklamayı kolaylaştırır

**Örnek:**
Bir `Topla(a, b)` fonksiyonu varsa, `Topla(2, 3)` sonucunun 5 olduğunu test eden bir Unit Test yazılır.

Bu projede `CartService` için xUnit kullanılarak yazılmış Unit Test senaryoları şunları kontrol eder:
- ✅ Yeni ürün sepete eklendiğinde doğru şekilde ekleniyor mu?
- 🔁 Aynı ürün tekrar eklenirse miktarı artırılıyor mu?
- 🧹 Sepet temizlendiğinde Redis’ten siliniyor mu?

---
## 🎯 Amaç

Bu proje, aşağıdaki teknolojileri öğrenmek ve entegre şekilde kullanmak için geliştirilmiştir:

- ☁️ Redis ile in-memory sepet yönetimi
- 🐇 RabbitMQ ile mesaj kuyruğu yapısı
- 🔍 Elasticsearch & 📊 Kibana ile log analizi
- 🢾 Serilog ile çoklu hedefe loglama
- ✅ xUnit ile Unit Test altyapısı

---
![Kibana index](https://github.com/busenurdmb/FastCart/blob/master/image/redisekleme.png)
![Kibana index](https://github.com/busenurdmb/FastCart/blob/master/image/getcachepng.png)
![Kibana index](https://github.com/busenurdmb/FastCart/blob/master/image/redisdelete.png)
![Kibana index](https://github.com/busenurdmb/FastCart/blob/master/image/elasticsearch.png)
![Kibana index](https://github.com/busenurdmb/FastCart/blob/master/image/rabbitmq.png)
![Kibana index](https://github.com/busenurdmb/FastCart/blob/master/image/Test.png)



## 🧱 Proje Katmanları

```
FastCart/
├── FastCart.API              → Web API giriş katmanı
├── FastCart.Application      → Uygulama servisleri ve Interface katmanı
├── FastCart.Domain           → Core varlıklar (Entities)
├── FastCart.Infrastructure   → Redis, RabbitMQ, Serilog servisleri
├── FastCart.Consumer         → RabbitMQ'dan mesaj okuyan Worker Service
├── FastCart.Tests.Unit       → xUnit ile yazılmış testler
└── docker-compose.yml        → Redis, Elasticsearch, RabbitMQ servis tanımı
```

---

## 🛠️ Kullanılan Teknolojiler

| Teknoloji         | Açıklama                                       |
|------------------|------------------------------------------------|
| ⚙️ .NET 9         | Web API ve Worker katmanı                      |
| 🐇 RabbitMQ       | Sipariş mesajlarını kuyrukta tutar             |
| 🨠 Redis          | Sepet cache işlemleri için in-memory yapı     |
| 🢾 Serilog        | Loglama altyapısı (Dosya + Elasticsearch)      |
| 🔍 Elasticsearch  | Logların indekslenmesi                         |
| 📊 Kibana         | Logların görselleştirilmesi                    |
| ✅ xUnit          | Birim test altyapısı                           |
| 🐳 Docker         | Geliştirme ortamı container olarak ayağa kalkar|

---

## 📅 Kurulum ve Çalıştırma

```bash
git clone https://github.com/kullaniciadi/FastCart.git
cd FastCart

docker-compose up -d # Docker servislerini başlat

dotnet run --project FastCart.API # API'yi çalıştır
```

---

## 🔀 Docker Servisleri

```yaml
version: "3.7"
services:
  redis:
    image: redis:7
    container_name: redis-demo
    ports:
      - "6380:6379"
    networks:
      - app-net

  rabbitmq:
    image: rabbitmq:3-management
    container_name: rabbitmq-docker
    ports:
      - "5673:5672"
      - "15673:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    networks:
      - app-net

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:7.17.0
    container_name: elasticsearch
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
    ports:
      - "9200:9200"
    networks:
      - app-net

  kibana:
    image: docker.elastic.co/kibana/kibana:7.17.0
    container_name: kibana
    ports:
      - "5601:5601"
    depends_on:
      - elasticsearch
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    networks:
      - app-net

networks:
  app-net:
```

---

## 💡 Sepet İşlemleri (API)

| HTTP | Endpoint                      | Açıklama                    |
|------|-------------------------------|-----------------------------|
| GET  | /api/cart/{userId}/get-cache  | Redis’ten sepet getirir     |
| POST | /api/cart/{userId}/add-cache  | Sepete ürün ekler           |
| DELETE | /api/cart/{userId}/clear    | Sepeti temizler             |

---

## 🧏‍♂️ Serilog Loglama
Bu projede loglar iki farklı şekilde toplanmaktadır:

-Genel loglar: Tüm servislerden gelen loglar Logs/log-.txt dosyasına günlük olarak yazılır.
-CartService'e özel loglar: Yalnızca CartService içindeki loglar hem ayrı bir dosyaya (Logs/Carts/cartlog-.txt) hem de Elasticsearch’e gönderilir.

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()

    // Genel log dosyası
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)

    // CartService'e özel loglar: Hem dosyaya hem Elasticsearch'e
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.FromSource("FastCart.Infrastructure.Services.Carts.CartService"))
        .WriteTo.File("Logs/Carts/cartlog-.txt", rollingInterval: RollingInterval.Day)
        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
        {
            AutoRegisterTemplate = true,
            IndexFormat = "fastcart-logs-{0:yyyy.MM.dd}",
            CustomFormatter = new SimpleElasticsearchFormatter()
        }))
    .CreateLogger();
```

### Örnek Kibana Log

```json
{
  "@timestamp": "2025-04-03T12:38:36.484Z",
  "UserId": "user7",
  "ProductName": "Vurun Kahbeye",
  "Quantity": 1,
  "UnitPrice": 280,
  "Total": 280,
  "SourceContext": "FastCart.Infrastructure.Services.Carts.CartService",
  "message": "🛒 Sipariş Detayı: Kullanıcı=user7, Ürün=Vurun Kahbeye, Adet=1, Fiyat=280, Toplam=280"
}
```

---

## ✅ Unit Test Altyapısı

- `FastCart.Tests.Unit` projesi xUnit ile yapılandırılmıştır.
- `CartServiceTests.cs` içinde `AddToCartAsync`, `GetCartAsync`, `ClearCartAsync` gibi metotlar test edilmiştir.
- Mock altyapısı için `Moq` kütüphanesi kullanılmıştır.

---

## 🎨 Kibana ve RabbitMQ Panelleri

| Kibana Dashboard                             | RabbitMQ Yönetim Paneli                    |
|----------------------------------------------|--------------------------------------------|
| ![Kibana](https://github.com/busenurdmb/FastCart/blob/master/image/e.png) | ![RabbitMQ](https://github.com/busenurdmb/FastCart/blob/master/image/rabbitmq1.png) |

---

## 💪 Setup Komutları (CLI çalışmak için)

```bash
mkdir FastCart
cd FastCart
dotnet new sln -n FastCart

dotnet new webapi -n FastCart.API
dotnet new classlib -n FastCart.Application
dotnet new classlib -n FastCart.Infrastructure
dotnet new classlib -n FastCart.Domain

dotnet sln add FastCart.API/FastCart.API.csproj
dotnet sln add FastCart.Application/FastCart.Application.csproj
dotnet sln add FastCart.Infrastructure/FastCart.Infrastructure.csproj
dotnet sln add FastCart.Domain/FastCart.Domain.csproj

dotnet add FastCart.API reference FastCart.Application
dotnet add FastCart.Application reference FastCart.Infrastructure
dotnet add FastCart.Application reference FastCart.Domain
dotnet add FastCart.Infrastructure reference FastCart.Domain

# Test projesi (opsiyonel)
dotnet new xunit -n FastCart.Tests.Unit
dotnet sln add FastCart.Tests.Unit/FastCart.Tests.Unit.csproj
dotnet add FastCart.Tests.Unit reference FastCart.Application
dotnet add FastCart.Tests.Unit reference FastCart.Infrastructure
dotnet add FastCart.Tests.Unit reference FastCart.Domain
```

---

## 📥 Örnek Sipariş (POST)

```json
POST /api/cart/a101/add-cache
{
  "productId": "kalem123",
  "productName": "Kalem",
  "quantity": 2,
  "unitPrice": 10
}
```

---

## 🧠 Katkıda Bulunmak

Projeye katkı sağlamak isterseniz PR gönderebilir veya Issue oluşturabilirsiniz!

> Hazırlayan: [@busenurdmb](https://github.com/busenurdmb)  
> Lisans: MIT





