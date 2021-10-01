using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TLmessanger.Services
{
    public class ServicePhoneNumber
    {
        public static string LeaveOnlyNumbers(string phoneNumber)
        {            
            string clearPhoneNumber = new string(phoneNumber.Where(t => char.IsDigit(t)).ToArray());

            return clearPhoneNumber;
        }
    }
}
