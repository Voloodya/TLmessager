using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Telegram.Bot;
using TLmessanger.Abstraction;
using TLmessanger.Models;
using TLmessanger.Services;

namespace TLmessanger
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //Add
            BotConfig = Configuration.GetSection("TelegramBotConfiguration").Get<TelegramBotConfiguration>();
        }

        public IConfiguration Configuration { get; }
        //Add
        private TelegramBotConfiguration BotConfig { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            //services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TLmessanger", Version = "v1" });
            });

            // The Telegram.Bot library heavily depends on Newtonsoft.Json library to deserialize
            // incoming webhook updates and send serialized responses back.
            // Read more about adding Newtonsoft.Json to ASP.NET Core pipeline:
            //   https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-5.0#add-newtonsoftjson-based-json-format-support
            services
                .AddScoped<ICommandService, CommandService>()
                .AddControllers()
                .AddNewtonsoftJson();
                //.AddFluentValidation();

            // Подключаем HttpClient для внедрения
            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TLmessanger v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            //Add
            //app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
