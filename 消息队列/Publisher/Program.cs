using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            string exchangeName = "exchange_direct";
            string queue = "deadqueue";
            string bindingkey = "direct_dead";
            var factory = new ConnectionFactory() { HostName = "127.0.0.1", UserName = "guest", Password = "guest", Port = 5672 };
            //factory.RequestedHeartbeat = 10;
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    ////消息如何持久化
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    ////定义交换机
                    channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);//交换机

                    //arguments
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    //dic.Add("x-expires", 10000);//队列是否被删除
                    dic.Add("x-message-ttl", 8000);//队列上消息过期时间，应小于队列过期时间  
                    dic.Add("x-dead-letter-exchange", "exchange-direct");//过期消息转向路由  
                    dic.Add("x-dead-letter-routing-key", "routing-delay");//过期消息转向路由相匹配routingkey  

                    channel.QueueDeclare(queue: queue,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: dic);

                    ////绑定queue与exchange
                    channel.QueueBind(queue, exchangeName, bindingkey, null);


                    ////发送消息
                    channel.BasicPublish(exchange: exchangeName,
                        routingKey: bindingkey,
                        basicProperties: properties,
                        body: Encoding.UTF8.GetBytes("姓名：朱星汉 手机：18051968628"));
                }
            }


            Console.Read();
        }
    }
}
