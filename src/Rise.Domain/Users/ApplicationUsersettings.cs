namespace Rise.Domain.Users;

public class ApplicationUserSettings : Entity
{
    public bool IsDarkMode { get; set; }
    public bool NotificationsEnabled { get; set; }
    public int FontSize { get; set; }
    public List<string> CommonPhrases { get; set; } = [];
}