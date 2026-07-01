using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalogueAPI.Core.Common
{
    public class ProductQueryFilter
    {
        // Price filters
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Stock filters
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }

        // Text search
        public string? NameContains { get; set; }

        // Category
        public int? CategoryId { get; set; }

        // Active status
        public bool? IsActive { get; set; }

        // Sorting
        public string? OrderBy { get; set; }       // "Price", "Name", "StockQuantity"
        public string? OrderDirection { get; set; } // "ASC" or "DESC"
    }
}
