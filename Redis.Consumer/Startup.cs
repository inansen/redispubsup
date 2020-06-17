using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Redis.Consumer.Hubs;
using StackExchange.Redis;

namespace Redis.Consumer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            services.AddSingleton<ConnectionMultiplexer>(sp =>
            {
                var configurationOptions = new ConfigurationOptions
                {
                    EndPoints = { "localhost:6379" }
                };
                var cn = ConnectionMultiplexer.Connect(configurationOptions);
                cn.ErrorMessage += (object sender, RedisErrorEventArgs e) =>
                {
                    Console.WriteLine(e.Message);
                };
                var db = cn.GetDatabase();
                db.Multiplexer.GetSubscriber().Subscribe("redismqdemo", (channel, message) =>
                {

                    var hub = (IHubContext<RedisMessageHub>)sp.GetService(typeof(IHubContext<RedisMessageHub>));
                    var data = System.Text.Json.JsonSerializer.Deserialize<Redis.Common.Message>(message);

                    var time = DateTime.FromBinary(data.Time).ToString("dd/MM/yyyy HH:ss");
                    hub.Clients.All.SendAsync("ReceiveMessage", time, data.Body);

                    Console.WriteLine("Got notification: " + (string)message);
                });
                return cn;

            });

            services.AddControllersWithViews();
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<RedisMessageHub>("/redismessagehub");

            });


        }
    }
}
