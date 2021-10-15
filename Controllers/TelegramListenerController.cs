using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
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
        private const string commandGet = "Получить QR-код";
        private const string commandRegistration = "Зарегистрироваться";
        private readonly ILogger<TelegramListenerController> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private TelegramBotClient _telegramBotClient;
        private ICommandService _commandService;
        private string _currentPath;
        private MessageDataService _messageDataService;
        private string _token { get; set; } = "1956785959:AAHbzkyMAzp6b6houFkvZyAMoPVgK5hxmlk";
        private string url = "https://оренбургвсе.рф/api/TelegramBot/AcceptReplyMessageBot";
        //private string url = "http://localhost:5001/api/TelegramBot/AcceptReplyMessageBot";
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
            listlogs.Add(String.Format("Update: Id {0}; Date: {1}; UserName: {2}; Message {3};", update.Id.ToString(), update.Message.Date, update.Message.From.Username, update.Message.Text));
            ReadWriteFileTxt.WriteFile(listlogs, _currentPath, "logs_TelegramBot_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day, "txt", newpath: "logs");

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
                                listlogs.Add(String.Format("Запрос на сервер отправлен на номер {0}", requestMessage.PhoneNumber));
                                ReadWriteFileTxt.WriteFile(listlogs, _currentPath, "logs_TelegramBot_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day, "txt", newpath: "logs");
                                /////////////////////////////////////////////////////////////////
                                string jsonResponseData = await PostRequestHttpAsync(url, jsonRequest);
                                ResponseMessageData responseData = JsonSerializer.Deserialize<ResponseMessageData>(jsonResponseData);

                                //ResponseMessageData responseData = new ResponseMessageData();
                                //responseData.textMessage = "No response";                                

                                if (responseData.status != null && !responseData.status.Equals("Not found") && responseData.textMessage != null)
                                {
                                    Byte[] byteCodes = null;
                                    string responseMessageFromDB = responseData.textMessage;
                                    if (responseData.byteQrcode != null)
                                    {
                                        byteCodes = QRcodeServices.BitmapToBytes(QRcodeServices.CreateBitmapFromBytes(responseData.byteQrcode));
                                    }
                                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Ваш QR-код: " + responseMessageFromDB);
                                    if (byteCodes != null)
                                    {
                                        using (var ms = new MemoryStream(byteCodes))
                                        {
                                            await _telegramBotClient.SendPhotoAsync(message.Chat.Id, photo: new InputOnlineFile(ms, "QR_code.png"));
                                        }

                                    }
                                }
                                else
                                {
                                    string responseMessageFromDB = "Участник с данным номером телефона не найден!";
                                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, responseMessageFromDB);
                                    await _telegramBotClient.SendTextMessageAsync(message.From.Id, "Если желаете зарегистрироваться выберите пункт меню Зарегистрироваться", replyMarkup: GetMenuButtons());

                                }
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
                                ReadWriteFileTxt.WriteFile(listlogs, _currentPath, "logs_TelegramBot_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day, "txt", newpath: "logs");
                                /////////////////////////////////////////////////////////////////
                                string jsonResponseData = await PostRequestHttpAsync(url, jsonRequest);
                                ResponseMessageData responseData = JsonSerializer.Deserialize<ResponseMessageData>(jsonResponseData);
                                //ResponseMessageData responseData = new ResponseMessageData();
                                //responseData.textMessage = "No response";

                                if (responseData.status!=null && !responseData.status.Equals("Not found") && responseData.textMessage != null)
                                {
                                    Byte[] byteCodes = null;
                                    string responseMessageFromDB = responseData.textMessage;
                                    if (responseData.byteQrcode != null)
                                    {
                                        byteCodes = QRcodeServices.BitmapToBytes(QRcodeServices.CreateBitmapFromBytes(responseData.byteQrcode));
                                    }
                                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Ваш QR-код: " + responseMessageFromDB);
                                    if (byteCodes != null)
                                    {
                                        string path = SaveByteInFileImage.SaveImage(byteCodes, _currentPath, message.From.Username, "jpg");
                                        if ( path != null)
                                        {
                                            await _telegramBotClient.SendPhotoAsync(message.Chat.Id, photo: path);
                                        }
                                    }
                                }
                                else
                                {
                                    string responseMessageFromDB = "Участник с данным номером телефона не найден!";
                                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, responseMessageFromDB);
                                    await _telegramBotClient.SendTextMessageAsync(message.From.Id, "Если желаете зарегистрироваться выберите пункт меню Зарегистрироваться", replyMarkup: GetMenuButtons());
                                }
                            }
                            break;
                        case "Укажите фамилию имя отчество полностью, через пробел":
                            string textMessage = message.Text != null ? message.Text.Trim() : null;
                            if (textMessage != null && textMessage.Length > 4)//
                            {
                                string[] fio = message.Text.Split(' ');

                                if (fio.Length > 1)
                                {
                                    List<string> fioList = new List<string>();
                                    fioList.Add(fio[0]);
                                    fioList.Add(fio[1]);
                                    if(fio.Length>2) fioList.Add(fio[2]);
                                    string nameUser = message.From.Username != null ? message.From.Username : message.From.Id.ToString();
                                    ReadWriteFileTxt.WriteFile(fioList, _currentPath, nameUser, "txt", newpath: "RegisterUsers");
                                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Укажите номер телефона в федеральном формате (7хххххххххх) для регистрации", replyMarkup: new ForceReplyMarkup { Selective = true });
                                }
                                else
                                {
                                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Вы не указали или указали в неверном формате ФИО. Укажите ФИО полностью через пробел textMessage", replyMarkup: new ForceReplyMarkup { Selective = true });
                                }
                            }
                            else
                            {
                                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Вы не указали или указали в неверном формате ФИО. Укажите ФИО полностью через пробел textMessage != null && textMessage.Length < 5", replyMarkup: new ForceReplyMarkup { Selective = true });
                            }
                            break;
                        case "Вы не указали или указали в неверном формате ФИО. Укажите ФИО полностью через пробел":
                            textMessage = message.Text != null ? message.Text.Trim() : null;
                            if (textMessage != null && textMessage.Length < 5)
                            {
                                string[] fio = message.Text.Split(' ');

                                if (fio.Length > 1)
                                {
                                    List<string> fioList = new List<string>();
                                    fioList.Add(fio[0]);
                                    fioList.Add(fio[1]);
                                    if (fio.Length > 2) fioList.Add(fio[2]);
                                    string nameUser = message.From.Username != null ? message.From.Username : message.From.Id.ToString();
                                    ReadWriteFileTxt.WriteFile(fioList, _currentPath, nameUser, "txt", newpath: "RegisterUsers");
                                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Укажите номер телефона в федеральном формате (7хххххххххх) для регистрации", replyMarkup: new ForceReplyMarkup { Selective = true });
                                }
                                else
                                {
                                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Вы не указали или указали в неверном формате ФИО. Укажите ФИО полностью через пробел", replyMarkup: new ForceReplyMarkup { Selective = true });
                                }
                            }
                            else
                            {
                                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Вы не указали или указали в неверном формате ФИО. Укажите ФИО полностью через пробел", replyMarkup: new ForceReplyMarkup { Selective = true });
                            }

                            break;
                        case "Укажите номер телефона в федеральном формате (7хххххххххх) для регистрации":
                            messageNumberPhone = ServicePhoneNumber.LeaveOnlyNumbers(message.Text);

                            if (messageNumberPhone.Length < 11 || messageNumberPhone.Length > 12)
                            {
                                await _telegramBotClient.SendTextMessageAsync(message.From.Id, "Номер телефона указан в неверном формате. Ждем Ваш номер для регистрации в федеральном формате (+7хххххххххх)", ParseMode.Default, replyMarkup: new ForceReplyMarkup { Selective = true });
                            }
                            else
                            {
                                string nameUser = message.From.Username != null ? message.From.Username : message.From.Id.ToString();
                                ReadWriteFileTxt.WriteFile(messageNumberPhone, _currentPath, nameUser, "txt", newpath: "RegisterUsers");

                                string fullPath = String.Format(@"{0}\{1}.{2}", newFolder.FullName, nameFile, typeFile);
                                List<string> strList = ReadWriteFileTxt.ReadFile();
                                // Регистрация на основном сервисе

                                // Отправка сообщения с основного сервиса пользователю:
                                // Удаление файла 
                                // отправка QR-кода

                                //Отпрака сообщения о невозможности регистрации - дубликат ФИО или номера телефона
                                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Спасибо за регистрацию!");
                            }
                            break;

                        case "Номер телефона указан в неверном формате. Ждем Ваш номер для регистрации в федеральном формате (+7хххххххххх)":
                            messageNumberPhone = ServicePhoneNumber.LeaveOnlyNumbers(message.Text);

                            if (messageNumberPhone.Length < 11 || messageNumberPhone.Length > 12)
                            {
                                await _telegramBotClient.SendTextMessageAsync(message.From.Id, "Номер телефона указан в неверном формате. Ждем Ваш номер для регистрации в федеральном формате (+7хххххххххх)", ParseMode.Default, replyMarkup: new ForceReplyMarkup { Selective = true });
                            }
                            else
                            {
                                string nameUser = message.From.Username != null ? message.From.Username : message.From.Id.ToString();
                                ReadWriteFileTxt.WriteFile(messageNumberPhone, _currentPath, nameUser, "txt", newpath: "RegisterUsers");

                                // Регистрация на основном сервисе

                                // Отправка сообщения с основного сервиса пользователю:
                                // Удаление файла 
                                //отправка QR-кода

                                //Отпрака сообщения о невозможности регистрации - дубликат ФИО или номера телефона
                                await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Спасибо за регистрацию!");

                            }
                            break;
                    }                    
                }
                else if (message.Text.Equals(commandGet))
                {
                    await _telegramBotClient.SendTextMessageAsync(message.From.Id, responseMessage, ParseMode.Default, replyMarkup: new ForceReplyMarkup { Selective = true });
                }
                else if (message.Text.Equals(commandRegistration))
                {
                    await _telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Укажите фамилию имя отчество полностью, через пробел", replyMarkup: new ForceReplyMarkup { Selective = true });
                }
                else
                {
                    //MessageData requestMessage = _messageDataService.CreateMessageData(update, this.HttpContext.Request.Host.Value.ToString());

                    //// Отправка запроса на API др. сервиса
                    //string jsonRequest = JsonSerializer.Serialize(requestMessage);
                    //string str = null;
                    //try
                    //{
                    //    string jsonResponseData = await PostRequestHttpAsync(url, jsonRequest);
                    //    ResponseMessageData responseData = JsonSerializer.Deserialize<ResponseMessageData>(jsonResponseData);
                    //    str = responseData.status;
                    //}
                    //catch (Exception ex)
                    //{
                    //    Console.WriteLine(ex.ToString());
                    //}
                    //return Ok(str);
                    responseMessage = "Выберите пункт меню:";
                    await _telegramBotClient.SendTextMessageAsync(message.From.Id, responseMessage, replyMarkup: GetMenuButtons());
                }
            }
            listlogs.Add(String.Format("Answer: {0};", responseMessage));
            ReadWriteFileTxt.WriteFile(listlogs, _currentPath, "logs_TelegramBot_" + DateTime.Now.Year+"_"+DateTime.Now.Month+"_"+ DateTime.Now.Day, "txt", newpath: "logs");

            return Ok();
        }

        private IReplyMarkup GetMenuButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                    {
                        new List<KeyboardButton>{ new KeyboardButton { Text = commandGet }, new KeyboardButton { Text = commandRegistration }},
                    },
                ResizeKeyboard = true
            };
        }

        // Сюда должны приходить сообщения с чат-бота
        //[ApiExplorerSettings(IgnoreApi = true)]
        //private async void TelegramBotClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        //{
        //    var message = e.Message;
        //    if(message != null)
        //    {
        //        MessageData requestMessage = new MessageData { TextMessage = message.Text };

        //        // Отправка запроса на API др. сервиса
        //        string jsonRequest = JsonSerializer.Serialize(requestMessage);
        //        string jsonResponseData = await PostRequestHttpAsync(url, jsonRequest);
        //        ResponseMessageData responseData = JsonSerializer.Deserialize<ResponseMessageData>(jsonResponseData);

        //        // Ответ в чат-бот 
        //        await _telegramBotClient.SendTextMessageAsync(message.Chat.Id,responseData.textMessage, replyToMessageId: message.MessageId);
        //    }
        //}

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
