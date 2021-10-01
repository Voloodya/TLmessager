using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TLmessanger.Abstraction;
using TLmessanger.Models;
using TLmessanger.Services;

namespace TLmessanger.Controllers
{
    [ApiController]
    [Route("api/v1/TelegramListener")]
    public class TelegramListenerController : ControllerBase
    {
        private readonly ILogger<TelegramListenerController> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private TelegramBotClient _telegramBotClient;
        private ICommandService _commandService;
        private string _currentPath;
        private MessageDataService _messageDataService;
        private string _token { get; set; } = "1956785959:AAHbzkyMAzp6b6houFkvZyAMoPVgK5hxmlk";
        //private string url = "https://оренбургвсе.рф/api/TelegramBot/AcceptReplyMessageBot";
        private string url = "http://localhost:5001//api/TelegramBot/AcceptReplyMessageBot";
        private string registerBotUrl = "https://api.telegram.org/bot1956785959:AAHbzkyMAzp6b6houFkvZyAMoPVgK5hxmlk/setwebhook?url=https://messager.оренбургвсе.рф/api/v1/TelegramListener";
        private string strCheackBot = "https://api.telegram.org/bot1956785959:AAHbzkyMAzp6b6houFkvZyAMoPVgK5hxmlk/getWebhookInfo";
        public TelegramListenerController(ILogger<TelegramListenerController> logger, IHttpClientFactory clientFactory, ICommandService commandService)
        {
            _logger = logger;
            // Фабрика для создания HttpClient, которая будет использоваться во всем приложении
            _clientFactory = clientFactory;
            _telegramBotClient = new TelegramBotClient(_token, _clientFactory.CreateClient());
            _currentPath = Environment.CurrentDirectory;
            _commandService = commandService;
            _messageDataService = new MessageDataService();
        }

        private static readonly string[] Summaries = new[] {"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"};
            
        //[HttpGet]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    var rng = new Random();
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateTime.Now.AddDays(index),
        //        TemperatureC = rng.Next(-20, 55),
        //        Summary = Summaries[rng.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}

