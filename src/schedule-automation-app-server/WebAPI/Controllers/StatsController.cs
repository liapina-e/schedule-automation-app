using Microsoft.AspNetCore.Mvc;
using schedule_automation_app_server.Application.Services.Interfaces;

namespace schedule_automation_app_server.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        return Ok(await _statsService.GetStatsAsync());
    }
}