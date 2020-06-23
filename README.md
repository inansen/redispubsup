# Redis pub/sup boardcast data with signalr

### Note
It can use the StackExchangeRedis reference because ServiceStack Redis blocks connections after an hourly 6000 request.
Note that redis Pub/Sub has no persistence,this means only connected clients could recieve messages.
## Installation

for redis
```bash
docker run --name my-redis-container -p 6379:6379 -d redis
```
for redis gui
```bash
docker run -v redisinsight:/db -p 8001:8001 redislabs/redisinsight
```
## Usage

### Producer

```bash
Install-Package StackExchange.Redis.Extensions.AspNetCore
```
#### Startup.cs
```csharp
public void ConfigureServices(IServiceCollection services)
{
     services.AddSingleton<ConnectionMultiplexer>(sp =>
     {
         var cn = ConnectionMultiplexer.Connect("localhost");
         cn.ErrorMessage += (object sender, RedisErrorEventArgs e) =>
         {
           Console.WriteLine(e.Message);
         };

         return cn;
      });
          
      services.AddScoped<MessageProducer>();
      services.AddControllersWithViews();
}
```
#### MessageProducer.cs
```csharp
public class MessageProducer
{
    private readonly ConnectionMultiplexer redisServer;
    public MessageProducer(ConnectionMultiplexer redis)
    {
            redisServer = redis;
    }

    internal void Publish(Redis.Common.Message message)
    {
       var sub = redisServer.GetDatabase().Multiplexer.GetSubscriber();
       var data = System.Text.Json.JsonSerializer.Serialize(message);
       sub.Publish("redismqdemo", data);
    }
}
```
#### HomeController.cs
To testing, easly pass messages with query string to /Home/Index action.
```csharp
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MessageProducer messageProducer;

        public HomeController(ILogger<HomeController> logger, MessageProducer producer)
        {
            _logger = logger;
            messageProducer = producer;
        }

        public IActionResult Index(string message)
        {
            messageProducer.Publish(new Message(message));
            return View();
        }
    }
```
### Subscriber

#### Startup.cs
```csharp

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
```
On Configure method
```csharp
app.UseEndpoints(endpoints =>
{
     endpoints.MapControllerRoute(
     name: "default",
     pattern: "{controller=Home}/{action=Index}/{id?}");
     endpoints.MapHub<RedisMessageHub>("/redismessagehub");
});
```

### Signalr

It can be easly added Signalr from this [link](https://docs.microsoft.com/en-us/aspnet/core/tutorials/signalr?view=aspnetcore-3.1&tabs=visual-studio).
```csharp
  public class RedisMessageHub: Hub
  {
        public async Task SendMessage(long time, string message)
        {
            var date = DateTime.FromBinary(time);
            await Clients.All.SendAsync("ReceiveMessage", date, message);
        }
  }
```
consumerhub.js
```js
"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/redismessagehub").build();

connection.on("ReceiveMessage", function (time, message) {
    console.log(time + " - " + message);
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = time + " - " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});
connection.start().then(function () {
    console.log("signalr connection established");
}).catch(function (err) {
    return console.error(err.toString());
});
```
