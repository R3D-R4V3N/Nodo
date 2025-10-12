using System;

namespace Rise.Shared.Organizations;

public static class OrganizationResponse
{
    public class List
    {
        public IEnumerable<OrganizationDto.Index> Organizations { get; set; } = Array.Empty<OrganizationDto.Index>();
    }
}
