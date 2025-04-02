using FastCart.Application.DTOs;
using FastCart.Application.Interfaces;
using FastCart.Domain.Entities;
using FastCart.Infrastructure.Services.Carts;
using Microsoft.AspNetCore.Mvc;


namespace FastCart.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly CartService _cartService; // ICacheService kullanan
    private readonly DirectRedisCartService _redisCartService; // RedisService kullanan
    private readonly IElasticService _elasticService;
    public CartController(CartService cartService, DirectRedisCartService redisCartService, IElasticService elasticService)
    {
        _cartService = cartService;
        _redisCartService = redisCartService;
        _elasticService = elasticService;
    }
    [HttpGet("search")]
    public async Task<IActionResult> SearchLogs([FromQuery] string productName)
    {
        var result = await _elasticService.SearchLogsByProductNameAsync(productName);
        return Ok(result);
    }


    // ICacheService kullanan(RedisCacheService üzerinden)
    [HttpPost("{userId}/add-cache")]
    public async Task<IActionResult> AddToCartWithCache(string userId, [FromBody] CartItemDto dto)
    {
        var item = new CartItem
        {
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice
        };

        await _cartService.AddToCartAsync(userId, item);
        return Ok("Ürün sepete eklendi (cacheService).");
    }

    // RedisService kullanan (doğrudan)
    [HttpPost("{userId}/add-redis")]
    public async Task<IActionResult> AddToCartWithRedis(string userId, [FromBody] CartItemDto dto)
    {
        var item = new CartItem
        {
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice
        };

        await _redisCartService.AddToCartAsync(userId, item);
        return Ok("Ürün sepete eklendi (redisService).");
    }

    [HttpGet("{userId}/get-cache")]
    public async Task<IActionResult> GetCartFromCache(string userId)
    {
        var cart = await _cartService.GetCartAsync(userId);
        if (cart == null)
            return NotFound("Sepet (cache) bulunamadı.");

        return Ok(cart);
    }

    [HttpGet("{userId}/get-redis")]
    public async Task<IActionResult> GetCartFromRedis(string userId)
    {
        var cart = await _redisCartService.GetCartAsync(userId);
        if (cart == null)
            return NotFound("Sepet (redis) bulunamadı.");

        return Ok(cart);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetCart(string userId)
    {
        var cart = await _cartService.GetCartAsync(userId);
        return Ok(cart);
    }

    [HttpDelete("{userId}/clear")]
    public async Task<IActionResult> ClearCart(string userId)
    {
        await _cartService.ClearCartAsync(userId);
        return Ok("Sepet temizlendi.");
    }
}
