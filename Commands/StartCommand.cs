using System.Threading.Tasks;
using TLmessanger.Abstraction;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TLmessanger.Commands
{
    public class StartCommand : TelegramCommand
    {
        public override string Name => @"/start";

        public override bool Contains(Message message)
        {
            if (message.Type != MessageType.Text)
                return false;

            return message.Text.Contains(Name);
        }

        public override async Task Execute(Message message, ITelegramBotClient botClient)
        {
            var keyBoard = new ReplyKeyboardMarkup
            {
                Keyboard = new[]
                {
                    new[]
                    {
                        new KeyboardButton("\U0001F3E0 Главная")
                    },
                    new[]
                    {
                        new KeyboardButton("\U0001F451 Получить QR-код")
                    },
                    new []
                    {
                        new KeyboardButton("\U0001F45C Зарегистрироваться")
                    },
                    new []
                    {
                        new KeyboardButton("\U0001F4D6 Помощь")
                    }
                }
            };
            await botClient.SendTextMessageAsync(message.Chat.Id, "Здравствуйте!", parseMode: ParseMode.Html, replyMarkup: keyBoard);
        }

    }
}
