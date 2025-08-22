
using Microsoft.AspNetCore.Identity;

namespace A_D_International_weight_trading.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }



}
