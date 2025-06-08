using app_example_net_core.Services;

using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace TelegramBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelegramBotController : ControllerBase
{
    private readonly TelegramBotService _botService;

    public TelegramBotController(TelegramBotService botService)
    {
        _botService = botService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update, CancellationToken cancellationToken)
    {
        await _botService.HandleUpdateAsync(update, cancellationToken);
        return Ok();
    }
}