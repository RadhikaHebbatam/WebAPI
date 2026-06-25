using Microsoft.AspNetCore.Mvc;
using ProductCatalogueAPI.Core.Common;
using ProductCatalogueAPI.Core.Interfaces.Services;

namespace ProductCatalogueAPI.Controllers;

/// <summary>
/// WHY ApiController attribute:
/// Enables automatic model validation, binding source inference,
/// and automatic 400 responses for invalid models.
/// Without it you would need to check ModelState manually.
///
/// WHY Route attribute with [controller]:
/// [controller] is replaced by the class name minus "Controller".
/// ProductsController becomes /api/products automatically.
/// Changing the class name changes the route — consistent by convention.
///
/// WHY controllers are thin:
/// Controllers handle HTTP concerns only.
/// Read the request → call the service → return the response.
/// No business logic, no SQL, no validation rules here.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// GET api/products
    /// Returns all active products
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _productService.GetAllProductsAsync();

        // WHY we always return 200 for GetAll even if empty:
        // An empty list is a valid successful response.
        // 404 means the RESOURCE does not exist — the endpoint exists,
        // it just has no data. Return 200 with an empty array.
        return Ok(result.Data);
    }

    /// <summary>
    /// GET api/products/5
    /// Returns a single product by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _productService.GetProductByIdAsync(id);

        // WHY MapToResponse helper:
        // Every controller method needs to map ServiceResult
        // to the correct HTTP response.
        // Centralising this logic in one method means
        // if we change how errors are returned, we change one place.
        return MapToResponse(result);
    }

    /// <summary>
    /// GET api/products/category/3
    /// Returns all products in a category
    /// </summary>
    [HttpGet("category/{categoryId:int}")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        var result = await _productService
            .GetProductsByCategoryAsync(categoryId);

        return MapToResponse(result);
    }

    /// <summary>
    /// POST api/products
    /// Creates a new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var result = await _productService.CreateProductAsync(dto);

        if (!result.IsSuccess)
            return MapToResponse(result);

        // WHY CreatedAtAction:
        // Returns HTTP 201 Created with a Location header
        // pointing to the newly created resource.
        // This is the correct REST response for a POST that creates.
        // Location: /api/products/42
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            result.Data);
    }

    /// <summary>
    /// PUT api/products/5
    /// Updates an existing product
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateProductDto dto)
    {
        var result = await _productService.UpdateProductAsync(id, dto);
        return MapToResponse(result);
    }

    /// <summary>
    /// DELETE api/products/5
    /// Soft deletes a product
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteProductAsync(id);

        if (!result.IsSuccess)
            return MapToResponse(result);

        // WHY 204 No Content:
        // DELETE succeeded but there is nothing to return.
        // 204 communicates success without a response body.
        return NoContent();
    }

    // ── Private Helper ────────────────────────────────────────────────────
    // WHY this helper method:
    // Maps our ServiceResult ErrorCode to the correct HTTP status.
    // Every controller action uses this — consistent error responses
    // across every endpoint with no duplication.
    private IActionResult MapToResponse<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Data);

        return result.ErrorCode switch
        {
            ErrorCode.NotFound => NotFound(new { message = result.ErrorMessage }),
            ErrorCode.Validation => BadRequest(new { message = result.ErrorMessage }),
            ErrorCode.Conflict => Conflict(new { message = result.ErrorMessage }),
            ErrorCode.Unauthorised => Unauthorized(new { message = result.ErrorMessage }),
            ErrorCode.Forbidden => StatusCode(403, new { message = result.ErrorMessage }),
            _ => StatusCode(500, new { message = result.ErrorMessage })
        };
    }

    private IActionResult MapToResponse(ServiceResult result)
    {
        if (result.IsSuccess)
            return Ok();

        return result.ErrorCode switch
        {
            ErrorCode.NotFound => NotFound(new { message = result.ErrorMessage }),
            ErrorCode.Validation => BadRequest(new { message = result.ErrorMessage }),
            ErrorCode.Conflict => Conflict(new { message = result.ErrorMessage }),
            _ => StatusCode(500, new { message = result.ErrorMessage })
        };
    }
}