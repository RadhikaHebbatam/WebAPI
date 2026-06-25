namespace ProductCatalogueAPI.Core.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        public int CategoryId { get; set; }

        // WHY string.Empty default: avoids null reference exceptions.
        // In .NET 8 with nullable reference types enabled,
        // we initialise strings to avoid compiler warnings.
        public string CategoryName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
