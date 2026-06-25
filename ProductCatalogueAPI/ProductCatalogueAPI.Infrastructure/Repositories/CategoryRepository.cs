using Dapper;
using ProductCatalogueAPI.Core.Entities;
using ProductCatalogueAPI.Core.Interfaces.Repositories;
using ProductCatalogueAPI.Infrastructure.Data;


namespace ProductCatalogueAPI.Infrastructure.Repositories
{
    public class CategoryRepository :ICategoryRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CategoryRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            const string sql = """
            SELECT
                Id,
                Name,
                Description,
                IsActive,
                CreatedAt
            FROM Categories
            WHERE IsActive = 1
            ORDER BY Name
            """;

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Category>(sql);
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            const string sql = """
            SELECT
                Id,
                Name,
                Description,
                IsActive,
                CreatedAt
            FROM Categories
            WHERE Id = @Id
            """;

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Category>(
                sql, new { Id = id });
        }

        public async Task<int> CreateAsync(Category category)
        {
            const string sql = """
            INSERT INTO Categories
                (Name, Description, IsActive, CreatedAt)
            VALUES
                (@Name, @Description, @IsActive, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, category);
        }

        public async Task UpdateAsync(Category category)
        {
            const string sql = """
            UPDATE Categories SET
                Name        = @Name,
                Description = @Description,
                IsActive    = @IsActive
            WHERE Id = @Id
            """;

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, category);
        }

        public async Task DeleteAsync(int id)
        {
            // WHY soft delete here too:
            // Categories may have products linked to them.
            // Hard deleting a category with products would either
            // fail due to foreign key constraints or orphan products.
            // Soft delete keeps the data intact and consistent.
            const string sql = """
            UPDATE Categories
            SET IsActive = 0
            WHERE Id = @Id
            """;

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string sql = """
            SELECT COUNT(1)
            FROM Categories
            WHERE Id = @Id AND IsActive = 1
            """;

            using var connection = _connectionFactory.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }
    }
}
