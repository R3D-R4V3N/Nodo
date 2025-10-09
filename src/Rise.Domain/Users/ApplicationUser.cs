using System;
using System.Collections.Generic;
using Rise.Domain.Chats;
using Rise.Domain.Events;
using Rise.Domain.Supervisors;

namespace Rise.Domain.Users;

public class ApplicationUser : ChatUser
{
    /// <summary>
    /// Link to the <see cref="IdentityUser"/> account, so a Technician HAS A Account and not IS A <see cref="IdentityUser"/>./>
    /// </summary>
    public required string AccountId { get; init; }

    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Biography { get; set; }
    public DateTime Birthday { get; set; }
    public Gender Gender { get; set; }

    // profile data
    public List<string> Hobbys { get; set; } = [];
    public List<string> Likes { get; set; } = [];
    public List<string> Dislikes { get; set; } = [];

    // connections
    public List<ApplicationUser> Friends { get; set; } = [];
    public List<ApplicationUser> FriendRequests { get; set; } = [];
    public List<ApplicationUser> BlockedUsers { get; set; } = [];

    // supervisor
    public required Supervisor Supervisor { get; set; }

    // events
    public List<Event> IntrestedEvents { get; set; } = [];

    // settings
    public required ApplicationUserSettings Settings { get; set;  }
}
