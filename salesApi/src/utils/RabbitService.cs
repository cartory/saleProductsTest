using System.Text;
using System.Threading.Channels;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace src.utils
{
    public interface IRabbitService
    {
        void publish(string queue, string message);
        void receive(string queue, Action<string> action);
    }

    public class RabbitService : IRabbitService
    {
        public readonly ConnectionFactory factory;

        public RabbitService()
        {
            this.factory = new ConnectionFactory() { HostName = "localhost" };
        }

        public void publish(string queue, string message)
        {
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queue);
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(
                        body: body,
                        exchange: "",
                        routingKey: queue,
                        basicProperties: null
                    );

                    channel.Dispose();
                }

                connection.Dispose();
            }
        }

        public void receive(string queue, Action<string> action)
        {
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queue);
                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (_, args) =>
                    {
                        var body = args.Body.ToArray();
                        string message = Encoding.UTF8.GetString(body);

                        action(message);
                    };

                    channel.BasicConsume(queue: queue, consumer: consumer);
                    channel.Dispose();
                }

                connection.Dispose();
            }
        }
    }
}
