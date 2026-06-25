using ProductCatalogueAPI.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalogueAPI.Core.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();

        Task<Product?> GetByIdAsync(int id);

        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);

        Task<int> CreateAsync(Product product);

        Task UpdateAsync(Product product);

        Task DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}
