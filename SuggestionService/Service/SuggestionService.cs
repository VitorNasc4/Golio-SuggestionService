using SuggestionService.DTOs;
using SuggestionService.Repository.Interfaces;
using SuggestionService.Service.Interfaces;

namespace SuggestionService.Service
{
    public class SuggestionService : ISuggestionService
    {
        private readonly ISuggestionRepository _suggestionRepository;
        private readonly IMessageBusService _messageBusService;
        private readonly IConfiguration _configuration;
        private readonly int upVotesTarget;
        private readonly int downVotesTarget;
        public SuggestionService(ISuggestionRepository suggestionRepository, IMessageBusService messageBusService, IConfiguration configuration)
        {
            _suggestionRepository = suggestionRepository;
            _messageBusService = messageBusService;
            _configuration = configuration;

            upVotesTarget = 3;
            downVotesTarget = 3;

            if (int.TryParse(_configuration["SuggestionServiceConfig:UpVotesTarget"], out int upVotes))
            {
                upVotesTarget = upVotes;
            }

            if (int.TryParse(_configuration["SuggestionServiceConfig:DownVotesTarget"], out int downVotes))
            {
                downVotesTarget = downVotes;
            }
        }
        public async Task CheckSuggestionAsync(SuggestionDTO suggestionDTO)
        {
            var suggestions = await _suggestionRepository.GetSuggestionByPriceIdAsync(suggestionDTO.PriceId);

            if (suggestions is not null && suggestions.Count() > 0)
            {
                foreach (var suggestion in suggestions)
                {
                    if (suggestion.Value == suggestionDTO.Value)
                    {
                        Console.WriteLine($"Suggestion for the PriceId {suggestionDTO.PriceId} already exists with the value {suggestionDTO.Value}");
                        return;
                    }
                }
            }

            var newSuggestion = suggestionDTO.ToEntity();
            var id = int.Parse(newSuggestion.PriceId.ToString() + ((int)(newSuggestion.Value * 100)).ToString());
            newSuggestion.Id = id;
            await _suggestionRepository.CreateSuggestionAsync(newSuggestion);

            var suggestionMessage = newSuggestion.ToMessage();
            suggestionMessage.Id = id;
            await _messageBusService.SendMessageQueueAsync(suggestionMessage);

        }
        public async Task CheckSuggestionVoteAsync(SuggestionVoteDTO suggestionVoteDTO)
        {
            var suggestion = await _suggestionRepository.GetSuggestionByIdAsync(suggestionVoteDTO.SuggestionId);

            if (suggestion is null)
            {
                Console.WriteLine($"Suggestion with ID {suggestionVoteDTO.SuggestionId} not found");
                return;
            }

            foreach (var autorVoteEamil in suggestion.VotesAutorEmails)
            {
                if (autorVoteEamil == suggestionVoteDTO.EmailAutor)
                {
                    Console.WriteLine($"This suggestion vote has already evaluated by autor email {suggestionVoteDTO.EmailAutor}");
                    return;
                }
            }

            if (suggestionVoteDTO.IsValid)
            {
                suggestion.Upvotes++;
                if (suggestion.Upvotes >= upVotesTarget)
                {
                    var suggestionVoteMessage = new SuggestionVoteMessage
                    {
                        SuggestionId = suggestionVoteDTO.SuggestionId,
                        PriceId = suggestion.PriceId,
                        Value = suggestion.Value,
                        IsValid = true
                    };
                    await _messageBusService.SendMessageQueueAsync(suggestionVoteMessage);
                    await _suggestionRepository.RemoveSuggestionAsync(suggestionVoteDTO.SuggestionId);
                    return;
                }
            }
            else
            {
                suggestion.Downvotes++;
                if (suggestion.Downvotes >= downVotesTarget)
                {
                    var suggestionVoteMessage = new SuggestionVoteMessage
                    {
                        SuggestionId = suggestionVoteDTO.SuggestionId,
                        PriceId = suggestion.PriceId,
                        Value = suggestion.Value,
                        IsValid = false
                    };
                    await _messageBusService.SendMessageQueueAsync(suggestionVoteMessage);
                    await _suggestionRepository.RemoveSuggestionAsync(suggestionVoteDTO.SuggestionId);
                    return;
                }
            }
            suggestion.UpdatedAt = DateTime.UtcNow;
            suggestion.VotesAutorEmails.Add(suggestionVoteDTO.EmailAutor);

            await _suggestionRepository.UpdateSuggestionAsync(suggestionVoteDTO.SuggestionId, suggestion);
        }
    }
}