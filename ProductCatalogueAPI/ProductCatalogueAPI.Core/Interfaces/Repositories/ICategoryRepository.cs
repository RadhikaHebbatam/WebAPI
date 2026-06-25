using ProductCatalogueAPI.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalogueAPI.Core.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();

        Task<Category?> GetByIdAsync(int id);

        Task<int> CreateAsync(Category category);

        Task UpdateAsync(Category category);

        Task DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}
