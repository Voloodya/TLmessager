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
        private TelegramBotClient telegramBotClient;
        private string currentPath;
        private string _token { get; set; } = "1956785959:AAHbzkyMAzp6b6houFkvZyAMoPVgK5hxmlk";
        private string url = "https://оренбургвсе.рф/api/TelegramBot/AcceptReplyMessageBot";
        private string registerBotUrl = "https://api.telegram.org/bot1956785959:AAHbzkyMAzp6b6houFkvZyAMoPVgK5hxmlk/setwebhook?url=https://messager.оренбургвсе.рф/api/v1/TelegramListener";
        private string strCheackBot = "https://api.telegram.org/bot1956785959:AAHbzkyMAzp6b6houFkvZyAMoPVgK5hxmlk/getWebhookInfo";
        public TelegramListenerController(ILogger<TelegramListenerController> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            // Фабрика для создания HttpClient, которая будет использоваться во всем приложении
            _clientFactory = clientFactory;
            telegramBotClient = new TelegramBotClient(_token, _clientFactory.CreateClient());
            currentPath = Environment.CurrentDirectory;
        }

        private static readonly string[] Summaries = new[] {"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"};
            
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        //[HttpGet]
        //public async Task<OkObjectResult> Test()
        //{

        //    return Ok("Тест пройден");
        //}

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)//[FromServices] HandleUpdateService handleUpdateService
        {
            // Запуск бота для приема сообщений
            //telegramBotClient.StartReceiving();
            //// Получение сообщений от бота
            //telegramBotClient.OnMessage += TelegramBotClient_OnMessage;


            // test your api configured correctly
            //var me = await telegramBotClient.GetMeAsync();
            //Console.WriteLine($"{me.Username} started");
            List<string> listlogs = new List<string>();
            listlogs.Add(String.Format("Update: Id {0}; Date: {1}; UserFirstName: {2}; Message {1};", update.Id.ToString(), update.Message.Date, "", update.Message.Text));

            var message = update.Message;
            string responseMessage = "";
            if (message != null)
            {
                //MessageData requestMessage = new MessageData { TextMessage = message.Text, PhoneNumber = message.Contact.PhoneNumber, UserName = message.Contact.FirstName };

                // Отправка запроса на API др. сервиса
                //string jsonRequest = JsonSerializer.Serialize(requestMessage);
                //string jsonResponseData = await PostRequestHttpAsync(url, jsonRequest);
                //ResponseMessageData responseData = JsonSerializer.Deserialize<ResponseMessageData>(jsonResponseData);
                responseMessage = message.Text;
                // Ответ в чат-бот 
                //await telegramBotClient.SendTextMessageAsync(message.Chat.Id, responseData.TextMessage, replyToMessageId: message.MessageId);

                await telegramBotClient.SendTextMessageAsync(message.From.Id, responseMessage, replyToMessageId: message.MessageId);
            }
            listlogs.Add(String.Format("Answer: {0};", responseMessage));
            ReadWriteFileTxt.WriteFile(listlogs, currentPath, "logs_TelegramBot_" + DateTime.Now.Year+"_"+DateTime.Now.Month+"_"+ DateTime.Now.Day, "txt");

            return Ok("Запрос прошел");
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
                await telegramBotClient.SendTextMessageAsync(message.Chat.Id,responseData.TextMessage, replyToMessageId: message.MessageId);
            }
        }

        // Функция отправки запроса на API др. сервиса 
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<string> PostRequestHttpAsync(string url, string json)
        {
            using HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await _clientFactory.CreateClient().PostAsync(url, content).ConfigureAwait(false);

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

    }
}
