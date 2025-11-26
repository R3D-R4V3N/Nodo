using Rise.Shared.Sentiments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Shared.Hobbies;

public static partial class HobbyResponse
{
    public class GetHobbies
    {
        public IEnumerable<HobbyDto.Get> Hobbies { get; set; } = [];
        public int TotalCount { get; set; }
    }
}
