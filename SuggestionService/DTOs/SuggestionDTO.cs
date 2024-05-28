using SuggestionService.Models;

namespace SuggestionService.DTOs
{
    public class SuggestionDTO
    {
        public double Value { get; set; }
        public string? AutorName { get; set; }
        public string? AutorEmail { get; set; }
        public int PriceId { get; set; }

        public Suggestion ToEntity()
        {
            return new Suggestion()
            {
                PriceId = PriceId,
                Value = Value,
                AutorName = AutorName,
                AutorEmail = AutorEmail,
                VotesAutorEmails = new List<string>(),
                Upvotes = 0,
                Downvotes = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }

}