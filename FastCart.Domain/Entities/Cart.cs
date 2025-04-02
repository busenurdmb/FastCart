namespace FastCart.Domain.Entities;

//Sepet bir kullanıcının tüm sepetini temsil eder.
public class Cart
{
    public string UserId { get; set; } = default!;
    public List<CartItem> Items { get; set; } = new();
}
