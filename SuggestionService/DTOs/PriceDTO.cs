using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuggestionService.Models;

namespace SuggestionService.DTOs
{
    public class PriceDTO
    {
        public int PriceId { get; set; }
        public double NewValue { get; set; }
    }

}