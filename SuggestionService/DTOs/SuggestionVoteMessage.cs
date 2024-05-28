namespace SuggestionService.DTOs
{
    public class SuggestionVoteMessage
    {
        public int SuggestionId { get; set; }
        public int PriceId { get; set; }
        public double Value { get; set; }
        public bool IsValid { get; set; }
    }

}