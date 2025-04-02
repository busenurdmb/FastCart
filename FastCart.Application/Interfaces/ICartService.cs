using FastCart.Domain.Entities;

namespace FastCart.Application.Interfaces;

public interface ICartService
{
    // Belirtilen kullanıcıya ait sepete yeni bir ürün ekler (varsa miktarını artırabilir).
    Task AddToCartAsync(string userId, CartItem item);

    // Belirtilen kullanıcıya ait sepet bilgisini getirir.
    Task<Cart?> GetCartAsync(string userId);

    // Belirtilen kullanıcıya ait sepeti tamamen temizler (tüm ürünleri siler).
    Task ClearCartAsync(string userId);
}

