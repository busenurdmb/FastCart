using FastCart.Application.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;


namespace FastCart.Infrastructure.Pipeline.Cache.Redis;

//Bu sınıf, uygulamada verileri Redis'e key-value şeklinde saklamak, okumak ve silmek için kullanılır.
// Redis tabanlı önbellekleme servisi
public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;

    // Redis bağlantısı constructor üzerinden alınır ve database nesnesi oluşturulur
    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase(); // Redis'in default veritabanı (db=0)
    }

    // Belirtilen anahtar ile Redis'e veri yazma işlemi yapılır
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonConvert.SerializeObject(value); // Nesne JSON'a çevrilir
        await _db.StringSetAsync(key, json, expiry);   // 📌 Redis'e key-value olarak yazar
    }

    // Belirtilen anahtara göre Redis'ten veri okunur ve ilgili tipe deserialize edilir
    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await _db.StringGetAsync(key); //📌 Key ile Redis'ten veri alınır
        return string.IsNullOrEmpty(json)
            ? default
            : JsonConvert.DeserializeObject<T>(json!); // JSON tekrar objeye dönüştürülür
    }

    // Belirtilen anahtarı Redis'ten siler
    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
}

//📌 Ek Bilgiler:

//StringSetAsync: Redis’e string veri yazmak için kullanılır.

//StringGetAsync: Redis’ten string veri almak için kullanılır.

//JsonConvert.SerializeObject: C# objesini string’e çevirir.

//JsonConvert.DeserializeObject: string’i C# objesine geri çevirir.

//IConnectionMultiplexer: Redis ile bağlantıyı yöneten yapı (StackExchange.Redis).

//IDatabase: Redis veritabanı işlemlerini yapan interface (Redis aslında birden fazla DB içerebilir ama genellikle db= 0 kullanılır).
//Redis’e veri yazmak için StringSetAsync, veri almak için StringGetAsync kullanılır.

//Her şey JSON olarak saklanır, çünkü Redis veri tabanı gibi değil → sadece string saklar.

// IDatabase objesi ConnectionMultiplexer üzerinden gelir (StackExchange.Redis kütüphanesi).