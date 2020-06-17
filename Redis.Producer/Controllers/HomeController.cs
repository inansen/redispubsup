using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Redis.Common;
using Redis.Producer.Producers;

namespace Redis.Producer.Controllers
{
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
            if (!string.IsNullOrWhiteSpace(message))
                messageProducer.Publish(new Message(message));
            return View();
        }


    }
}
