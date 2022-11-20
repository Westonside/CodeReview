using Microsoft.AspNetCore.Mvc;
using UserService.Misc;
using UserService.Misc.Serializer;
using UserService.Model.Dto.Pagination;
using UserService.Model.Dto.User;
using UserService.Service;

namespace UserService.WebApi.Controllers;

[ApiController]
public class UserController : ControllerBase
{
    private readonly IConfiguration          _conf;
    private readonly ILogger<UserController> _logger;
    private readonly IUserService            _userService;


    public UserController(ILogger<UserController> logger, IConfiguration conf, IUserService userService)
    {
        _logger      = logger;
        _conf        = conf;
        _userService = userService;
    }

    [Route("/user/{id:int}")]
    [HttpGet]
    public async Task<IActionResult> GetUser(int id)
    {
        (var appCode, UserResDto dto) = await _userService.GetUser(id);

        if (appCode != AppCode.SUCCESS)
        {
            if (appCode == AppCode.ERR_3021_DB_INFO_NOT_EXIST)
                return MyResult.Error(statusCode: 404, code: 404, msg: "not found");

            return MyResult.Error(500, msg: "cannot get user");
        }

        return MyResult.Success(data: dto);
    }

    [Route("/user/")]
    [HttpGet]
    public async Task<IActionResult> ListUser([FromQuery] PaginationReqDto reqDto)
    {
        Task<(uint, List<UserResDto>)> getListTask  = _userService.ListUser(reqDto.PageNum, reqDto.PageSize);
        Task<(uint, int)>              getCountTask = _userService.CountUser();
        (var appCodeList, List<UserResDto> userList) = await getListTask;
        var (appCodeCount, count)                    = await getCountTask;

        if (appCodeList != AppCode.SUCCESS)
        {
            if (appCodeList == AppCode.ERR_3021_DB_INFO_NOT_EXIST)
            {
                return MyResult.Success(msg: "empty result");
            }

            return MyResult.Error(500, msg: "failed to get user list");
        }

        if (appCodeCount != AppCode.SUCCESS)
        {
            return MyResult.Error(500, msg: "failed to count user");
        }

        return MyResult.Success(data: PaginationResMaker.Make(userList, count, reqDto.PageNum, reqDto.PageSize));
    }

    [Route("/user/")]
    [HttpPost]
    public async Task<IActionResult> CreatUser([FromForm] CreateUserReqDto dto)
    {
        var (appCode, id) = await _userService.CreateUser(dto);

        if (appCode != AppCode.SUCCESS)
        {
            return MyResult.Error(500, msg: "cannot create user");
        }

        return MyResult.Success(data: new { Id = id });
    }

    [Route("/user/{id:int}")]
    [HttpDelete]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var appCode = await _userService.DeleteUser(id);

        if (appCode != AppCode.SUCCESS)
        {
            if (appCode == AppCode.ERR_3021_DB_INFO_NOT_EXIST)
                return MyResult.Error(statusCode: 404, code: 404, msg: "not found");

            return MyResult.Error(500, msg: "cannot delete user");
        }

        return MyResult.Success(data: new { Id = id });
    }
}
