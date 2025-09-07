using A_D_International_weight_trading.Data;
using A_D_International_weight_trading.Dtos.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace A_D_International_weight_trading.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator")]

    public class MetaDataController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MetaDataController> _logger;

        public MetaDataController(AppDbContext context, ILogger<MetaDataController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("public")]
        public async Task<ActionResult<PublicMetaDataDto>> GetPublicMetaData()
        {
            try
            {
                var metaData = await _context.MetaData.FirstOrDefaultAsync();

                if (metaData == null)
                {
                    return NotFound("Business information not found.");
                }

                var publicDto = new PublicMetaDataDto
                {
                    Name = metaData.CompanyName,
                    Address = metaData.Address,
                    Phone = metaData.Phone,
                    Email = metaData.Email,
                    Hours = metaData.Hours,
                    Coordinates = new CoordinatesDto
                    {
                        Lat = metaData.Latitude,
                        Lng = metaData.Longitude
                    },
                    MapEmbedUrl = metaData.MapEmbedUrl,
                    LocationDescription = metaData.LocationDescription,
                    DirectionsUrl = metaData.DirectionsUrl,
                    PhoneCallUrl = metaData.PhoneCallUrl,
                    EmailUrl = metaData.EmailUrl
                };

                return Ok(publicDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public metadata");
                return StatusCode(500, "An error occurred while retrieving business information.");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MetaDataDto>>> GetMetaData()
        {
            try
            {
                var metaDataList = await _context.MetaData
                    .Select(m => new MetaDataDto
                    {
                        Id = m.Id,
                        CompanyName = m.CompanyName,
                        Address = m.Address,
                        Phone = m.Phone,
                        Email = m.Email,
                        Hours = m.Hours,
                        Latitude = m.Latitude,
                        Longitude = m.Longitude,
                        MapEmbedUrl = m.MapEmbedUrl,
                        LocationDescription = m.LocationDescription,
                        CreatedAt = m.CreatedAt,
                        UpdatedAt = m.UpdatedAt,
                        DirectionsUrl = m.DirectionsUrl,
                        PhoneCallUrl = m.PhoneCallUrl,
                        EmailUrl = m.EmailUrl
                    })
                    .ToListAsync();

                return Ok(metaDataList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving metadata");
                return StatusCode(500, "An error occurred while retrieving metadata.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MetaDataDto>> GetMetaData(int id)
        {
            try
            {
                var metaData = await _context.MetaData.FindAsync(id);

                if (metaData == null)
                {
                    return NotFound();
                }

                var dto = new MetaDataDto
                {
                    Id = metaData.Id,
                    CompanyName = metaData.CompanyName,
                    Address = metaData.Address,
                    Phone = metaData.Phone,
                    Email = metaData.Email,
                    Hours = metaData.Hours,
                    Latitude = metaData.Latitude,
                    Longitude = metaData.Longitude,
                    MapEmbedUrl = metaData.MapEmbedUrl,
                    LocationDescription = metaData.LocationDescription,
                    CreatedAt = metaData.CreatedAt,
                    UpdatedAt = metaData.UpdatedAt,
                    DirectionsUrl = metaData.DirectionsUrl,
                    PhoneCallUrl = metaData.PhoneCallUrl,
                    EmailUrl = metaData.EmailUrl
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving metadata with id {Id}", id);
                return StatusCode(500, "An error occurred while retrieving metadata.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMetaData(int id, UpdateMetaDataDto updateDto)
        {
            try
            {
                var metaData = await _context.MetaData.FindAsync(id);
                if (metaData == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(updateDto.CompanyName))
                    metaData.CompanyName = updateDto.CompanyName;

                if (!string.IsNullOrEmpty(updateDto.Address))
                    metaData.Address = updateDto.Address;

                if (!string.IsNullOrEmpty(updateDto.Phone))
                    metaData.Phone = updateDto.Phone;

                if (!string.IsNullOrEmpty(updateDto.Email))
                    metaData.Email = updateDto.Email;

                if (!string.IsNullOrEmpty(updateDto.Hours))
                    metaData.Hours = updateDto.Hours;

                if (updateDto.Latitude.HasValue)
                    metaData.Latitude = updateDto.Latitude.Value;

                if (updateDto.Longitude.HasValue)
                    metaData.Longitude = updateDto.Longitude.Value;

                if (!string.IsNullOrEmpty(updateDto.MapEmbedUrl))
                    metaData.MapEmbedUrl = updateDto.MapEmbedUrl;

                if (updateDto.LocationDescription != null)
                    metaData.LocationDescription = updateDto.LocationDescription;

                metaData.UpdatedAt = DateTime.UtcNow;

                _context.Entry(metaData).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating metadata with id {Id}", id);
                return StatusCode(500, "An error occurred while updating metadata.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<MetaDataDto>> PostMetaData(CreateMetaDataDto createDto)
        {
            try
            {
                var metaData = new Model.MetaData
                {
                    CompanyName = createDto.CompanyName,
                    Address = createDto.Address,
                    Phone = createDto.Phone,
                    Email = createDto.Email,
                    Hours = createDto.Hours,
                    Latitude = createDto.Latitude,
                    Longitude = createDto.Longitude,
                    MapEmbedUrl = createDto.MapEmbedUrl,
                    LocationDescription = createDto.LocationDescription
                };

                _context.MetaData.Add(metaData);
                await _context.SaveChangesAsync();

                var dto = new MetaDataDto
                {
                    Id = metaData.Id,
                    CompanyName = metaData.CompanyName,
                    Address = metaData.Address,
                    Phone = metaData.Phone,
                    Email = metaData.Email,
                    Hours = metaData.Hours,
                    Latitude = metaData.Latitude,
                    Longitude = metaData.Longitude,
                    MapEmbedUrl = metaData.MapEmbedUrl,
                    LocationDescription = metaData.LocationDescription,
                    CreatedAt = metaData.CreatedAt,
                    UpdatedAt = metaData.UpdatedAt,
                    DirectionsUrl = metaData.DirectionsUrl,
                    PhoneCallUrl = metaData.PhoneCallUrl,
                    EmailUrl = metaData.EmailUrl
                };

                return CreatedAtAction(nameof(GetMetaData), new { id = metaData.Id }, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating metadata");
                return StatusCode(500, "An error occurred while creating metadata.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMetaData(int id)
        {
            try
            {
                var metaData = await _context.MetaData.FindAsync(id);
                if (metaData == null)
                {
                    return NotFound();
                }

                _context.MetaData.Remove(metaData);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting metadata with id {Id}", id);
                return StatusCode(500, "An error occurred while deleting metadata.");
            }
        }

        [HttpGet("current")]
        [AllowAnonymous]
        public async Task<ActionResult<PublicMetaDataDto>> GetCurrentBusinessInfo()
        {
            try
            {
                var metaData = await _context.MetaData
                    .OrderByDescending(m => m.UpdatedAt ?? m.CreatedAt)
                    .FirstOrDefaultAsync();

                if (metaData == null)
                {
                    return NotFound("Business information not configured.");
                }

                var publicDto = new PublicMetaDataDto
                {
                    Name = metaData.CompanyName,
                    Address = metaData.Address,
                    Phone = metaData.Phone,
                    Email = metaData.Email,
                    Hours = metaData.Hours,
                    Coordinates = new CoordinatesDto
                    {
                        Lat = metaData.Latitude,
                        Lng = metaData.Longitude
                    },
                    MapEmbedUrl = metaData.MapEmbedUrl,
                    LocationDescription = metaData.LocationDescription,
                    DirectionsUrl = metaData.DirectionsUrl,
                    PhoneCallUrl = metaData.PhoneCallUrl,
                    EmailUrl = metaData.EmailUrl
                };

                return Ok(publicDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current business info");
                return StatusCode(500, "An error occurred while retrieving business information.");
            }
        }
    }
}
