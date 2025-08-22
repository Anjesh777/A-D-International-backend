using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace A_D_International_weight_trading.Controller
{
    [Authorize(Roles = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
     
        [HttpGet]
        public IActionResult GetAdminData()
        {
            return Ok(new { Message = "This is admin data. Auth success" });
        }



    }
}
