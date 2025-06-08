using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace app_example_net_core.Services;
public class TelegramBotService
{
    private readonly ITelegramBotClient _botClient;
    private readonly string _webhookUrl;
    private const string ChannelId = "@safe_land";
    private const string PdfFilePath = "file.pdf";
    private const string FirstImageFilePath = "firstimage.jpg";
    private const string QuestImageFilePath = "questimage.jpg";
    private const string ImageFilePath = "image.jpg";
    private const string VoiceFilePath = "voice.ogg";
    private readonly ILogger<TelegramBotService> _logger;

    public TelegramBotService(ILogger<TelegramBotService> logger)
    {
        _logger = logger;
        var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
        var webhookUrl = Environment.GetEnvironmentVariable("WEBHOOK_URL");
        _botClient = new TelegramBotClient(botToken);
        _webhookUrl = webhookUrl;
    }

    public async Task SetWebhookAsync()
    {
        await _botClient.SetWebhook(_webhookUrl);
        _logger.LogInformation($"Webhook установлен: {_webhookUrl}");
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update is { Type: UpdateType.Message, Message.Type: MessageType.Text })
            {
                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;
                var user = update.Message.From;
                
                if (messageText == "/start")
                {
                    var keyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Скачать гайд", "download_pdf"));
                    
                    await using (var imageStream = new FileStream(FirstImageFilePath, FileMode.Open, FileAccess.Read))
                    {
                        await _botClient.SendPhoto(chatId: chatId,
                                                   photo: new InputFileStream(imageStream, "image.jpg"),
                                                   caption: "Рада видеть вас здесь.\nВы сделали важный шаг — выбрали заботу о себе \ud83d\udc96\n\nНиже вы получите практическое руководство «5 упражнений для укрепления личных границ».\n\nЭто не просто набор техник — это первые шаги к внутренней опоре, спокойствию и умению говорить \"нет\" без чувства вины.",
                                                   replyMarkup: keyboard,
                                                   cancellationToken: cancellationToken);
                    }
                    
                    _logger.LogInformation($"User ({user?.Id}) {user?.FirstName} sent /start");
                }
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;
                var user = callbackQuery?.From;
                var chatId = callbackQuery?.Message?.Chat.Id;
                
                switch (callbackQuery?.Data)
                {
                    case "download_pdf":
                    {
                        await using (var fileStream = new FileStream(PdfFilePath, FileMode.Open, FileAccess.Read))
                        {
                            var keyboardForPdf = new InlineKeyboardMarkup(new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Да", "yes"),
                                InlineKeyboardButton.WithCallbackData("Нет", "no"),
                            });
                            
                            await _botClient.SendDocument(chatId: chatId,
                                                          document: new InputFileStream(fileStream, "5_упражнений_для_укрепления_личных_границ.pdf"),
                                                          caption: "Готово! Гайд уже у вас \ud83d\udc4d\n\nПопробуйте хотя бы одно упражнение уже сегодня. Маленькие шаги ведут к большим изменениям!\n\nХотите получить в подарок лекцию и узнать почему вам так сложно говорить нет?",
                                                          replyMarkup: keyboardForPdf,
                                                          cancellationToken: cancellationToken);
                        }

                        _logger.LogInformation($"User ({user?.Id}) {user?.FirstName} sent /download_pdf");
                        
                        await _botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: cancellationToken);

                        break;
                    }

                    case "yes":
                    {
                        var keyboard = new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithUrl("Подписаться", $"https://t.me/{ChannelId.TrimStart('@')}"),
                            InlineKeyboardButton.WithCallbackData("Забрать подарок", "check_subscription"),
                        });

                        await using (var imageStream = new FileStream(ImageFilePath, FileMode.Open, FileAccess.Read))
                        {
                            await _botClient.SendPhoto(chatId: chatId,
                                                       photo: new InputFileStream(imageStream, "image.jpg"),
                                                       caption: "Присоединяйтесь к моему каналу, и получите от меня в подарок аудио-лекцию о том, почему бывает так сложно говорить \"нет\"",
                                                       replyMarkup: keyboard,
                                                       cancellationToken: cancellationToken);
                        }
                        
                        _logger.LogInformation($"User ({user?.Id}) {user?.FirstName} sent /yes");

                        await _botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: cancellationToken);
                        
                        break;
                    }

                    case "no":
                    {
                        await using (var imageStream = new FileStream(QuestImageFilePath, FileMode.Open, FileAccess.Read))
                        {
                            var keyboard = new InlineKeyboardMarkup(new[]
                            {
                                InlineKeyboardButton.WithUrl("Подписаться", $"https://t.me/{ChannelId.TrimStart('@')}"),
                            });
                            
                            await _botClient.SendPhoto(chatId: chatId,
                                                       photo: new InputFileStream(imageStream, "image.jpg"),
                                                       caption: "Даже, если вы с психологией на \"ты\", присоединяйтесь к моему телеграм каналу\n\nТам я делюсь короткими практиками, историями клиентов (анонимно) и разбираю психологические вопросы от подписчиков",
                                                       replyMarkup: keyboard,
                                                       cancellationToken: cancellationToken);
                        }
                        
                        _logger.LogInformation($"User ({user?.Id}) {user?.FirstName} sent /no");

                        await _botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: cancellationToken);
                        
                        break;
                    }
                        
                    case "check_subscription":
                        try
                        {
                            var chatMember = await _botClient.GetChatMember(ChannelId, user.Id, cancellationToken: cancellationToken);

                            if (chatMember.Status is ChatMemberStatus.Member or ChatMemberStatus.Administrator or ChatMemberStatus.Creator)
                            {
                                await using var voiceStream = new FileStream(VoiceFilePath, FileMode.Open, FileAccess.Read);

                                await _botClient.SendVoice(chatId: chatId,
                                                           voice: new InputFileStream(voiceStream, "voice.ogg"),
                                                           cancellationToken: cancellationToken);
                                
                                await _botClient.SendMessage(chatId: chatId,
                                                             text: "Эта лекция отвечает на вопрос почему чаше всего люди снова и снова соглашаются на то, что им не нравится\n\n__\nИрина Тимофеева, семейный психолог\n\nВы всегда можете мне написать, если возникнут вопросы @psy_irr",
                                                             cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await _botClient.SendMessage(chatId: chatId,
                                                             text: $"Пожалуйста, подпишитесь на канал {ChannelId} сначала!",
                                                             cancellationToken: cancellationToken);
                            }
                        }
                        catch (ApiRequestException ex)
                        {
                            await _botClient.SendMessage(chatId: chatId,
                                                         text:
                                                         "Ошибка при проверке подписки. Убедитесь, что вы подписаны на канал!",
                                                         cancellationToken: cancellationToken);

                            Console.WriteLine($"Ошибка проверки подписки: {ex.Message}");
                        }
                        
                        _logger.LogInformation($"User ({user?.Id}) {user?.FirstName} sent /check_subscription");

                        await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка: {ex.Message}");
        }
    }
}