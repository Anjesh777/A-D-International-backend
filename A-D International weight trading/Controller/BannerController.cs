using A_D_International_weight_trading.Data;
using A_D_International_weight_trading.Dtos.Banner;
using A_D_International_weight_trading.Model;
using A_D_International_weight_trading.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace A_D_International_weight_trading
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<BannerController> _logger;

        public BannerController(AppDbContext context, ICloudinaryService cloudinaryService, ILogger<BannerController> logger)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PublicBannerDto>>> GetPublicBanners()
        {
            try
            {
                var now = DateTime.UtcNow;
                var banners = await _context.Banners
                    .Where(b => b.IsActive &&
                               b.Status == "active" &&
                               b.StartDate <= now &&
                               (b.EndDate == null || b.EndDate >= now))
                    .OrderBy(b => b.DisplayOrder)
                    .ThenByDescending(b => b.CreatedAt)
                    .Select(b => new PublicBannerDto
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Subtitle = b.Subtitle,
                        ImageUrl = b.ImageUrl,
                        ButtonText = b.ButtonText,
                        LinkType = b.LinkType,
                        ProductId = b.ProductId,
                        CategoryId = b.CategoryId,
                        ExternalUrl = b.ExternalUrl,
                        DisplayOrder = b.DisplayOrder
                    })
                    .ToListAsync();

                return Ok(banners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public banners");
                return StatusCode(500, "An error occurred while retrieving banners");
            }
        }

        // Admin endpoints - require authorization
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<IEnumerable<BannerListDto>>> GetBanners(
            [FromQuery] string status = "",
            [FromQuery] string linkType = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Banners
                    .Include(b => b.Product)
                    .Include(b => b.Category)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(b => b.Status == status);
                }

                if (!string.IsNullOrEmpty(linkType))
                {
                    query = query.Where(b => b.LinkType == linkType);
                }

                var totalCount = await query.CountAsync();
                var banners = await query
                    .OrderBy(b => b.DisplayOrder)
                    .ThenByDescending(b => b.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(b => new BannerListDto
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Subtitle = b.Subtitle,
                        ImageUrl = b.ImageUrl,
                        ButtonText = b.ButtonText,
                        LinkType = b.LinkType,
                        ExternalUrl = b.ExternalUrl,
                        Status = b.Status,
                        DisplayOrder = b.DisplayOrder,
                        IsActive = b.IsActive,
                        StartDate = b.StartDate,
                        EndDate = b.EndDate,
                        CreatedAt = b.CreatedAt
                    })
                    .ToListAsync();

                Response.Headers.Add("X-Total-Count", totalCount.ToString());
                Response.Headers.Add("X-Page", page.ToString());
                Response.Headers.Add("X-Page-Size", pageSize.ToString());

                return Ok(banners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving banners");
                return StatusCode(500, "An error occurred while retrieving banners");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<BannerResponseDto>> GetBanner(int id)
        {
            try
            {
                var banner = await _context.Banners
                    .Include(b => b.Product)
                    .Include(b => b.Category)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (banner == null)
                    return NotFound($"Banner with ID {id} not found");

                var bannerDto = new BannerResponseDto
                {
                    Id = banner.Id,
                    Title = banner.Title,
                    Subtitle = banner.Subtitle,
                    ImageUrl = banner.ImageUrl,
                    ButtonText = banner.ButtonText,
                    LinkType = banner.LinkType,
                    ProductId = banner.ProductId,
                    ProductName = banner.Product?.Name,
                    ExternalUrl = banner.ExternalUrl,
                    Status = banner.Status,
                    DisplayOrder = banner.DisplayOrder,
                    IsActive = banner.IsActive,
                    StartDate = banner.StartDate,
                    EndDate = banner.EndDate,
                    CreatedAt = banner.CreatedAt,
                    UpdatedAt = banner.UpdatedAt
                };

                return Ok(bannerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving banner with ID {BannerId}", id);
                return StatusCode(500, "An error occurred while retrieving the banner");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<BannerResponseDto>> CreateBanner([FromForm] CreateBannerDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate link type specific requirements
            var validationResult = await ValidateBannerLinkType(createDto.LinkType, createDto.ProductId, createDto.CategoryId, createDto.ExternalUrl);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Upload image to Cloudinary
                var uploadResult = await _cloudinaryService.UploadImageAsync(createDto.Image);

                var banner = new Banner
                {
                    Title = createDto.Title.Trim(),
                    Subtitle = createDto.Subtitle?.Trim(),
                    ImageUrl = uploadResult.imageUrl,
                    PublicId = uploadResult.publicId,
                    ButtonText = createDto.ButtonText?.Trim() ?? "Shop Now",
                    LinkType = createDto.LinkType,
                    ProductId = createDto.ProductId,
                    CategoryId = createDto.CategoryId,
                    ExternalUrl = createDto.ExternalUrl?.Trim(),
                    Status = createDto.Status,
                    DisplayOrder = createDto.DisplayOrder,
                    IsActive = createDto.IsActive,
                    StartDate = createDto.StartDate,
                    EndDate = createDto.EndDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Banners.Add(banner);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Load related entities for response
                await _context.Entry(banner)
                    .Reference(b => b.Product)
                    .LoadAsync();
                await _context.Entry(banner)
                    .Reference(b => b.Category)
                    .LoadAsync();

                var responseDto = new BannerResponseDto
                {
                    Id = banner.Id,
                    Title = banner.Title,
                    Subtitle = banner.Subtitle,
                    ImageUrl = banner.ImageUrl,
                    ButtonText = banner.ButtonText,
                    LinkType = banner.LinkType,
                    ProductId = banner.ProductId,
                    ProductName = banner.Product?.Name,
                    ExternalUrl = banner.ExternalUrl,
                    Status = banner.Status,
                    DisplayOrder = banner.DisplayOrder,
                    IsActive = banner.IsActive,
                    StartDate = banner.StartDate,
                    EndDate = banner.EndDate,
                    CreatedAt = banner.CreatedAt,
                    UpdatedAt = banner.UpdatedAt
                };

                return CreatedAtAction(nameof(GetBanner), new { id = banner.Id }, responseDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating banner");
                return StatusCode(500, "An error occurred while creating the banner");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<BannerResponseDto>> UpdateBanner(int id, [FromForm] UpdateBannerDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validationResult = await ValidateBannerLinkType(updateDto.LinkType, updateDto.ProductId, updateDto.CategoryId, updateDto.ExternalUrl);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var banner = await _context.Banners
                    .Include(b => b.Product)
                    .Include(b => b.Category)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (banner == null)
                    return NotFound($"Banner with ID {id} not found");

                // Update image if provided
                if (updateDto.Image != null)
                {
                    // Delete old image
                    if (!string.IsNullOrEmpty(banner.PublicId))
                    {
                        await _cloudinaryService.DeleteImageAsync(banner.PublicId);
                    }

                    // Upload new image
                    var uploadResult = await _cloudinaryService.UploadImageAsync(updateDto.Image);
                    banner.ImageUrl = uploadResult.imageUrl;
                    banner.PublicId = uploadResult.publicId;
                }

                banner.Title = updateDto.Title.Trim();
                banner.Subtitle = updateDto.Subtitle?.Trim();
                banner.ButtonText = updateDto.ButtonText?.Trim() ?? "Shop Now";
                banner.LinkType = updateDto.LinkType;
                banner.ProductId = updateDto.ProductId;
                banner.CategoryId = updateDto.CategoryId;
                banner.ExternalUrl = updateDto.ExternalUrl?.Trim();
                banner.Status = updateDto.Status;
                banner.DisplayOrder = updateDto.DisplayOrder;
                banner.IsActive = updateDto.IsActive;
                banner.StartDate = updateDto.StartDate;
                banner.EndDate = updateDto.EndDate;
                banner.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Reload to get updated related entities
                banner = await _context.Banners
                    .Include(b => b.Product)
                    .Include(b => b.Category)
                    .FirstOrDefaultAsync(b => b.Id == id);

                var responseDto = new BannerResponseDto
                {
                    Id = banner.Id,
                    Title = banner.Title,
                    Subtitle = banner.Subtitle,
                    ImageUrl = banner.ImageUrl,
                    ButtonText = banner.ButtonText,
                    LinkType = banner.LinkType,
                    ProductId = banner.ProductId,
                    ProductName = banner.Product?.Name,
                    ExternalUrl = banner.ExternalUrl,
                    Status = banner.Status,
                    DisplayOrder = banner.DisplayOrder,
                    IsActive = banner.IsActive,
                    StartDate = banner.StartDate,
                    EndDate = banner.EndDate,
                    CreatedAt = banner.CreatedAt,
                    UpdatedAt = banner.UpdatedAt
                };

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating banner with ID {BannerId}", id);
                return StatusCode(500, "An error occurred while updating the banner");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var banner = await _context.Banners.FindAsync(id);

                if (banner == null)
                    return NotFound($"Banner with ID {id} not found");

                // Delete image from Cloudinary
                if (!string.IsNullOrEmpty(banner.PublicId))
                {
                    await _cloudinaryService.DeleteImageAsync(banner.PublicId);
                }

                _context.Banners.Remove(banner);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting banner with ID {BannerId}", id);
                return StatusCode(500, "An error occurred while deleting the banner");
            }
        }

        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ToggleBannerStatus(int id)
        {
            try
            {
                var banner = await _context.Banners.FindAsync(id);

                if (banner == null)
                    return NotFound($"Banner with ID {id} not found");

                banner.IsActive = !banner.IsActive;
                banner.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { IsActive = banner.IsActive });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling banner status for ID {BannerId}", id);
                return StatusCode(500, "An error occurred while toggling banner status");
            }
        }

        private async Task<(bool IsValid, string ErrorMessage)> ValidateBannerLinkType(string linkType, int? productId, int? categoryId, string externalUrl)
        {
            switch (linkType?.ToLower())
            {
                case "product":
                    if (!productId.HasValue)
                        return (false, "Product ID is required for product link type");

                    var product = await _context.Products.FindAsync(productId.Value);
                    if (product == null || product.Status != "active")
                        return (false, "Invalid or inactive product");
                    break;

                case "category":
                    if (!categoryId.HasValue)
                        return (false, "Category ID is required for category link type");

                    var category = await _context.Categories.FindAsync(categoryId.Value);
                    if (category == null || category.Status != "active")
                        return (false, "Invalid or inactive category");
                    break;

                case "external":
                    if (string.IsNullOrEmpty(externalUrl))
                        return (false, "External URL is required for external link type");

                    if (!Uri.TryCreate(externalUrl, UriKind.Absolute, out _))
                        return (false, "Invalid external URL format");
                    break;

                case "all-products":
                    break;

                default:
                    return (false, "Invalid link type. Supported types: product, category, external, all-products");
            }

            return (true, string.Empty);
        }
    }
}