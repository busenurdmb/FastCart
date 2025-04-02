using FastCart.Application.Interfaces;
using FastCart.Application.Settings;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Reflection;

namespace FastCart.Infrastructure.Pipeline.Cache.Redis;

// Redis sunucusuna bağlanmak ve belirli bir veritabanı üzerinden işlem yapmak için kullanılan servis.
// IRedisService arayüzünden türetilmiştir.
public class RedisConnectionService : IRedisService
{
    private readonly string _host; // Redis sunucusunun IP veya domain adresi (örneğin: localhost)
    private readonly int _port;    // Redis portu (genelde 6379)

    private ConnectionMultiplexer _connectionMultiplexer; // Redis ile bağlantı yöneticisi


    // Host ve port bilgileri constructor aracılığıyla alınır
    public RedisConnectionService(string host, int port)
    {
        _host = host;
        _port = port;
    }

    // Redis sunucusuna bağlantı kurar
    public void Connect()
    {
        _connectionMultiplexer = ConnectionMultiplexer.Connect($"{_host}:{_port}");
    }
    //Redis'e ilk kez bağlantı kuran metottur.
    //Verilen host:port bilgisiyle Redis'e bağlanır.
    //Bağlantı başarılıysa ConnectionMultiplexer nesnesi hazır hale gelir ve üzerinden işlem yapılabilir.
    //🔸 Örn: localhost:6379 → Redis'e yerelde bağlanır.

    // Belirtilen db numarasına göre Redis veritabanı nesnesi döner
    public IDatabase GetDb(int db = 1)
    {
        return _connectionMultiplexer.GetDatabase(db); // ← Buradaki düzeltme önemli
    }
     //Redis içinde birden fazla veritabanı olabilir(db= 0, db= 1, ...).
    //Bu metod, o veritabanına erişim sağlar.
   //Geriye IDatabase nesnesi döner → Bu nesne üzerinden veri set/get işlemleri yapılır.
}

