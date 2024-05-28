using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuggestionService.DTOs;

namespace SuggestionService.Service.Interfaces
{
    public interface ISuggestionService
    {
        public Task CheckSuggestionAsync(SuggestionDTO suggestionDTO);
        public Task CheckSuggestionVoteAsync(SuggestionVoteDTO suggestionDTO);
    }
}