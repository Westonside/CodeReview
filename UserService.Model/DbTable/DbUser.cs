using FreeSql.DataAnnotations;

namespace UserService.Model.DbTable;

[Table(Name = "user", DisableSyncStructure = true)]
public class DbUser
{
    [Column(Name = "id", IsPrimary = true, IsIdentity = true)]
    public int Id { get; set; }

    [Column(Name = "create_time")] public ulong CreateTime { get; set; }

    [Column(Name = "delete_time")] public ulong DeleteTime { get; set; } = 0;

    [Column(Name = "email", StringLength = 63, IsNullable = false)]
    public string Email { get; set; }

    [Column(Name = "password", StringLength = 127, IsNullable = false)]
    public string Password { get; set; }

    [Column(Name = "update_time")] public ulong UpdateTime { get; set; } = 0;

    [Column(Name = "username", StringLength = 63, IsNullable = false)]
    public string Username { get; set; }
}
