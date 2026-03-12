using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

//***Consumer-2***
Console.WriteLine("Consumer 2");

//Create Connection factory
var factory = new ConnectionFactory() { HostName = "localhost" };
//Create a new connection - this will open a new connection to the RabbitMQ broker
using var connection = await factory.CreateConnectionAsync();
//Establisha new channel - this is how we will communicate with RabbitMQ
using var channel = await connection.CreateChannelAsync();

//Using an exchange instead of a queue - this will allow us to publish messages to the exchange and have them routed to the appropriate queue(s) based on the routing key.
//This is so that we can scale on the consumer side - we can have multiple consumers consuming from the same exchange and RabbitMQ will load balance the messages between them.
await channel.ExchangeDeclareAsync(
    exchange: "messages",
    durable: true,
    autoDelete: false,
    type: ExchangeType.Fanout);


// Ensure the queue exists (create it if not already there)
//Via the channel we can declare a queue, bind to a queue, unbind from a queue, work with exchanges, and acknowledge & consume messages.
await channel.QueueDeclareAsync(
    queue: "message-2",
    durable: true, // save to disk so the queue isn’t lost on broker restart
    exclusive: false, // can be used by other connections
    autoDelete: false, // don’t delete when the last consumer disconnects
    arguments: null); // Will create a new queue if it does not exist - making this an idempotent operation - safe to call it multiple times.


//We need to bind the queue to the exchange so that messages published to the exchange will be routed to the queue.
await channel.QueueBindAsync("message-2", "messages", string.Empty);

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
await channel.BasicConsumeAsync("message-2", autoAck: false, consumer);

Console.ReadLine();
