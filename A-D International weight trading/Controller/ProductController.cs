using A_D_International_weight_trading.Data;
using A_D_International_weight_trading.Dtos.Products;
using A_D_International_weight_trading.Model;
using A_D_International_weight_trading.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace A_D_International_weight_trading
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(AppDbContext context, ICloudinaryService cloudinaryService, ILogger<ProductController> logger)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductListDto>>> GetProducts(
            [FromQuery] string search = "",
            [FromQuery] int? categoryId = null,
            [FromQuery] string status = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Category)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Name.Contains(search) ||
                                           p.Description.Contains(search) ||
                                           p.Category.Name.Contains(search));
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(p => p.Status == status);
                }

                var totalCount = await query.CountAsync();
                var products = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProductListDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.Name,
                        Status = p.Status,
                        Standards = p.Standards,
                        CreatedAt = p.CreatedAt,
                        ImageCount = p.Images.Count
                    })
                    .ToListAsync();

                Response.Headers.Add("X-Total-Count", totalCount.ToString());
                Response.Headers.Add("X-Page", page.ToString());
                Response.Headers.Add("X-Page-Size", pageSize.ToString());

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, "An error occurred while retrieving products");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                    return NotFound($"Product with ID {id} not found");

                var productDto = new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category.Name,
                    Specifications = product.Specifications,
                    Status = product.Status,
                    Standards = product.Standards,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                    Images = product.Images.Select(img => new ProductImageDto
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        CreatedAt = img.CreatedAt
                    }).ToList()
                };

                return Ok(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID {ProductId}", id);
                return StatusCode(500, "An error occurred while retrieving the product");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> CreateProduct([FromForm] CreateProductDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Verify category exists and is active
                var category = await _context.Categories.FindAsync(createDto.CategoryId);
                if (category == null || category.Status != "active")
                    return BadRequest("Invalid or inactive category");

                var product = new Product
                {
                    Name = createDto.Name.Trim(),
                    Description = createDto.Description.Trim(),
                    CategoryId = createDto.CategoryId,
                    Specifications = createDto.Specifications?.Trim(),
                    Status = createDto.Status,
                    Standards = createDto.Standards?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                if (createDto.Images?.Any() == true)
                {
                    var uploadResults = await _cloudinaryService.UploadMultipleImagesAsync(createDto.Images);

                    var productImages = uploadResults.Select(result => new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = result.imageUrl,
                        PublicId = result.publicId,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    _context.ProductImages.AddRange(productImages);
                    await _context.SaveChangesAsync();

                    product.Images = productImages;
                }

                await transaction.CommitAsync();

                // Load the category for response
                await _context.Entry(product)
                    .Reference(p => p.Category)
                    .LoadAsync();

                var responseDto = new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category.Name,
                    Specifications = product.Specifications,
                    Status = product.Status,
                    Standards = product.Standards,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                    Images = product.Images.Select(img => new ProductImageDto
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        CreatedAt = img.CreatedAt
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, responseDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, "An error occurred while creating the product");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductResponseDto>> UpdateProduct(int id, [FromForm] UpdateProductDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var product = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                    return NotFound($"Product with ID {id} not found");

                var category = await _context.Categories.FindAsync(updateDto.CategoryId);
                if (category == null || category.Status != "active")
                    return BadRequest("Invalid or inactive category");

                product.Name = updateDto.Name.Trim();
                product.Description = updateDto.Description.Trim();
                product.CategoryId = updateDto.CategoryId;
                product.Specifications = updateDto.Specifications?.Trim();
                product.Status = updateDto.Status;
                product.Standards = updateDto.Standards?.Trim();
                product.UpdatedAt = DateTime.UtcNow;

                if (updateDto.RemoveImageIds?.Any() == true)
                {
                    var imagesToRemove = product.Images
                        .Where(img => updateDto.RemoveImageIds.Contains(img.Id))
                        .ToList();

                    foreach (var image in imagesToRemove)
                    {
                        await _cloudinaryService.DeleteImageAsync(image.PublicId);
                        _context.ProductImages.Remove(image);
                    }
                }

                if (updateDto.NewImages?.Any() == true)
                {
                    var currentImageCount = product.Images.Count - (updateDto.RemoveImageIds?.Count ?? 0);
                    var maxNewImages = 7 - currentImageCount;

                    if (updateDto.NewImages.Count > maxNewImages)
                        return BadRequest($"Cannot add {updateDto.NewImages.Count} images. Maximum {maxNewImages} more images allowed.");

                    var uploadResults = await _cloudinaryService.UploadMultipleImagesAsync(updateDto.NewImages);

                    var newProductImages = uploadResults.Select(result => new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = result.imageUrl,
                        PublicId = result.publicId,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    _context.ProductImages.AddRange(newProductImages);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                product = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                var responseDto = new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category.Name,
                    Specifications = product.Specifications,
                    Status = product.Status,
                    Standards = product.Standards,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                    Images = product.Images.Select(img => new ProductImageDto
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        CreatedAt = img.CreatedAt
                    }).ToList()
                };

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating product with ID {ProductId}", id);
                return StatusCode(500, "An error occurred while updating the product");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var product = await _context.Products
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                    return NotFound($"Product with ID {id} not found");

                foreach (var image in product.Images)
                {
                    await _cloudinaryService.DeleteImageAsync(image.PublicId);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
                return StatusCode(500, "An error occurred while deleting the product");
            }
        }

        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => c.Status == "active")
                    .Select(c => new { Id = c.Id, Name = c.Name })
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, "An error occurred while retrieving categories");
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetStats()
        {
            try
            {
                var totalProducts = await _context.Products.CountAsync();
                var activeProducts = await _context.Products.CountAsync(p => p.Status == "active");
                var categories = await _context.Categories.CountAsync(c => c.Status == "active");

                return Ok(new
                {
                    TotalProducts = totalProducts,
                    ActiveProducts = activeProducts,
                    Categories = categories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stats");
                return StatusCode(500, "An error occurred while retrieving stats");
            }
        }
    }
}