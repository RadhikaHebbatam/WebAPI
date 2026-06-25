using ProductCatalogueAPI.Core.Common;

namespace ProductCatalogueAPI.Core.Interfaces.Services
{
    public interface IProductService
    {
        Task<ServiceResult<IEnumerable<ProductDto>>> GetAllProductsAsync();

        Task<ServiceResult<ProductDto>> GetProductByIdAsync(int id);

        Task<ServiceResult<IEnumerable<ProductDto>>> GetProductsByCategoryAsync(int categoryId);

        Task<ServiceResult<ProductDto>> CreateProductAsync(CreateProductDto dto);

        Task<ServiceResult<ProductDto>> UpdateProductAsync(int id, UpdateProductDto dto);

        Task<ServiceResult> DeleteProductAsync(int id);
    }
}
