using System.Collections.Generic;

namespace Rise.Shared.Organizations;

public static class OrganizationResponse
{
    public class List
    {
        public List<OrganizationDto.Summary> Organizations { get; init; } = [];
    }
}
