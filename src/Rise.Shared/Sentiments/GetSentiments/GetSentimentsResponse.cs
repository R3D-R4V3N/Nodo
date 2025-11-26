using Rise.Shared.UserConnections;
using Rise.Shared.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Shared.Sentiments;

public static partial class SentimentResponse
{
    public class GetSentiments
    {
        public IEnumerable<SentimentDto.Get> Sentiments { get; set; } = [];
        public int TotalCount { get; set; }
    }
}
