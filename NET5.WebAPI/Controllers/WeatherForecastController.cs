using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NET5.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ILoggerManager _loggerManager;
        private IRepositoryWrapper _repoWrapper;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, 
            ILoggerManager loggerManager,
            IRepositoryWrapper repoWrapper)
        {
            _logger = logger;
            _loggerManager = loggerManager;
            _repoWrapper = repoWrapper;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            // Log to file

            //_loggerManager.LogInfo("Here is info message from the controller.");
            //_loggerManager.LogDebug("Here is debug message from the controller.");
            //_loggerManager.LogWarn("Here is warn message from the controller.");
            //_loggerManager.LogError("Here is error message from the controller.");

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
