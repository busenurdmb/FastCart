using StackExchange.Redis;
using Newtonsoft.Json;
using FastCart.Application.Interfaces;
using FastCart.Domain.Entities;

public class DirectRedisCartService : ICartService
{
    private readonly IDatabase _db;
    private const string CartKeyPrefix = "cart:";

    // IRedisService üzerinden Redis veritabanı alınır
    public DirectRedisCartService(IRedisService redisService)
    {
        _db = redisService.GetDb(); // db=1 default olarak geliyor
    }

    public async Task AddToCartAsync(string userId, CartItem item)
    {
        var key = $"{CartKeyPrefix}{userId}";

        // Redis’ten mevcut sepeti getir (yoksa yeni oluştur)
        var json = await _db.StringGetAsync(key);
        Cart cart = string.IsNullOrEmpty(json)
            ? new Cart { UserId = userId }
            : JsonConvert.DeserializeObject<Cart>(json)!;

        // Aynı ürün varsa miktarını artır, yoksa ürünü ekle
        var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == item.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            cart.Items.Add(item);
        }

        // Güncellenmiş sepeti Redis’e 2 saatlik TTL ile kaydet
        var updatedJson = JsonConvert.SerializeObject(cart);
        await _db.StringSetAsync(key, updatedJson, TimeSpan.FromHours(2));
    }

    public async Task<Cart?> GetCartAsync(string userId)
    {
        var key = $"{CartKeyPrefix}{userId}";
        var json = await _db.StringGetAsync(key);
        return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<Cart>(json!);
    }

    public async Task ClearCartAsync(string userId)
    {
        await _db.KeyDeleteAsync($"{CartKeyPrefix}{userId}");
    }
}
