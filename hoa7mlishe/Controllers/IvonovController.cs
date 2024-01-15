using hoa7mlishe.API.Database.Context;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace hoa7mlishe.Controllers
{
    /// <summary>
    /// Контроллер для работы с часами и цитатами
    /// </summary>
    /// <param name="context">Контекст БД</param>
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class IvonovController(Hoa7mlisheContext context) : ControllerBase
    {
        private readonly Hoa7mlisheContext _context = context;
        private Random _rnd { get; }

        /// <summary>
        /// Возвращает все цитаты ивонова
        /// </summary>
        /// <returns></returns>
        [HttpGet("Quotes/Ivonov")]
        public IActionResult GetIvonovQuotes()
        {
            return Ok(_context.IvonovQuotes.ToList());
        }

        /// <summary>
        /// Получает время окончания отсчета (в миллисекундах)
        /// </summary>
        /// <param name="name">Название счетчика</param>
        /// <returns></returns>
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
