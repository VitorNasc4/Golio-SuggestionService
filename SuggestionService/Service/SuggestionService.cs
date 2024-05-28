using SuggestionService.DTOs;
using SuggestionService.Repository.Interfaces;
using SuggestionService.Service.Interfaces;

namespace SuggestionService.Service
{
    public class SuggestionService : ISuggestionService
    {
        private readonly ISuggestionRepository _suggestionRepository;
        private readonly IMessageBusService _messageBusService;
        public SuggestionService(ISuggestionRepository suggestionRepository, IMessageBusService messageBusService)
        {
            _suggestionRepository = suggestionRepository;
            _messageBusService = messageBusService;
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
            var suggestionVote = await _suggestionRepository.GetSuggestionByIdAsync(suggestionVoteDTO.SuggestionId);

            if (suggestionVote is null)
            {
                Console.WriteLine($"Suggestion with ID {suggestionVoteDTO.SuggestionId} not found");
                return;
            }

            if (suggestionVoteDTO.IsValid)
            {
                suggestionVote.Upvotes++;
                if (suggestionVote.Upvotes >= 2)
                {
                    var suggestionVoteMessage = new SuggestionVoteMessage
                    {
                        SuggestionId = suggestionVoteDTO.SuggestionId,
                        PriceId = suggestionVote.PriceId,
                        Value = suggestionVote.Value,
                        IsValid = true
                    };
                    await _messageBusService.SendMessageQueueAsync(suggestionVoteMessage);
                    await _suggestionRepository.RemoveSuggestionAsync(suggestionVoteDTO.SuggestionId);
                    return;
                }
            }
            else
            {
                suggestionVote.Downvotes++;
                if (suggestionVote.Downvotes >= 2)
                {
                    var suggestionVoteMessage = new SuggestionVoteMessage
                    {
                        SuggestionId = suggestionVoteDTO.SuggestionId,
                        PriceId = suggestionVote.PriceId,
                        Value = suggestionVote.Value,
                        IsValid = false
                    };
                    await _messageBusService.SendMessageQueueAsync(suggestionVoteMessage);
                    await _suggestionRepository.RemoveSuggestionAsync(suggestionVoteDTO.SuggestionId);
                    return;
                }
            }
            suggestionVote.UpdatedAt = DateTime.UtcNow;

            await _suggestionRepository.UpdateSuggestionAsync(suggestionVoteDTO.SuggestionId, suggestionVote);
        }
    }
}