using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPCClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string exchangeName = "exchange_direct";
            string queue = "rpc_queue";
            string queueInfoBindingKey = "requestkey";

            string queueresult = "result_queue";

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
                    channel.QueueDeclare(queue: queue,   //队列
                        durable: true,  ////队列持久化
                        exclusive: false,
                        autoDelete: false, ////消息消费完断开连接队列自动删除
                        arguments: null);

                    ////绑定queue与exchange
                    channel.QueueBind(queue, exchangeName, queueInfoBindingKey, null);

                    ////发送消息
                    channel.BasicPublish(exchange: exchangeName,
                        routingKey: queueInfoBindingKey,
                        basicProperties: properties,
                        body: Encoding.UTF8.GetBytes("request"));

                    Console.WriteLine("rpc客户端已经启动......");
                    Console.WriteLine("正在远程调用rpc服务端：1+1=?");


                    ////得到结果
                    var consumer = new EventingBasicConsumer(channel);
                    channel.BasicConsume(queue: queueresult,
                         noAck: false,
                         consumer: consumer);

                    ////基于事件，不阻塞
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine("远程调用结束，获得了结果:"+ message);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };

                    Console.Read();
                }

            }
        }
    }
}

