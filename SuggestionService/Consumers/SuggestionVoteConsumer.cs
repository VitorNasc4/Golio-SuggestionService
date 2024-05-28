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
    public class SuggestionVoteConsumer : IHostedService
    {
        private readonly IConfiguration _configuration;
        private IQueueClient queueClient;
        private readonly string? connectionString;
        private readonly string? suggestionVoteQueueName;
        private readonly IServiceProvider _serviceProvider;

        public SuggestionVoteConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            suggestionVoteQueueName = _configuration["ServiceBus:VotesQueueName"];
            connectionString = _configuration["ServiceBus:ConnectioString"];
            queueClient = new QueueClient(connectionString, suggestionVoteQueueName);

            _serviceProvider = serviceProvider;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Starting Consumer from votes queue");
            ProcessMessageHandler();
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Finishing Consumer from votes queue");
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
                Console.WriteLine($"Processing message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
                var suggestionService = scope.ServiceProvider.GetRequiredService<ISuggestionService>();

                var suggestionVoteDTO = JsonSerializer.Deserialize<SuggestionVoteDTO>(message.Body);
                if (suggestionVoteDTO is not null)
                {
                    await suggestionService.CheckSuggestionVoteAsync(suggestionVoteDTO);
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