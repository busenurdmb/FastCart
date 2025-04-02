namespace FastCart.Application.Interfaces;

public interface IElasticService
{
    Task<List<object>> SearchLogsByProductNameAsync(string productName);
}
