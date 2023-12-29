/*using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galytix.WebApi.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ServerController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("ping")]
        public async Task<IActionResult> Ping()
        {
            return Ok("pong");
        }
    }
}*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Galytix.WebApi.Controllers
{
    public class ServerController : ControllerBase
    {
        // Existing ping endpoint
        [AllowAnonymous]
        [HttpGet]
        [Route("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }

        // New endpoint for calculating average GWP
        [AllowAnonymous]
        [HttpPost]
        [Route("api/gwp/avg")]
        public IActionResult CalculateAverageGwp([FromBody] JObject requestData)
        {
            try
            {
                string country = requestData["country"].ToString();
                List<string> lobs = requestData["lob"].ToObject<List<string>>();

                Dictionary<string, double> result = CalculateAverageForCountry(country, lobs);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    private Dictionary<string, double> CalculateAverageForCountry(string country, List<string> lobs)
{
    string csvFilePath = Path.Combine("Data", "gwpByCountry.csv");

    var data = System.IO.File.ReadLines(csvFilePath)
        .Skip(1) // Skip header
        .Select(line => line.Split(','))
        .Where(parts => parts.Length >= 6 && parts[0].ToLower() == country.ToLower() && lobs.Contains(parts[3].ToLower()))
        .SelectMany(parts => parts.Skip(4).Where(s => !string.IsNullOrEmpty(s)).Select(s => double.TryParse(s, out double value) ? value : 0.0)) // Assuming the GWP values start from index 4
        .ToList();

    var result = lobs.ToDictionary(
        lob => lob,
        lob => data.Where((_, index) => index % lobs.Count == lobs.IndexOf(lob)).Average()
    );

    return result;
}
    }
}
