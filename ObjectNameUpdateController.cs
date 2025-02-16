using Microsoft.AspNetCore.Mvc;
using SpaceTrack.Services;

namespace SpaceTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObjectNameUpdateController : ControllerBase
    {
        private readonly ObjectNameUpdateService _objectNameUpdateService;

        public ObjectNameUpdateController(ObjectNameUpdateService objectNameUpdateService)
        {
            _objectNameUpdateService = objectNameUpdateService;
        }

        [HttpPut("update-object-names")]
        public async Task<IActionResult> UpdateObjectNames()
        {
            try
            {
                await _objectNameUpdateService.UpdateObjectNamesAsync();
                return Ok(new { Message = "Object names updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while updating object names.",
                    Error = ex.Message
                });
            }
        }
    }
}

/*
using Microsoft.AspNetCore.Mvc;
using SpaceTrack.Services;
using System.Threading.Tasks;

namespace SpaceTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObjectNameUpdateController : ControllerBase
    {
        private readonly ObjectNameUpdateService _objectNameUpdateService;

        public ObjectNameUpdateController(ObjectNameUpdateService objectNameUpdateService)
        {
            _objectNameUpdateService = objectNameUpdateService;
        }

        /// <summary>
        /// Endpoint to trigger the update of object names in the database.
        /// </summary>
        /// <returns>A status message indicating success or failure.</returns>
        [HttpPost("update-object-names")]
        public async Task<IActionResult> UpdateObjectNames()
        {
            try
            {
                await _objectNameUpdateService.UpdateObjectNamesAsync();
                return Ok(new { Message = "Object names updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating object names.", Error = ex.Message });
            }
        }
    }
}
*/