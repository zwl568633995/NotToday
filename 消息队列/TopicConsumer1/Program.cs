using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopicConsumer1
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "127.0.0.1", UserName = "guest", Password = "guest", Port = 5672 };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);////每次消费一个，解决业务复杂的情况

                    var consumer = new EventingBasicConsumer(channel);
                    ////基于事件，不阻塞
                    consumer.Received += (model, ea) =>
                    {
                        var routingKey = ea.RoutingKey;
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine("Received {0}", message);

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };


                    channel.BasicConsume(queue: "topic_nalong",
                     noAck: false,
                     consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
