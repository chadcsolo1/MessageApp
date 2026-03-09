using RabbitMQ.Client;
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

// Create a message
//var orderPlaced = new OrderPlaced
//{
//    OrderId = Guid.NewGuid(),
//    Total = 99.99,
//    CreatedAt = DateTime.UtcNow
//};
//var message = JsonSerializer.Serialize(orderPlaced);
//var body = Encoding.UTF8.GetBytes(message);

//// Publish the message
//await channel.BasicPublishAsync(
//    exchange: string.Empty, // default exchange
//    routingKey: "orders",
//    mandatory: true, // fail if the message can’t be routed
//    basicProperties: new BasicProperties { Persistent = true }, // message will be saved to disk
//    body: body);

//Console.WriteLine($"Sent: {message}");


//For loop that will publish 10 messages
for (int i = 0; i < 10; i++)
{
    var messageAlt = $"{DateTime.UtcNow} - {Guid.CreateVersion7()}";
    var bodyAlt = Encoding.UTF8.GetBytes(messageAlt);

    // Publish the message
    await channel.BasicPublishAsync(
        exchange: string.Empty,
        routingKey: "message",
        mandatory: true,
        basicProperties: new BasicProperties { Persistent = true },
        body: bodyAlt);

    Console.WriteLine($"Sent: {messageAlt}");

    await Task.Delay(2000);
}
