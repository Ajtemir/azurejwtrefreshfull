using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTechJwt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NamesController : ControllerBase
    {
        [Authorize]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetNames()
        {
            var names = await Task.FromResult(new List<string> {"Adam", "Robert"});
            return Ok(names);
        }
    }
}