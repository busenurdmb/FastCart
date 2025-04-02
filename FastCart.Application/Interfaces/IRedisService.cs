using StackExchange.Redis;

namespace FastCart.Application.Interfaces;

public interface IRedisService
{
    // Redis sunucusuna bağlantı kurar.
    void Connect();

    // Belirtilen veritabanı numarasına göre Redis veritabanını döner.
    // Varsayılan olarak db=1 kullanılır.
    IDatabase GetDb(int db = 1);
}

