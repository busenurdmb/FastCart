using FastCart.Application.Interfaces;
using FastCart.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FastCart.Infrastructure.Services.Carts;

public class CartService : ICartService
{
    private readonly ICacheService _cacheService; // Redis cache servisi
    private readonly ILogger<CartService> _logger;
    private readonly IRabbitMqService _rabbitMqService;
    private const string CartKeyPrefix = "cart:"; // Redis key prefix (her kullanıcıya özel sepet verisi için)

    // ICacheService DI üzerinden alınır
    public CartService(ICacheService cacheService, ILogger<CartService> logger, IRabbitMqService rabbitMqService)
    {
        _cacheService = cacheService;
        _logger = logger;
        _rabbitMqService = rabbitMqService;
    }

    // Belirtilen kullanıcıya ait sepete ürün ekler
    public async Task AddToCartAsync(string userId, CartItem item)
    {
        var key = $"{CartKeyPrefix}{userId}"; // Redis key: cart:kullaniciId

        // Mevcut sepet Redis’ten alınır, yoksa yeni sepet oluşturulur
        var cart = await _cacheService.GetAsync<Cart>(key) ?? new Cart { UserId = userId };

        // Sepette aynı ürün varsa miktarını artır, yoksa ürünü sepete ekle
        var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == item.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            cart.Items.Add(item);
        }

        // Sepet Redis’e 2 saat süreyle kaydedilir
        await _cacheService.SetAsync(key, cart, TimeSpan.FromHours(2));
        _rabbitMqService.Publish("cart-queue", new
        {
            UserId = userId,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity
        });
        _logger.LogInformation("🛒 Sipariş Detayı: Kullanıcı={UserId}, Ürün={ProductName}, Adet={Quantity}, Fiyat={UnitPrice}, Toplam={Total}",
    userId,
    item.ProductName,
    item.Quantity,
    item.UnitPrice,
    item.Quantity * item.UnitPrice);

        //    _logger.LogInformation("🛒 Sepet güncellendi - Kullanıcı: {UserId}, Ürün: {ProductId}, Adet: {Quantity}",
        //userId, item.ProductId, item.Quantity);
        //    _logger.LogInformation("🛒 Sipariş logu: {@OrderDetails}", new
        //    {
        //        OrderProduct = item.ProductName,
        //        OrderQuantity = item.Quantity,
        //        OrderUserId = userId
        //    });
        //    _logger.LogInformation("🧪 TEST - CartService'ten log geldi.");
        //    _logger.LogInformation("🔍 SOURCE CONTEXT: {Source}", typeof(CartService).FullName);

    }

    // Kullanıcının sepetini Redis’ten getirir
    public async Task<Cart?> GetCartAsync(string userId)
    {
        var cart = await _cacheService.GetAsync<Cart>($"{CartKeyPrefix}{userId}");
        if (cart != null && cart.Items.Any())
        {
            _logger.LogInformation("📥 Sepet getirildi - Kullanıcı: {UserId}, Ürün Sayısı: {ItemCount}",
                userId, cart.Items.Count);
        }
        else
        {
            _logger.LogWarning("⚠️ Kullanıcının sepeti boş veya bulunamadı - Kullanıcı: {UserId}", userId);
        }
        return cart;
    }

    // Kullanıcının sepetini tamamen temizler (Redis’ten siler)
    public async Task ClearCartAsync(string userId)
    {
        await _cacheService.RemoveAsync($"{CartKeyPrefix}{userId}");
        _logger.LogWarning("🗑️ Sepet temizlendi - Kullanıcı: {UserId}", userId);
    }
}

//📦 Notlar:
//CartKeyPrefix ile her kullanıcı için eşsiz Redis anahtarı(cart:kullaniciId) üretiliyor.

//ICacheService, Redis işlemlerini soyutlar(bu da test edilebilirliği artırır).

//Bu yapı sayesinde sepet işlemleri tamamen in-memory Redis cache üzerinde yürür.