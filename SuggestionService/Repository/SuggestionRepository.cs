using MongoDB.Driver;
using SuggestionService.Models;
using SuggestionService.Repository.Interfaces;

namespace SuggestionService.Repository
{
    public class SuggestionRepository : ISuggestionRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<Suggestion> _suggestionCollection;
        public SuggestionRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            var mongoClient = new MongoClient(_configuration["SuggestionServiceDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(_configuration["SuggestionServiceDB:Database"]);

            _suggestionCollection = mongoDatabase.GetCollection<Suggestion>(_configuration["SuggestionServiceDB:CollectionName"]);
        }
        public async Task<Suggestion> GetSuggestionByIdAsync(int id)
        {
            return await _suggestionCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
        }
        public async Task<List<Suggestion>> GetSuggestionByPriceIdAsync(int priceId)
        {
            return await _suggestionCollection.Find(s => s.PriceId == priceId).ToListAsync();
        }
        public async Task CreateSuggestionAsync(Suggestion suggestion)
        {
            await _suggestionCollection.InsertOneAsync(suggestion);
            Console.WriteLine($"Suggestion with ID {suggestion.Id} created with success");
        }
        public async Task UpdateSuggestionAsync(int suggestionId, Suggestion suggestion)
        {
            await _suggestionCollection.ReplaceOneAsync(s => s.Id == suggestionId, suggestion);
            Console.WriteLine($"Suggestion with ID {suggestionId} updated with success");
        }
        public async Task RemoveSuggestionAsync(int suggestionId)
        {
            await _suggestionCollection.DeleteOneAsync(s => s.Id == suggestionId);
            Console.WriteLine($"Suggestion with ID {suggestionId} removed with success");
        }

        public async Task<int> GetSuggestionIdAsync(int priceId, double priceValue)
        {
            var suggestion = await _suggestionCollection.Find(s => s.PriceId == priceId && s.Value == priceValue).FirstOrDefaultAsync();
            return suggestion != null ? suggestion.Id : 0;
        }
    }
}