using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using SuggestionService.DTOs;

namespace SuggestionService.Models
{
    public class Suggestion
    {
        [BsonId]
        public int Id { get; set; }
        public int PriceId { get; set; }
        public double Value { get; set; }
        public string? AutorName { get; set; }
        public string? AutorEmail { get; set; }
        public int Upvotes { get; set; }
        public List<string> VotesAutorEmails { get; set; } = new List<string>();
        public int Downvotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public SuggestionMessage ToMessage()
        {
            return new SuggestionMessage()
            {
                Id = Id,
                PriceId = PriceId,
                Value = Value,
                AutorName = AutorName,
                AutorEmail = AutorEmail,
            };
        }
    }

}