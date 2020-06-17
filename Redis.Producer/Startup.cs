using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Redis.Producer.Producers;
using StackExchange.Redis;

namespace Redis.Producer
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
               
                var cn = ConnectionMultiplexer.Connect("localhost");
                cn.ErrorMessage += (object sender, RedisErrorEventArgs e) =>
                {
                    Console.WriteLine(e.Message);
                };

                //cn.GetSubscriber().Subscribe("redismqdemo", (channel, message) => {


                //    //var hub = (IHubContext<RedisMessageHub>)sp.GetService(typeof(IHubContext<RedisMessageHub>));
                //    //var data = System.Text.Json.JsonSerializer.Deserialize<Redis.Common.Message>(message);

                //    //var time = DateTime.FromBinary(data.Time).ToString("dd/MM/yyyy HH:ss");
                //    //hub.Clients.All.SendAsync("ReceiveMessage", time, data.Body);

                //    Console.WriteLine("Got notification: " + (string)message);
                //});
                return cn;
            });
            //ConnectionMultiplexer
            //services.AddSingleton<RedisMqServer>(ctx => {
            //    var redisFactory = new PooledRedisClientManager("localhost:6379");
            //    var mqHost = new RedisMqServer(redisFactory, retryCount: 2);
            //    return mqHost;
            //});
            services.AddScoped<MessageProducer>();
            services.AddControllersWithViews();
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
            });
        }
    }
}
