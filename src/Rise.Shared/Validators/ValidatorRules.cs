namespace Rise.Shared.Validators;
public class ValidatorRules
{
    //value objects
    //public required int MAX_AVATAR_URL_LENGTH { get; set;}
    public required int MAX_BIOGRAPHY_LENGTH {get; set;}
    public required int MAX_DEFAULT_CHAT_LINE_LENGTH {get; set;}
    public required int MAX_DEFAULT_CHAT_LINES_COUNT { get; set; }
    public required int MAX_EMAIL_LENGTH {get; set;}
    public required int MAX_FIRSTNAME_LENGTH {get; set;}
    public required int MAX_FONT_SIZE {get; set;}
    public required int MIN_FONT_SIZE {get; set;}
    public required int MAX_LASTNAME_LENGTH {get; set;}
    public required int MAX_REGISTRATION_NOTE_LENGTH {get; set;}
    public required int MAX_TEXT_MESSAGE_LENGTH {get; set;}
    // user
    public required int MAX_HOBBIES_COUNT { get; set; }
    public required int MAX_SENTIMENTS_PER_TYPE { get; set; }
}
