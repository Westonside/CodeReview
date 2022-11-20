using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using UserService.Contract;

namespace UserService.Model.Dto.User;

public class CreateUserReqDto
{
    [FromForm]
    [Required]
    [EmailAddress]
    [StringLength(UserConst.MAX_EMAIL_LENGTH,
        MinimumLength = UserConst.MIN_EMAIL_LENGTH)]
    public string Email { get; set; }

    [FromForm]
    [Required]
    [StringLength(UserConst.MAX_USERNAME_LENGTH,
        MinimumLength = UserConst.MIN_USERNAME_LENGTH)]
    public string Username { get; set; }

    [FromForm]
    [Required]
    [StringLength(UserConst.MAX_PASSWORD_LENGTH,
        MinimumLength = UserConst.MIN_PASSWORD_LENGTH)]
    public string Password { get; set; }
}
