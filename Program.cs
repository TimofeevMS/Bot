using app_example_net_core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<TelegramBotService>();

var app = builder.Build();

var botService = app.Services.GetRequiredService<TelegramBotService>();
await botService.SetWebhookAsync();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();