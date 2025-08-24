using A_D_International_weight_trading.Data;
using A_D_International_weight_trading.Dtos.Catagory;
using A_D_International_weight_trading.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace A_D_International_weight_trading.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator")]

    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(AppDbContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories(
            [FromQuery] string search = "",
            [FromQuery] string status = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Categories.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(c => c.Name.Contains(search) ||
                                           c.Description.Contains(search));
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(c => c.Status == status);
                }

                var totalCount = await query.CountAsync();
                var categories = await query
                    .OrderBy(c => c.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Status = c.Status,
                        CreatedAt = c.CreatedAt,
                        ProductCount = c.Products.Count
                    })
                    .ToListAsync();

                Response.Headers.Add("X-Total-Count", totalCount.ToString());
                Response.Headers.Add("X-Page", page.ToString());
                Response.Headers.Add("X-Page-Size", pageSize.ToString());

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, "An error occurred while retrieving categories");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                    return NotFound($"Category with ID {id} not found");

                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    Status = category.Status,
                    CreatedAt = category.CreatedAt,
                    ProductCount = category.Products.Count
                };

                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID {CategoryId}", id);
                return StatusCode(500, "An error occurred while retrieving the category");
            }
        }

        [HttpPost]
        [AllowAnonymous]

        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == createDto.Name.ToLower().Trim());

                if (existingCategory != null)
                    return BadRequest("A category with this name already exists");

                var category = new Category
                {
                    Name = createDto.Name.Trim(),
                    Description = createDto.Description?.Trim(),
                    Status = createDto.Status,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var responseDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    Status = category.Status,
                    CreatedAt = category.CreatedAt,
                    ProductCount = 0
                };

                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, "An error occurred while creating the category");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, UpdateCategoryDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                    return NotFound($"Category with ID {id} not found");

                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id != id &&
                                           c.Name.ToLower() == updateDto.Name.ToLower().Trim());

                if (existingCategory != null)
                    return BadRequest("A category with this name already exists");

                category.Name = updateDto.Name.Trim();
                category.Description = updateDto.Description?.Trim();
                category.Status = updateDto.Status;
                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var responseDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    Status = category.Status,
                    CreatedAt = category.CreatedAt,
                    ProductCount = category.Products.Count
                };

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID {CategoryId}", id);
                return StatusCode(500, "An error occurred while updating the category");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .ThenInclude(p => p.Images)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                    return NotFound($"Category with ID {id} not found");

                // Check if category has products
                if (category.Products.Any())
                {
                    return BadRequest("Cannot delete category that contains products. Please move or delete all products first.");
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting category with ID {CategoryId}", id);
                return StatusCode(500, "An error occurred while deleting the category");
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetCategoryStats()
        {
            try
            {
                var totalCategories = await _context.Categories.CountAsync();
                var activeCategories = await _context.Categories.CountAsync(c => c.Status == "active");
                var categoriesWithProducts = await _context.Categories
                    .CountAsync(c => c.Products.Any());

                var topCategories = await _context.Categories
                    .Include(c => c.Products)
                    .Where(c => c.Status == "active")
                    .OrderByDescending(c => c.Products.Count)
                    .Take(5)
                    .Select(c => new
                    {
                        Name = c.Name,
                        ProductCount = c.Products.Count
                    })
                    .ToListAsync();

                return Ok(new
                {
                    TotalCategories = totalCategories,
                    ActiveCategories = activeCategories,
                    CategoriesWithProducts = categoriesWithProducts,
                    TopCategories = topCategories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category stats");
                return StatusCode(500, "An error occurred while retrieving category stats");
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<CategoryDto>> UpdateCategoryStatus(int id, [FromBody] string status)
        {
            try
            {
                if (string.IsNullOrEmpty(status))
                    return BadRequest("Status is required");

                var validStatuses = new[] { "active", "inactive" };
                if (!validStatuses.Contains(status.ToLower()))
                    return BadRequest("Status must be either 'active' or 'inactive'");

                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                    return NotFound($"Category with ID {id} not found");

                // If deactivating category, check if it has active products
                if (status.ToLower() == "inactive" && category.Products.Any(p => p.Status == "active"))
                {
                    return BadRequest("Cannot deactivate category that contains active products. Please deactivate all products first.");
                }

                category.Status = status.ToLower();
                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var responseDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    Status = category.Status,
                    CreatedAt = category.CreatedAt,
                    ProductCount = category.Products.Count
                };

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category status with ID {CategoryId}", id);
                return StatusCode(500, "An error occurred while updating the category status");
            }
        }
    }
}
