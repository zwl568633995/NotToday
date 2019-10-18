using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RPCServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string queue = "rpc_queue";

            var factory = new ConnectionFactory() { HostName = "127.0.0.1", UserName = "guest", Password = "guest", Port = 5672 };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    createQueue(); ///创建一个结果队列
                    channel.QueueDeclare(queue: queue, 
                        durable: true,
                        exclusive: false,
                        autoDelete: false, 
                        arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);  ////每次消费一个，解决业务复杂的情况
                    var consumer = new EventingBasicConsumer(channel);
                    channel.BasicConsume(queue: queue,
                         noAck: false,
                         consumer: consumer);

                    ////基于事件，不阻塞
                    consumer.Received += (model, ea) =>
                    {
                        var routingKey = ea.RoutingKey;
                        var body = ea.Body;
                        string response = null;
                        try
                        {
                            var message = Encoding.UTF8.GetString(body);
                            Console.WriteLine("收到请求，正在处理......");
                            response = "2";
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(" [.] " + e.Message);
                            response = "";
                        }
                        finally
                        {
                            Thread.Sleep(3000);
                            publusheQueue(response);
                            channel.BasicAck(deliveryTag: ea.DeliveryTag,
                              multiple: false);
                            Console.WriteLine("处理完毕，发送结果中......");
                            Console.WriteLine("发送完毕");
                        }
                    };

                    Console.WriteLine("rpc服务已经启动......");
                    Console.Read();
                }
            }
        }

        static void createQueue()
        {
            var factory = new ConnectionFactory() { HostName = "127.0.0.1", UserName = "guest", Password = "guest", Port = 5672 };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    ////消息如何持久化
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    ////定义交换机
                    channel.ExchangeDeclare("exchanage_direct", ExchangeType.Direct);//交换机

                    ////定义队列
                    channel.QueueDeclare(queue: "result_queue",   //队列
                        durable: true,  ////队列持久化
                        exclusive: false,
                        autoDelete: false, ////消息消费完断开连接队列自动删除
                        arguments: null);

                    ////绑定queue与exchange
                    channel.QueueBind("result_queue", "exchanage_direct", "resultkey", null);
                }
            }
        }

        static void publusheQueue(string message)
        {
            var factory = new ConnectionFactory() { HostName = "127.0.0.1", UserName = "guest", Password = "guest", Port = 5672 };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    ////消息如何持久化
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    ////定义交换机
                    channel.ExchangeDeclare("exchanage_direct", ExchangeType.Direct);//交换机

                    ////定义队列
                    channel.QueueDeclare(queue: "result_queue",   //队列
                        durable: true,  ////队列持久化
                        exclusive: false,
                        autoDelete: false, ////消息消费完断开连接队列自动删除
                        arguments: null);

                    ////绑定queue与exchange
                    channel.QueueBind("result_queue", "exchanage_direct", "resultkey", null);

                    ////发送消息
                    channel.BasicPublish(exchange: "exchanage_direct",
                        routingKey: "resultkey",
                        basicProperties: properties,
                        body: Encoding.UTF8.GetBytes(message));
                }
            }
        }
    }
}
