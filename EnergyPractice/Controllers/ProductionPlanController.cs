using Core.Interface;
using Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using System.Text;

namespace EnergyPractice.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductionPlanController : ControllerBase
    {

        private readonly IComputeSolution _computeSolution;

        public ProductionPlanController(IComputeSolution computeSolution)
        {
            _computeSolution = computeSolution;
        }

        [HttpPost(Name = "productionplan")]
        public async Task<IActionResult> Post([FromBody] Payload payload)
        {
            var response = _computeSolution.Solution(payload);
            return Ok(response);
        }
    }
}