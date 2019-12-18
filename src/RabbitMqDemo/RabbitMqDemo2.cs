using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMqDemo
{
    public class RabbitMqDemo2
    {
        public static void Publish(string[] args)
        {

            var factory = new ConnectionFactory()
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672/")
            };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("logs", ExchangeType.Fanout);
                    string input = "";

                    while (true)
                    {
                        input = Console.ReadLine();


                        var body = Encoding.UTF8.GetBytes(input);

                        channel.BasicPublish(exchange: "logs",
                                             routingKey: "",
                                             basicProperties: null,
                                             body: body);
                        Console.WriteLine(" [x] Sent {0}", input);

                        if (input == "exit")
                        {
                            break;
                        }
                    }
                }
            }

            Console.WriteLine(" Press 'exit' to exit.");
            Console.ReadLine();
        }

        public static void Consume()
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672/")
            };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("logs", ExchangeType.Fanout, durable: true);
                    //var queueName = channel.QueueDeclare().QueueName;
                    var queueName = "test001";

                    channel.QueueDeclare(queueName);

                    Console.WriteLine("Queue name {0}", queueName);
                    channel.QueueBind(queue: queueName, exchange: "logs", routingKey: "");
                    Console.WriteLine(" [*] Waiting for logs.");
                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (model, ea) =>
                     {
                         var body = ea.Body;
                         var message = Encoding.UTF8.GetString(body);
                         Console.WriteLine(" [x] {0}", message);
                     };

                    channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
