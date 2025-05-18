using apbd11_cw5.DTOs;
using apbd11_cw5.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd11_cw5.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IDbService _dbService;
        public PrescriptionsController(IDbService dbService) => _dbService = dbService;

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PrescriptionRequestDto dto)
        {
            try
            {
                var id = await _dbService.CreatePrescriptionAsync(dto);
                return CreatedAtAction(nameof(GetByPatient), new { idPatient = dto.IdPatient ?? 0 }, new { IdPrescription = id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("patient/{idPatient}")]
        public async Task<IActionResult> GetByPatient(int idPatient)
        {
            try
            {
                var detail = await _dbService.GetPatientWithPrescriptionsAsync(idPatient);
                return Ok(detail);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
