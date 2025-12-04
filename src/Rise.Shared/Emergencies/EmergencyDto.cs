using System;

namespace Rise.Shared.Emergencies;

public static class EmergencyDto
{
    public class Create
    {
        public required string Message { get; set; }
    }

    public class Get
    {
        public int Id { get; set; }
        public string NotifierFirstName { get; set; } = string.Empty;
        public string NotifierLastName { get; set; } = string.Empty;
        public string NotifierFullName { get; set; } = string.Empty;
        public EmergencyTypeDto Type { get; set; }
        public DateTime ReportedAt { get; set; }
        public int ResolvedCount { get; set; }
        public int AllowedResolverCount { get; set; }
    }
}
