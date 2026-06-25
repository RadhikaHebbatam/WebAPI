using Microsoft.Extensions.Logging;
using ProductCatalogueAPI.Core.Common;
using ProductCatalogueAPI.Core.Entities;
using ProductCatalogueAPI.Core.Interfaces.Repositories;
using ProductCatalogueAPI.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalogueAPI.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ILogger<ProductService> logger
            )
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        private static ProductDto MapToDto(Product product) => new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = product.CategoryName,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };

        public async Task<ServiceResult<IEnumerable<ProductDto>>> GetAllProductsAsync()
        {
            _logger.LogInformation("Retrieving all active products");

            var products = await _productRepository.GetAllAsync();
            var productDtos = products.Select(MapToDto).ToList();

            _logger.LogInformation("Retrieved {Count} products", productDtos.Count);

            return ServiceResult<IEnumerable<ProductDto>>.Success(productDtos);
        }
        public async Task<ServiceResult<ProductDto>> GetProductByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving product with ID {ProductId}", id);

            var product = await _productRepository.GetByIdAsync(id);

            if (product is null)
            {
                _logger.LogWarning("Product with ID {ProductId} was not found", id);
                return ServiceResult<ProductDto>.Failure(
                    $"Product with ID {id} was not found.",
                    ErrorCode.NotFound);
            }

            return ServiceResult<ProductDto>.Success(MapToDto(product));
        }

        public async Task<ServiceResult<IEnumerable<ProductDto>>> GetProductsByCategoryAsync(int categoryId)
        {
            _logger.LogInformation(
                "Retrieving products for category {CategoryId}", categoryId);

            var categoryExists = await _categoryRepository.ExistsAsync(categoryId);
            if (!categoryExists)
            {
                return ServiceResult<IEnumerable<ProductDto>>.Failure(
                    $"Category with ID {categoryId} was not found.",
                    ErrorCode.NotFound);
            }

            var products = await _productRepository.GetByCategoryAsync(categoryId);
            var productDtos = products.Select(MapToDto).ToList();

            return ServiceResult<IEnumerable<ProductDto>>.Success(productDtos);
        }

        public async Task<ServiceResult<ProductDto>> CreateProductAsync(CreateProductDto dto)
        {
            _logger.LogInformation("Creating new product: {ProductName}", dto.Name);

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return ServiceResult<ProductDto>.Failure(
                    "Product name is required.",
                    ErrorCode.Validation);
            }

            if (dto.Price < 0)
            {
                return ServiceResult<ProductDto>.Failure(
                    "Product price cannot be negative.",
                    ErrorCode.Validation);
            }

            if (dto.StockQuantity < 0)
            {
                return ServiceResult<ProductDto>.Failure(
                    "Stock quantity cannot be negative.",
                    ErrorCode.Validation);
            }

            var categoryExists = await _categoryRepository.ExistsAsync(dto.CategoryId);
            if (!categoryExists)
            {
                return ServiceResult<ProductDto>.Failure(
                    $"Category with ID {dto.CategoryId} does not exist.",
                    ErrorCode.Validation);
            }

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var newId = await _productRepository.CreateAsync(product);
            product.Id = newId;

            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            product.CategoryName = category?.Name ?? string.Empty;

            _logger.LogInformation(
                "Product created successfully with ID {ProductId}", newId);

            return ServiceResult<ProductDto>.Success(MapToDto(product));
        }

        public async Task<ServiceResult<ProductDto>> UpdateProductAsync(
        int id,
        UpdateProductDto dto)
        {
            _logger.LogInformation("Updating product with ID {ProductId}", id);

            var existing = await _productRepository.GetByIdAsync(id);
            if (existing is null)
            {
                return ServiceResult<ProductDto>.Failure(
                    $"Product with ID {id} was not found.",
                    ErrorCode.NotFound);
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return ServiceResult<ProductDto>.Failure(
                    "Product name is required.",
                    ErrorCode.Validation);
            }

            if (dto.Price < 0)
            {
                return ServiceResult<ProductDto>.Failure(
                    "Product price cannot be negative.",
                    ErrorCode.Validation);
            }

            if (dto.StockQuantity < 0)
            {
                return ServiceResult<ProductDto>.Failure(
                    "Stock quantity cannot be negative.",
                    ErrorCode.Validation);
            }

            if (dto.CategoryId != existing.CategoryId)
            {
                var categoryExists = await _categoryRepository.ExistsAsync(dto.CategoryId);
                if (!categoryExists)
                {
                    return ServiceResult<ProductDto>.Failure(
                        $"Category with ID {dto.CategoryId} does not exist.",
                        ErrorCode.Validation);
                }
            }

            existing.Name = dto.Name.Trim();
            existing.Description = dto.Description?.Trim() ?? string.Empty;
            existing.Price = dto.Price;
            existing.StockQuantity = dto.StockQuantity;
            existing.CategoryId = dto.CategoryId;
            existing.IsActive = dto.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(existing);

            var category = await _categoryRepository.GetByIdAsync(existing.CategoryId);
            existing.CategoryName = category?.Name ?? string.Empty;

            _logger.LogInformation("Product {ProductId} updated successfully", id);

            return ServiceResult<ProductDto>.Success(MapToDto(existing));
        }

        public async Task<ServiceResult> DeleteProductAsync(int id)
        {
            _logger.LogInformation("Deleting product with ID {ProductId}", id);

            var exists = await _productRepository.ExistsAsync(id);
            if (!exists)
            {
                return ServiceResult.Failure(
                    $"Product with ID {id} was not found.",
                    ErrorCode.NotFound);
            }

            await _productRepository.DeleteAsync(id);

            _logger.LogInformation("Product {ProductId} deleted successfully", id);

            return ServiceResult.Success();
        }
    }
}
