using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Contract;
using UserService.Dao;
using UserService.Misc;
using UserService.Misc.FreeSql;
using UserService.Model.DbTable;
using UserService.Model.Dto.User;

namespace UserService.Service;

public class UserService : IUserService
{
    private readonly IConfiguration       _conf;
    private readonly ITransactionFreeSql  _db;
    private readonly ILogger<UserService> _logger;
    private readonly IUserDao             _userDao;


    public UserService(IConfiguration conf, ILogger<UserService> logger, ITransactionFreeSql db, IUserDao userDao)
    {
        _conf    = conf;
        _logger  = logger;
        _db      = db;
        _userDao = userDao;
    }

    public async Task<(uint, int)> CountUser() => await _userDao.CountUser();

    public async Task<(uint, int)> CreateUser(CreateUserReqDto dto)
    {
        DbUser user = new DbUser
        {
            CreateTime = MyTime.GetTimeStamp(), Email = dto.Email, Password = dto.Password, Username = dto.Username
        };
        return await _userDao.CreateUser(user);
    }

    public async Task<(uint, UserResDto)> GetUser(int userId,
        string selectParams = UserConst.DEFAULT_USER_SELECT_PARAMS)
    {
        (var appCode, DbUser user) = await _userDao.GetUser(userId);

        if (appCode != AppCode.SUCCESS)
        {
            return (appCode, null!);
        }

        UserResDto dto = new UserResDto
        {
            Id         = user.Id,
            Username   = user.Username,
            Email      = user.Email,
            CreateTime = user.CreateTime,
            UpdateTime = user.UpdateTime
        };

        return (AppCode.SUCCESS, dto);
    }

    public async Task<uint> UpdateUser(int userId, Dictionary<string, object> user) =>
        await _userDao.UpdateUser(userId, user);

    public async Task<uint> DeleteUser(int userId) => await _userDao.DeleteUser(userId);

    public async Task<(uint, List<UserResDto>)> ListUser(int pageNum, int pageSize,
        string selectParams = UserConst.DEFAULT_USER_SELECT_PARAMS)
    {
        (var appCode, List<DbUser> users) = await _userDao.ListUser(pageNum, pageSize, selectParams);

        if (appCode != AppCode.SUCCESS)
        {
            return (appCode, null!);
        }

        List<UserResDto> dto = new List<UserResDto>();
        foreach (DbUser user in users)
        {
            dto.Add(new UserResDto
            {
                Id         = user.Id,
                Username   = user.Username,
                Email      = user.Email,
                CreateTime = user.CreateTime,
                UpdateTime = user.UpdateTime
            });
        }

        return (AppCode.SUCCESS, dto);
    }
}
