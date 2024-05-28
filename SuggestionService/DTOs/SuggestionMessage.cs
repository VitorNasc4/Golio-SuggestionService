using SuggestionService.Models;

namespace SuggestionService.DTOs
{
    public class SuggestionMessage
    {
        public int Id { get; set; }
        public double Value { get; set; }
        public string? AutorName { get; set; }
        public string? AutorEmail { get; set; }
        public int PriceId { get; set; }
    }

}