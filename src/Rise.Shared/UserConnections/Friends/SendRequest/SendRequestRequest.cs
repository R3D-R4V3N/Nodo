using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Shared.UserConnections;
public static partial class UserConnectionRequest
{
    public class SendFriendRequest
    {
        public string TargetAccountId { get; set; }
    }
}
