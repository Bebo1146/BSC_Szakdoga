using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.Identity
{
    [ApiController]
    [Route("api/identity")]
    [Authorize]
    internal class IdentityController
    {
    }
}
