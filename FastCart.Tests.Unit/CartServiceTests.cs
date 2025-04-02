using FastCart.Domain.Entities;
using FastCart.Infrastructure.Services.Carts;
using FastCart.Application.Interfaces;
using Moq;
using Microsoft.Extensions.Logging;

namespace FastCart.Tests.Unit;

public class CartServiceTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<CartService>> _loggerMock;
    private readonly Mock<IRabbitMqService> _rabbitMqServiceMock;
    private readonly CartService _cartService;

    public CartServiceTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<CartService>>();
        _rabbitMqServiceMock = new Mock<IRabbitMqService>();

        _cartService = new CartService(
       _cacheServiceMock.Object,
       _loggerMock.Object,
       _rabbitMqServiceMock.Object // 👈 buraya da parametre olarak ver
   );
        
    }

    [Fact]
    public async Task AddToCartAsync_ShouldAddNewItem_WhenCartIsEmpty()
    {
        // Arrange
        var userId = "testUser";
        var product = new CartItem { ProductId = "p1", ProductName = "Kalem", Quantity = 1, UnitPrice = 5 };

        _cacheServiceMock.Setup(x => x.GetAsync<Cart>($"cart:{userId}"))
                         .ReturnsAsync((Cart?)null); // Sepet boş

        // Act
        await _cartService.AddToCartAsync(userId, product);

        // Assert
        _cacheServiceMock.Verify(x => x.SetAsync($"cart:{userId}",
            It.Is<Cart>(c => c.Items.Count == 1 && c.Items[0].ProductId == "p1"),
            It.IsAny<TimeSpan>()), Times.Once);
    }
}

//🔍 Açıklama:
//Kısım Anlamı
//Mock<ICacheService> Gerçek ICacheService yerine sahte/mock nesne kullanılır
//Setup(...).ReturnsAsync(...)    Mock nesneye, bir metot çağrıldığında ne döneceğini söyler
//Verify(...) Metodun doğru çağrılıp çağrılmadığını kontrol eder
//Fact Bu testin çalıştırılabilir bir test olduğunu belirtir