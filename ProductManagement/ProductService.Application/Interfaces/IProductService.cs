using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsForUserAsync(int userId);
        Task<Product?> GetProductByIdAsync(int id, int userId);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product, int userId);
        Task DeleteProductAsync(int id, int userId);
        Task<IEnumerable<Product>> SearchProductsAsync(int userId, string? searchQuery,
            decimal? minPrice, decimal? maxPrice, bool? isAvailable);
    }

}