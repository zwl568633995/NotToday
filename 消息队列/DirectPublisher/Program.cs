using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectPublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            string exchangeName = "exchange_direct";
            string queueInfo = "direct_info";
            string queueError = "direct_error";
            string queueInfoBindingKey = "info";
            string queueErrorBindingKey = "error";

            var factory = new ConnectionFactory() { HostName = "127.0.0.1", UserName = "guest", Password = "guest", Port = 5672 };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    ////消息如何持久化
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    ////定义交换机
                    channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);//交换机

                    ////定义队列
                    channel.QueueDeclare(queue: queueInfo,   //队列
                        durable: true,  ////队列持久化
                        exclusive: false,
                        autoDelete: false, ////消息消费完断开连接队列自动删除
                        arguments: null);
                    channel.QueueDeclare(queue: queueError,   //队列
                       durable: true,
                       exclusive: false,
                       autoDelete: false,
                       arguments: null);

                    ////绑定queue与exchange
                    channel.QueueBind(queueInfo, exchangeName, queueInfoBindingKey, null);
                    channel.QueueBind(queueError, exchangeName, queueErrorBindingKey, null);

                    ////发送消息
                    channel.BasicPublish(exchange: exchangeName,
                        routingKey: queueInfoBindingKey,
                        basicProperties: properties,
                        body: Encoding.UTF8.GetBytes("info"));
                    channel.BasicPublish(exchange: exchangeName,
                        routingKey: queueErrorBindingKey,
                        basicProperties: properties,
                        body: Encoding.UTF8.GetBytes("error"));                }
            }

            Console.WriteLine("消息已发送");
            Console.ReadLine();
        }
    }
}
