using SuggestionService.Models;

namespace SuggestionService.Repository.Interfaces
{
    public interface ISuggestionRepository
    {
        public Task<Suggestion> GetSuggestionByIdAsync(int id);
        public Task<List<Suggestion>> GetSuggestionByPriceIdAsync(int priceId);
        public Task<int> GetSuggestionIdAsync(int priceId, double priceValue);
        public Task CreateSuggestionAsync(Suggestion suggestion);
        public Task UpdateSuggestionAsync(int priceId, Suggestion suggestion);
        public Task RemoveSuggestionAsync(int priceId);
    }
}