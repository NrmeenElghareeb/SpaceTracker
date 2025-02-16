using Microsoft.AspNetCore.Mvc;
using SpaceTrack.Services;
using System;
using System.Threading.Tasks;

namespace SpaceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TLEController : ControllerBase
    {
        private readonly ITLEService _tleService;

        public TLEController(ITLEService tleService)
        {
            _tleService = tleService;
        }
        [HttpPut("Update-PayloadsTLEs")]
        public async Task<IActionResult> Save_UpdateTLEs()
        {
            await _tleService.SaveOrUpdateAllPayloadsTLEsAsync();
            return Ok("TLE data successfully updated for all tracked active payloads.");
        }

        [HttpPut("Update-DebrisTLEs")]
        public async Task<IActionResult> SaveOrUpdateTLEs()
        {
            await _tleService.SaveOrUpdateAllDebrisTLEsAsync();
            return Ok("TLE data successfully updated for all tracked space debris.");
        }

        [HttpPut("Update-RocketsTLEs")]
        public async Task<IActionResult> SaveUpdateTLEs()
        {
            await _tleService.SaveOrUpdateAllRocketsTLEsAsync();
            return Ok("TLE data successfully updated for all tracked Rockets.");
        }

        [HttpPut("Update-UnknownsobjTLEs")]
        public async Task<IActionResult> SaveandUpdateTLEs()
        {
            await _tleService.SaveOrUpdateAllUnknownTLEsAsync();
            return Ok("TLE data successfully updated for all tracked unknown object.");
        }











        //////////////////////////////////////////////////////////////////////////////////////////Excellent

        //[HttpPut("save-tles")]
        //public async Task<IActionResult> SaveOrUpdateTLEs()
        //{
        //    await _tleService.SaveOrUpdateTLEsAsync();
        //    return Ok("TLE data saved or updated successfully for all NORAD IDs.");
        //}
        ///////////////////////////////////////////////////////////////////////////////////////Excellent

        /////////////////////////////////////////////////Change 3
        //[HttpPut("save-iss-tle")]
        //public async Task<IActionResult> SaveOrUpdateISSTLE()
        //{
        //    await _tleService.SaveOrUpdateTLEOfISSAsync();
        //    return Ok("ISS TLE data saved or updated successfully.");
        //}
        /////////////////////////////////////////////////////////////////////Change 3
        //[HttpPost("save-iss-tle")] /////////////////////change 2
        //public async Task<IActionResult> SaveISSTLE()
        //{
        //    await _tleService.SaveTLEOfISSAsync();
        //    return Ok("ISS TLE data saved successfully.");
        //}////////////////////////////////////////////change 2
    }
    ///////////////////////////////////////////////////










    //public class TLEController : ControllerBase
    //{
    //    private readonly ITLEService _tleService;

    //    public TLEController(ITLEService tleService)
    //    {
    //        _tleService = tleService;
    //    }

    //    [HttpPost("fetch-and-save")]
    //    public async Task<IActionResult> FetchAndSaveTLE()
    //    {
    //        try
    //        {
    //            int[] noradIds = { 25544 }; // Add the NORAD IDs you want to fetch
    //            var tleData = await _tleService.FetchTLEFromSpaceTrack(noradIds);
    //            await _tleService.SaveTLEToDatabase(tleData);

    //            return Ok("TLE data has been fetched and saved successfully.");
    //        }
    //        catch (Exception ex)
    //        {
    //            return StatusCode(500, $"An error occurred: {ex.Message}");
    //        }
    //    }
    //}
}


