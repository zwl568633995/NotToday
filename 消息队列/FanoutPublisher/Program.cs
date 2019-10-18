using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FanoutPublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            string exchangeName = "exchange_fanout";
            string queueInfo = "fanout_test1";
            string queueError = "fanout_test2";
            //factory.RequestedHeartbeat = 10;  //心跳检测
            var factory = new ConnectionFactory() { HostName = "127.0.0.1", UserName = "guest", Password = "guest", Port = 5672 };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    ////定义交换机
                    channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);//交换机

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
                    channel.QueueBind(queueInfo, exchangeName, "", null);
                    channel.QueueBind(queueError, exchangeName, "", null);

                    ////消息如何持久化
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    ////发送消息
                    channel.BasicPublish(exchange: exchangeName,
                        routingKey: "",
                        basicProperties: properties,
                        body: Encoding.UTF8.GetBytes("fanout"));
                }
            }

            Console.WriteLine("消息已发送");
            Console.ReadLine();
        }
    }
}
