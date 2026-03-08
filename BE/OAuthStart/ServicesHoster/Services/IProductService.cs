namespace ServicesHoster.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(string id);
        Task AddRangeAsync(IEnumerable<ProductDto> products, string userId);
        Task<IEnumerable<ProductDto>> GetByUserAsync(string userId);
    }
}