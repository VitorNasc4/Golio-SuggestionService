using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using SuggestionService.DTOs;
using SuggestionService.Service.Interfaces;

namespace SuggestionService.Consumers
{
    public class NewSuggestionConsumer : IHostedService
    {
        private readonly IConfiguration _configuration;
        private IQueueClient queueClient;
        private readonly string? connectionString;
        private readonly string? suggestionQueueName;
        private readonly IServiceProvider _serviceProvider;

        public NewSuggestionConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            suggestionQueueName = _configuration["ServiceBus:SuggestionQueueName"];
            connectionString = _configuration["ServiceBus:ConnectioString"];
            queueClient = new QueueClient(connectionString, suggestionQueueName);

            _serviceProvider = serviceProvider;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Starting Consumer from new suggestions queue");
            ProcessMessageHandler();
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Finishing Consumer from new suggestions queue");
            await queueClient.CloseAsync();
            await Task.CompletedTask;
        }

        private void ProcessMessageHandler()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var suggestionService = scope.ServiceProvider.GetRequiredService<ISuggestionService>();

                Console.WriteLine($"Processing message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

                var suggestionDTO = JsonSerializer.Deserialize<SuggestionDTO>(message.Body);
                if (suggestionDTO is not null)
                {
                    await suggestionService.CheckSuggestionAsync(suggestionDTO);
                }

                await queueClient.CompleteAsync(message.SystemProperties.LockToken);
            }

        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Error processing event: {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine($"Endpoint: {context.Endpoint}");
            Console.WriteLine($"Entity Path: {context.EntityPath}");
            Console.WriteLine($"Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}