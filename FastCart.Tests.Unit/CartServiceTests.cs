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
       _rabbitMqServiceMock.Object 
   );
        
    }
    // ✅ Sepet boşsa, ürün ilk defa ekleniyorsa → sepete yeni ürün eklenmeli
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

    // ✅ Sepette zaten aynı ürün varsa → sadece adeti artırılmalı
    [Fact]
    public async Task AddToCartAsync_ShouldIncreaseQuantity_WhenItemAlreadyExists()
    {
        // Arrange
        var userId = "testUser";
        var existingItem = new CartItem
        {
            ProductId = "p1",
            ProductName = "Kalem",
            Quantity = 1,
            UnitPrice = 5
        };

        var existingCart = new Cart
        {
            UserId = userId,
            Items = new List<CartItem> { existingItem }
        };

        var newItem = new CartItem
        {
            ProductId = "p1", // aynı ürün
            ProductName = "Kalem",
            Quantity = 2,     // yeni gelen adet
            UnitPrice = 5
        };

        _cacheServiceMock.Setup(x => x.GetAsync<Cart>($"cart:{userId}"))
                         .ReturnsAsync(existingCart);

        // Act
        await _cartService.AddToCartAsync(userId, newItem);

        // Assert
        _cacheServiceMock.Verify(x => x.SetAsync(
            $"cart:{userId}",
            It.Is<Cart>(c =>
                c.Items.Count == 1 &&
                c.Items[0].ProductId == "p1" &&
                c.Items[0].Quantity == 3 // ✅ 1 + 2 = 3
            ),
            It.IsAny<TimeSpan>()
        ), Times.Once);
    }

    // ✅ Kullanıcının sepeti varsa → doğru şekilde getirilmeli
    [Fact]
    public async Task GetCartAsync_ShouldReturnCart_WhenExists()
    {
        // Arrange
        var userId = "testUser";
        var expectedCart = new Cart
        {
            UserId = userId,
            Items = new List<CartItem>
        {
            new CartItem { ProductId = "p1", ProductName = "Kalem", Quantity = 2, UnitPrice = 5 }
        }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<Cart>($"cart:{userId}"))
                         .ReturnsAsync(expectedCart);

        // Act
        var result = await _cartService.GetCartAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.Items.Count);
        Assert.Equal("p1", result.Items[0].ProductId);
    }

    // ✅ Kullanıcının sepeti silinmek istendiğinde → Redis’ten key silinmeli
    [Fact]
    public async Task ClearCartAsync_ShouldRemoveCartKey_FromCache()
    {
        // Arrange
        var userId = "testUser";

        // Act
        await _cartService.ClearCartAsync(userId);

        // Assert
        _cacheServiceMock.Verify(x => x.RemoveAsync($"cart:{userId}"), Times.Once);
    }

}

//🔍 Açıklama:
//💬 Temel Test Terimleri ve Açıklamaları
//Terim   Açıklama Örnek
//Test Metodu Kodun belirli bir parçasını test eden fonksiyon AddToCartAsync_ShouldAddNewItem
//[Fact]  Bu bir testtir! diye işaretler  xUnit’te her testin başına yazılır
//Arrange Test için gerekli veriler hazırlanır Sahte kullanıcı, ürün vs.oluşturma
//Act Test etmek istediğin metodu çağırırsın  AddToCartAsync(userId, item)
//Assert  Sonuçlar beklediğin gibi mi kontrol edilir  Sepette 1 ürün var mı?
//Mock    Gerçek servis yerine sahte versiyonunu kullanma Redis yerine Moq<ICacheService>
//Verify  Mock'lanmış fonksiyon gerçekten çağrıldı mı kontrolü	SetAsync gerçekten çalıştı mı?