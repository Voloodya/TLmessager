using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TLmessanger
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    //Настройка кросс-платформенного web server'a Kestrel
                    // Запуск в папке проекта cmd dotnet run
                    webBuilder.UseKestrel(options =>
                    {
                        options.Limits.MaxConcurrentConnections = 100;
                        //// MaxRequestBodySize устанавливает максимальный размер для запроса в байтах
                        //options.Limits.MaxRequestBodySize = 10 * 1024;
                        //// MinRequestBodyDataRate задает минимальную скорость передачи данных в запросе в байтах в секунду
                        //options.Limits.MinRequestBodyDataRate =
                        //    new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                        ////MinResponseDataRate задает минимальную скорость передачи данных в исходящем потоке в байтах в секунду
                        //options.Limits.MinResponseDataRate =
                        //    new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                        options.Listen(IPAddress.Loopback, 5000);
                    });
                });
                
    }
}
