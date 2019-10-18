using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            string exchange = "exchange-direct";
            string queue = "direct_info";
            string routingkey = "routing-delay";
            var factory = new ConnectionFactory() { HostName = "127.0.0.1", UserName = "guest", Password = "guest", Port = 5672 };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: exchange, type: "direct");
                    string name = channel.QueueDeclare().QueueName;

                    //string name = "deadQueue";
                    channel.QueueBind(queue: name, exchange: exchange, routingKey: routingkey);
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);////每次消费一个，解决业务复杂的情况

                    //回调，当consumer收到消息后会执行该函数
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var routingKey = ea.RoutingKey;
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine("Received {0}", message);

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };


                    channel.BasicConsume(queue: name,
                     noAck: false,
                     consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }

            Console.Read();
        }
    }
}