        [HttpGet]
        public async Task<OkObjectResult> Get()
        {

            return Ok("Started!");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {

            if (update == null) return Ok();

            List<string> listlogs = new List<string>();
            listlogs.Add(String.Format("Update: Id {0}; Date: {1}; UserName: {2}; Message {1};", update.Id.ToString(), update.Message.Date, update.Message.From.Username, update.Message.Text));

            var message = update.Message;

            foreach (var command in _commandService.Get())
            {
                if (command.Contains(message))
                {
                    await command.Execute(message, _telegramBotClient);
                    break;
                }
            }

            string responseMessage = "";
            if (message != null)
            {
                responseMessage = "Укажите номер в федеральном формате (+7хххххххххх)";
                // Ответ в чат-бот 
                //await telegramBotClient.SendTextMessageAsync(message.Chat.Id, responseData.TextMessage, replyToMessageId: message.MessageId);

                if (message.ReplyToMessage != null)
                {
                    string messageNumberPhone;
                    switch (message.ReplyToMessage.Text)
                    {
                        case "Укажите номер в федеральном формате (+7хххххххххх)":
                            messageNumberPhone = ServicePhoneNumber.LeaveOnlyNumbers(message.Text);

                            if (messageNumberPhone.Length < 11 || messageNumberPhone.Length > 12)
                            {
                                await _telegramBotClient.SendTextMessageAsync(message.From.Id, "Номер указан в неверном формате. Ждем Ваш номер в федеральном формате (+7хххххххххх)", ParseMode.Default, replyMarkup: new ForceReplyMarkup { Selective = true });
                            }
                            else
                            {
                                MessageData requestMessage = _messageDataService.CreateMessageData(update, this.HttpContext.Request.Host.Value.ToString());

                                // Отправка запроса на API др. сервиса
                                string jsonRequest = JsonSerializer.Serialize(requestMessage);
                                //////////////////////////////////////////////////////////////
                                listlogs.Add(String.Format("Запрос на сервер отправлен на номер {0};", requestMessage.PhoneNumber));
                                ReadWriteFileTxt.WriteFile(listlogs, _currentPath, "logs_TelegramBot_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day, "txt");
                                /////////////////////////////////////////////////////////////////
                                string jsonResponseData = await PostRequestHttpAsync(url, jsonRequest);
                                ResponseMessageData responseData = JsonSerializer.Deserialize<ResponseMessageData>(jsonResponseData);

                                if (!responseData.Status.Equals("NotFound") && responseData.TextMessage != null)
                                {
                                    string responseMessageFromDB = responseData.TextMessage;
                                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, responseMessageFromDB);
                                }
                                else
                                {
                                    string responseMessageFromDB = "Участник с данным номером телефона не найден!";
                                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, responseMessageFromDB);
                                }
                                //await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Номер указан в верном формате");
                            }
                            break;

                        case "Номер указан в неверном формате. Ждем Ваш номер в федеральном формате (+7хххххххххх)":
                            messageNumberPhone = ServicePhoneNumber.LeaveOnlyNumbers(message.Text);

                            if (messageNumberPhone.Length < 11 || messageNumberPhone.Length > 12)
                            {
                                await _telegramBotClient.SendTextMessageAsync(message.From.Id, "Номер указан в неверном формате. Ждем Ваш номер в федеральном формате (+7хххххххххх)", ParseMode.Default, replyMarkup: new ForceReplyMarkup { Selective = true });
                            }
                            else
                            {
                                MessageData requestMessage = _messageDataService.CreateMessageData(update, this.HttpContext.Request.Host.Value.ToString());
                                // Отправка запроса на API др. сервиса
                                string jsonRequest = JsonSerializer.Serialize(requestMessage);
                                //////////////////////////////////////////////////////////////
                                listlogs.Add(String.Format("Запрос на сервер отправлен на номер {0};", requestMessage.PhoneNumber));
                                ReadWriteFileTxt.WriteFile(listlogs, _currentPath, "logs_TelegramBot_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day, "txt");
                                /////////////////////////////////////////////////////////////////
                                string jsonResponseData = await PostRequestHttpAsync(url, jsonRequest);
                                ResponseMessageData responseData = JsonSerializer.Deserialize<ResponseMessageData>(jsonResponseData);

                                if (!responseData.Status.Equals("Not found") && responseData.TextMessage != null)
                                {
                                    string responseMessageFromDB = responseData.TextMessage;
                                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, responseMessageFromDB);
                                }
                                else
                                {
                                    string responseMessageFromDB = "Участник с данным номером телефона не найден!";
                                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, responseMessageFromDB);
                                }
                            }
                            //await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Номер был указан в верном формате и исправлен");
                            break;
                    }
                }
                else
                {
                    MessageData requestMessage = _messageDataService.CreateMessageData(update, this.HttpContext.Request.Host.Value.ToString());

                    // Отправка запроса на API др. сервиса
                    string jsonRequest = JsonSerializer.Serialize(requestMessage);
                    string str = null;
                    try
                    {
                        string jsonResponseData = await PostRequestHttpAsync(url, jsonRequest);
                        ResponseMessageData responseData = JsonSerializer.Deserialize<ResponseMessageData>(jsonResponseData);
                        str = responseData.Status;
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex.ToString());
                    }
                    return Ok(str);
                    await _telegramBotClient.SendTextMessageAsync(message.From.Id, responseMessage, ParseMode.Default, replyMarkup: new ForceReplyMarkup { Selective = true });
                }
            }

            listlogs.Add(String.Format("Answer: {0};", responseMessage));
            ReadWriteFileTxt.WriteFile(listlogs, _currentPath, "logs_TelegramBot_" + DateTime.Now.Year+"_"+DateTime.Now.Month+"_"+ DateTime.Now.Day, "txt");

            return Ok();
        }

        // Сюда должны приходить сообщения с чат-бота
        [ApiExplorerSettings(IgnoreApi = true)]
        private async void TelegramBotClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            if(message != null)
            {
                MessageData requestMessage = new MessageData { TextMessage = message.Text };

                // Отправка запроса на API др. сервиса
                string jsonRequest = JsonSerializer.Serialize(requestMessage);
                string jsonResponseData = await PostRequestHttpAsync(url, jsonRequest);
                ResponseMessageData responseData = JsonSerializer.Deserialize<ResponseMessageData>(jsonResponseData);

                // Ответ в чат-бот 
                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id,responseData.TextMessage, replyToMessageId: message.MessageId);
            }
        }

        // Функция отправки запроса на API др. сервиса 
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<string> PostRequestHttpAsync(string url, string json)
        {
            using HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpClient httpClient = _clientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(120);
            using HttpResponseMessage response = await httpClient.PostAsync(url, content).ConfigureAwait(false);

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

    }
}
