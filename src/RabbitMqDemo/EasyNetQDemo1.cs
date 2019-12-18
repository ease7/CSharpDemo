using EasyNetQ;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMqDemo
{
    public class EasyNetQDemo1
    {
        static void StartEasyNetQServer()
        {
            const string host = "host=172.16.127.101:35672;username=guest;password=guest";

            Console.WriteLine("Hello World!");

            using (var bus = RabbitHutch.CreateBus(host))
            {
                var input = "";

                Console.WriteLine("Enter a message. 'Quit' to quit.");
                while ((input = Console.ReadLine()) != "Quit")
                {
                    string json = JsonConvert.SerializeObject(new TextMessage() { Text = input });

                    var result = bus.Request<string, string>(json, o => o.WithQueueName("TEST_QUEUE"));

                    Console.WriteLine(result);
                }
            }
        }

        static void StartEasyNetQConsumer()
        {
            const string host = "host=172.16.127.101:35672;username=guest;password=guest";


            using (var bus = RabbitHutch.CreateBus(host))
            {
                //bus.Subscribe<string>("", HandleTextMessage, o=> o.WithQueueName("TEST_QUEUE").WithTopic("test"));

                bus.Respond<string, string>(RespondHandler, o => o.WithQueueName("TEST_QUEUE"));

                Console.WriteLine("Listening for messages. Hit <return> to quit.");
                Console.ReadLine();
            }
        }


        static void HandleTextMessage(string textMessage)
        {
            var item = JsonConvert.DeserializeObject<TextMessage>(textMessage);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Got message: {0}", item.Text);
            Console.ResetColor();
        }

        static string RespondHandler(string request)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Got message: {0}", request);
            Console.ResetColor();

            return $"RESULT:{request}";
        }
    }
}
