using System;
using MySqlConnector;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Rise.Persistence;

internal static class MySqlServerVersionResolver
{
    private static readonly ServerVersion DefaultServerVersion = new MySqlServerVersion(new Version(8, 0, 36));

    public static ServerVersion Resolve(string connectionString, string? configuredVersion)
    {
        if (!string.IsNullOrWhiteSpace(configuredVersion))
        {
            try
            {
                return ServerVersion.Parse(configuredVersion);
            }
            catch (Exception ex) when (ex is ArgumentException or FormatException)
            {
                throw new InvalidOperationException($"The configured MySQL server version '{configuredVersion}' is not in a valid format.", ex);
            }
        }

        try
        {
            return ServerVersion.AutoDetect(connectionString);
        }
        catch (MySqlException)
        {
            return DefaultServerVersion;
        }
    }
}
