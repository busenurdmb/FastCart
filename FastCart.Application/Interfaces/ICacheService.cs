namespace FastCart.Application.Interfaces;

// Bu arayüz, cache işlemleri için bir soyutlama sağlar.
// Redis gibi sistemler yerine MemoryCache gibi başka servislerle de uyumlu çalışmasını mümkün kılar.
// SOLID prensiplerinden "Interface Segregation" ve "Dependency Inversion" ilkelerine uygundur.

public interface ICacheService
{
    // Belirtilen 'key' adıyla generic bir veriyi cache'e ekler.
    // Optional 'expiry' parametresiyle verinin ne kadar süreyle tutulacağı ayarlanabilir.
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    // Belirtilen 'key' ile daha önce cache'e eklenmiş veriyi okur.
    // Bulunamazsa 'null' döner.
    Task<T?> GetAsync<T>(string key);

    // Belirtilen 'key' ile cache'te bulunan veriyi siler.
    Task RemoveAsync(string key);
}


//Bu arayüz, tüm cache işlemleri için tek giriş noktası sağlar.

//Redis’ten bağımsız çalışır → testte farklı cache(Memory, InMemory) kullanılabilir.

//SOLID → Interface Segregation + Dependency Inversion uygulanır.
