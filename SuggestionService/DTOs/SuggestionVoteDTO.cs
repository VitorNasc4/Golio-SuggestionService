using SuggestionService.Models;

namespace SuggestionService.DTOs
{
    public class SuggestionVoteDTO
    {
        public int SuggestionId { get; set; }
        public bool IsValid { get; set; }
        public string? EmailAutor { get; set; }
    }

}