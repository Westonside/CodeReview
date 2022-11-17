namespace UserService.Contract;

public static class UserConst
{
    public const int MIN_USERNAME_LENGTH = 1;
    public const int MAX_USERNAME_LENGTH = 63;
    public const int MIN_EMAIL_LENGTH    = 1;
    public const int MAX_EMAIL_LENGTH    = 63;
    public const int MIN_PASSWORD_LENGTH = 1;
    public const int MAX_PASSWORD_LENGTH = 63;

    public const string DEFAULT_USER_SELECT_PARAMS =
        "id,username,email,password,create_time,update_time";
}
