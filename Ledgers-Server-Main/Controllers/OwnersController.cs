using Ledgers_Server_Main.Classes;
using Microsoft.AspNetCore.Mvc;

namespace Ledgers_Server_Main.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OwnersController : Controller
    {
        [HttpGet()]
        public ActionResult<string> Read()
        {
            try
            {
                var enterprise = new Enterprise();
                return Ok(enterprise.Owners.Select(item => item.GetJSON()));
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}\nSource: {ex.Source}\nTrace: {ex.StackTrace}");
            }
        }
    }
}
