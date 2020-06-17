using StackExchange.Redis;

namespace Redis.Producer.Producers
{
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
}
