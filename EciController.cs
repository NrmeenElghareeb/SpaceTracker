using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpaceTrack.DAL.Model;
using SpaceTrack.Services;

namespace SpaceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EciController : ControllerBase
    {

        private readonly IEciService _eciService;
        private readonly ILogger<EciController> _logger;

        public EciController(IEciService eciService, ILogger<EciController> logger)
        {
            _eciService = eciService ?? throw new ArgumentNullException(nameof(eciService));
            _logger = logger;
        }
        [HttpPut("compute")]
        public async Task<IActionResult> ComputeAndUpdateEci([FromBody] ComputeEciRequest request)
        {
            if (request == null)
            {
                _logger.LogWarning("Received a null request.");
                return BadRequest("Request body is required.");
            }

            if (string.IsNullOrWhiteSpace(request.ObjectId))
            {
                _logger.LogWarning("Missing Object ID in request.");
                return BadRequest("Object ID is required.");
            }

            if (request.NoradId <= 0)
            {
                _logger.LogWarning("Invalid Norad ID: {NoradId}", request.NoradId);
                return BadRequest("Norad ID must be a positive integer.");
            }

            if (string.IsNullOrWhiteSpace(request.FirstLine) || string.IsNullOrWhiteSpace(request.SecondLine))
            {
                _logger.LogWarning("Missing TLE lines in request.");
                return BadRequest("Both TLE lines are required.");
            }

            try
            {
                _logger.LogInformation("Computing ECI for ObjectId: {ObjectId}, NoradId: {NoradId}, Date: {Date}",
                                        request.ObjectId, request.NoradId, request.Date);

                var result = await _eciService.ComputeEciForDayAsync(
                    request.ObjectId,
                    request.NoradId,
                    request.FirstLine,
                    request.SecondLine,
                    request.Date);

                if (result == null || result.Count == 0)
                {
                    _logger.LogInformation("No ECI positions computed for ObjectId: {ObjectId} on {Date}", request.ObjectId, request.Date);
                    return NotFound("No ECI positions were computed or updated.");
                }

                _logger.LogInformation("Successfully computed and updated {Count} ECI positions for ObjectId: {ObjectId} on {Date}",
                                        result.Count, request.ObjectId, request.Date);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing ECI for ObjectId: {ObjectId} on {Date}", request.ObjectId, request.Date);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }

    public class ComputeEciRequest
    {
        public string ObjectId { get; set; }
        public int NoradId { get; set; }
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Computes the ECI position based on TLE data and UTC time.
    /// </summary>
    /// <param name="request">TLE and UTC time details</param>
    /// <returns>ECI position data</returns>
    //[HttpPost("compute")]
    //public async Task<IActionResult> ComputeEciAsync([FromBody] EciPosition request)
    //{
    //    if (request == null)
    //        return BadRequest("Request data is required.");

    //    _logger.LogInformation("Received ComputeEciAsync Request: {@Request}", request);

    //    if (string.IsNullOrWhiteSpace(request.FirstLine) || string.IsNullOrWhiteSpace(request.SecondLine))
    //    {
    //        return BadRequest("TLE data (firstLine and secondLine) is required.");
    //    }

    //    try
    //    {
    //        var eciResult = await _eciService.ComputeEciAsync(
    //            request.ObjectId,
    //            request.NoradId,
    //            request.FirstLine,
    //            request.SecondLine,
    //            request.UtcTime
    //        );

    //        return Ok(eciResult);
    //    }
    //    catch (ArgumentException ex)
    //    {
    //        _logger.LogError(ex, "Invalid input: {Message}", ex.Message);
    //        return BadRequest(ex.Message);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Internal Server Error");
    //        return StatusCode(500, $"Internal Server Error: {ex.Message}");
    //    }
    ////}
    //[HttpGet("daily")]
    //public async Task<IActionResult> ComputeEciForDay(int noradId, string firstLine, string secondLine, DateTime date)
    //{
    //    if (noradId <= 0 || string.IsNullOrWhiteSpace(firstLine) || string.IsNullOrWhiteSpace(secondLine))
    //    {
    //        return BadRequest("Valid NORAD ID and TLE lines are required.");
    //    }

    //    try
    //    {
    //        _logger.LogInformation("Computing daily ECI for NORAD ID: {NoradId} on {Date}", noradId, date);

    //        var eciPositions = await _eciService.ComputeEciForDayAsync(
    //            $"Object-{noradId}",
    //            noradId,
    //            firstLine,
    //            secondLine,
    //            date
    //        );

    //        if (eciPositions == null || eciPositions.Count == 0)
    //        {
    //            return NotFound("No ECI data computed for this day.");
    //        }

    //        return Ok(eciPositions);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error computing daily ECI for NORAD ID: {NoradId}", noradId);
    //        return StatusCode(500, $"Internal Server Error: {ex.Message}");
    //    }
    //}


}


    

