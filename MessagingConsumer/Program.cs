using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

//Create Connection factory
var factory = new ConnectionFactory() { HostName = "localhost" };
//Create a new connection - this will open a new connection to the RabbitMQ broker
using var connection = await factory.CreateConnectionAsync();
//Establisha new channel - this is how we will communicate with RabbitMQ
using var channel = await connection.CreateChannelAsync();

// Ensure the queue exists (create it if not already there)
//Via the channel we can declare a queue, bind to a queue, unbind from a queue, work with exchanges, and acknowledge & consume messages.
await channel.QueueDeclareAsync(
    queue: "message",
    durable: true, // save to disk so the queue isn’t lost on broker restart
    exclusive: false, // can be used by other connections
    autoDelete: false, // don’t delete when the last consumer disconnects
    arguments: null); // Will create a new queue if it does not exist - making this an idempotent operation - safe to call it multiple times.


Console.WriteLine("Waiting for messages....");

//Overall Defines the consumer***
var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (sender, EventArgs) =>
{
    //Here we will get the message body as a byte array, convert it to a string, and print it to the console.
    byte[] body = EventArgs.Body.ToArray();
    string message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"Received: {message}");

    //Then we will acknowledge the message so that RabbitMQ knows it has been processed and can remove it from the Queue.
    //This will prevent other consumers from consuming this message.
    await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(EventArgs.DeliveryTag, multiple: false);
};

//Start consuming messages from the queue
await channel.BasicConsumeAsync("message", autoAck: false, consumer);

Console.ReadLine();