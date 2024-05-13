using Ledgers_Server_Main.Classes;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Ledgers_Server_Main.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DatabaseController : Controller
    {
        [HttpGet()]
        public ActionResult<string> ReadDatabase()
        {
            try
            {
                var enterprise = new Enterprise();
                return Ok(new
                {
                    funders = enterprise.Funders.Select(item => item.GetJSON()),
                    merchants = enterprise.Merchants.Select(item => item.GetJSON()),
                    owners = enterprise.Owners.Select(item => item.GetJSON())
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}\nSource: {ex.Source}\nTrace: {ex.StackTrace}");
            }
        }

        [HttpPost()]
        public ActionResult<string> InsertDatabase([FromBody] Dictionary<string, Dictionary<string, dynamic>[]> data)
        {
            try
            {

                var enterprise = new Enterprise();
                enterprise.Insert(data);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}\nSource: {ex.Source}\nTrace: {ex.StackTrace}");
            }
        }

        [HttpPut()]
        public ActionResult<string> UpdateDatabase([FromBody] dynamic data)
        {
            try
            {
                var enterprise = new Enterprise();
                enterprise.Insert(data);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}\nSource: {ex.Source}\nTrace: {ex.StackTrace}");
            }
        }

        [HttpDelete("{elements}/{id}")]
        public ActionResult<string> DeleteDatabase(string elements, string id)
        {
            try
            {
                if (id is null) throw new NoNullAllowedException();

                var enterprise = new Enterprise();
                enterprise.Delete(elements, id);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}\nSource: {ex.Source}\nTrace: {ex.StackTrace}");
            }
        }
    }
}
