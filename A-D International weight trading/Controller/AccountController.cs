using A_D_International_weight_trading.Model;
using A_D_International_weight_trading.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace A_D_International_weight_trading.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtHelper _jwtHelper;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, JwtHelper jwtHelper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtHelper = jwtHelper;
        }

        [HttpPost("register")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Register([FromBody] Register model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUserByUsername = await _userManager.FindByNameAsync(model.Username);
            if (existingUserByUsername != null)
            {
                return BadRequest(new { Message = "Username already exists." });
            }

            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                return BadRequest(new { Message = "Email already exists." });
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }
                await _userManager.AddToRoleAsync(user, "User");
                return Ok(new { 
                    Message = "User registered successfully.",
                    UserId = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ApplicationUser user = null;

            if (!string.IsNullOrEmpty(model.Email))
            {
                user = await _userManager.FindByEmailAsync(model.Email);
            }
            else if (!string.IsNullOrEmpty(model.Username))
            {
                user = await _userManager.FindByNameAsync(model.Username);
            }
            else
            {
                return BadRequest(new { Message = "Either email or username must be provided." });
            }
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized(new { Message = "Invalid login attempt." });
            }
            var token = await _jwtHelper.GenerateJwtTokenAsync(user);
            return Ok(new { Token = token });
        }

        [HttpPost("change-role")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangeRole([FromBody] UserRole model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                return BadRequest(new { Message = $"Role '{model.Role}' does not exist." });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return BadRequest(new { Message = "Failed to remove current roles.", Errors = removeResult.Errors });
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!addResult.Succeeded)
            {
                return BadRequest(new { Message = "Failed to assign new role.", Errors = addResult.Errors });
            }

            return Ok(new { Message = $"User role changed to '{model.Role}' successfully." });
        }

        [HttpGet("user-roles/{username}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetUserRoles(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new { Username = username, Roles = roles });
        }
    }
}