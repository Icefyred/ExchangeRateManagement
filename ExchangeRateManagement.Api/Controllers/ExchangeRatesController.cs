using ExchangeRateManagement.Domain.ExchangeRate;
using ExchangeRateManagement.Service.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeRateManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly IExchangeRateService _exchangeRateService;

        public ExchangeRatesController(IExchangeRateService exchangeRateService)
        {
            _exchangeRateService = exchangeRateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRateAsync([FromQuery] string baseCurrency, [FromQuery] string targetCurrency)
        {
            if (string.IsNullOrWhiteSpace(baseCurrency) || string.IsNullOrWhiteSpace(targetCurrency))
                return BadRequest("Base and target currencies are required.");

            var result = await _exchangeRateService.GetRateAsync(baseCurrency, targetCurrency);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ExchangeRate rate)
        {
            if (rate == null)
                return BadRequest();

            await _exchangeRateService.CreateRateAsync(rate);

            return CreatedAtAction(nameof(GetRateAsync), new
            {
                baseCurrency = rate.BaseCurrency,
                targetCurrency = rate.TargetCurrency
            }, rate);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ExchangeRate rate)
        {
            if (rate == null)
                return BadRequest();

            await _exchangeRateService.UpdateRateAsync(rate);

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] string baseCurrency, [FromQuery] string targetCurrency)
        {
            if (string.IsNullOrWhiteSpace(baseCurrency) || string.IsNullOrWhiteSpace(targetCurrency))
                return BadRequest();

            await _exchangeRateService.DeleteRateAsync(baseCurrency, targetCurrency);

            return NoContent();
        }
    }
}