using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopicPublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            string exchangeName = "exchange_topic";
            string queueTopic1 = "topic_nalong";
            string queueTopic2 = "topic_code";
            string queueTopic3 = "topic_example";

            string queueTopic1RoutingKey = "nalong.code";
            string queueTopic2RoutingKey = "nalong.example";
            string queueTopic3RoutingKey = "nalong.code.example";

            var factory = new ConnectionFactory() { HostName = "127.0.0.1", UserName = "guest", Password = "guest", Port = 5672 };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    ////消息如何持久化
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    ////定义交换机
                    channel.ExchangeDeclare(exchangeName, ExchangeType.Topic);//交换机

                    ////定义队列
                    channel.QueueDeclare(queue: queueTopic1,   //队列
                        durable: true,  ////队列持久化
                        exclusive: false,
                        autoDelete: false, ////消息消费完断开连接队列自动删除
                        arguments: null);
                    channel.QueueDeclare(queue: queueTopic2,  
                       durable: true,
                       exclusive: false,
                       autoDelete: false,
                       arguments: null);
                    channel.QueueDeclare(queue: queueTopic3,  
                       durable: true,
                       exclusive: false,
                       autoDelete: false,
                       arguments: null);

                    ////绑定queue与exchange
                    channel.QueueBind(queueTopic1, exchangeName, "nalong.#", null);
                    channel.QueueBind(queueTopic2, exchangeName, "*.code", null);
                    channel.QueueBind(queueTopic3, exchangeName, "*.example", null);

                    ////发送消息
                    channel.BasicPublish(exchange: exchangeName,
                        routingKey: queueTopic1RoutingKey,
                        basicProperties: properties,
                        body: Encoding.UTF8.GetBytes("nalong"));
                    channel.BasicPublish(exchange: exchangeName,
                        routingKey: queueTopic2RoutingKey,
                        basicProperties: properties,
                        body: Encoding.UTF8.GetBytes("code"));
                    channel.BasicPublish(exchange: exchangeName,
                        routingKey: queueTopic3RoutingKey,
                        basicProperties: properties,
                        body: Encoding.UTF8.GetBytes("example"));
                }
            }

            Console.WriteLine("消息已发送");
            Console.ReadLine();
        }
    }
}

