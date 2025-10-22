<<<<<<< HEAD
﻿using Rise.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
=======
using Rise.Shared.Common;
>>>>>>> codex/add-alert-message-for-supervisor-monitoring

namespace Rise.Shared.UserConnections;
public interface IUserConnectionService
{
<<<<<<< HEAD
    Task<Result<UserConnectionResponse.Index>> GetFriendIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
=======
   Task<Result<UserConnectionResponse.GetFriends>> GetFriendIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
    Task<Result<string>> AddFriendAsync(string targetAccountId, CancellationToken ctx = default);
    
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
}
