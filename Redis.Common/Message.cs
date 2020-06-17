using System;

namespace Redis.Common
{
    public class Message
    {
        public Message()
        {

        }
        public Message(string message)
        {

            Key = Guid.NewGuid();
            Time = DateTime.UtcNow.Ticks;
            Body = message;
        }

        public string Body { get; set; }
        public Guid Key { get; set; }
        public long Time { get; set; }
    }
}
