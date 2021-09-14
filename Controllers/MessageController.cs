using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using TLmessanger.Models;

namespace TLmessanger.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly ILogger<MessageController> _logger;
        private static  string _token { get; set; } = "1956785959:AAHbzkyMAzp6b6houFkvZyAMoPVgK5hxmlk";
        private static TelegramBotClient telegramBotClient;

        public MessageController(ILogger<MessageController> logger)
        {
            _logger = logger;

            telegramBotClient = new TelegramBotClient(_token);
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

        [HttpPost("sendMessage")]
        public async Task SendMessage([FromBody] Message message)
        {
            
            // Получение сообщений от бота
            telegramBotClient.OnMessage += TelegramBotClient_OnMessage;
            // Запуск бота для приема сообщений
            telegramBotClient.StartReceiving();
            // test your api configured correctly
            //var me = await telegramBotClient.GetMeAsync();
            //Console.WriteLine($"{me.Username} started");

        }

        // Сюда должны приходить сообщения
        private void TelegramBotClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {

        }
    }
}
