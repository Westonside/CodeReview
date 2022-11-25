namespace UserService.Model.Dto.User;

public class UserResDto
{
    public int    Id         { get; set; }
    public string Username   { get; set; }
    public string Email      { get; set; }
    public ulong  CreateTime { get; set; }
    public ulong  UpdateTime { get; set; }
}
