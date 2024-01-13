using hoa7mlishe.API.Database.Context;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace hoa7mlishe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class IvonovController : ControllerBase
    {
        private readonly Hoa7mlisheContext _context;
        private Random _rnd { get; }
        public IvonovController(Hoa7mlisheContext context)
        {
            _context = context;
        }
        

        /// <summary>
        /// Возвращает все цитаты ивонова
        /// </summary>
        /// <returns>список цитат</returns>
        /// <response code="200">OK</response>
        [HttpGet("Quotes/Ivonov")]
        public IActionResult GetIvonovQuotes()
        {
            return Ok(_context.IvonovQuotes.ToList());
        }

        /// <summary>
        /// Получает время окончания отсчета (в миллисекундах)
        /// </summary>
        /// <param name="name">Название счетчика</param>
        /// <returns>Время окончания отсчета</returns>
        [HttpGet("DeathClock/{name}")]
        public IActionResult GetDeathClock(
            string name)
        {
            Console.WriteLine(name);
            var deathClock = _context.DeathClocks.Where(x => x.Name == name).FirstOrDefault();

            if (deathClock is null)
            {
                return NotFound();
            }

            var milliseconds = deathClock.DeathTime.ToUniversalTime().Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc
            )).TotalMilliseconds;

            return Ok(milliseconds);
        }
    }
}
