using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TLmessanger.Models;

namespace TLmessanger.Services
{
    public class MessageDataService
    {

        public MessageData CreateMessageData(Update update, string urlHost)
        {
            string messageNumberPhone;
            string firstNameUser;
            string lastNameUser;
            long idUser;

            var message = update.Message;
            messageNumberPhone = ServicePhoneNumber.LeaveOnlyNumbers(message.Text);

            firstNameUser = message.From.FirstName != null ? message.From.FirstName : null;
            lastNameUser = message.From.LastName != null ? message.From.LastName : null;
            idUser = message.From.Id;
            MessageData requestMessage;
            if (lastNameUser != null && firstNameUser != null)
            {
                requestMessage = new MessageData { PhoneNumber = messageNumberPhone, UserName = message.From.Username, UserFirstName = firstNameUser, UserLastName = lastNameUser, IdUser = idUser };
            }
            else if (firstNameUser != null)
            {
                requestMessage = new MessageData { PhoneNumber = messageNumberPhone, UserName = message.From.Username, UserFirstName = firstNameUser, IdUser = idUser };
            }
            else
            {
                requestMessage = new MessageData { PhoneNumber = messageNumberPhone, UserName = message.From.Username, IdUser = idUser };
            }
            requestMessage.UrlFrom = urlHost;

            return requestMessage;
        }

    }
}
