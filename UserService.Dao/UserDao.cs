using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Contract;
using UserService.Misc;
using UserService.Misc.FreeSql;
using UserService.Model.DbTable;

namespace UserService.Dao;

public class UserDao : IUserDao
{
    private readonly IConfiguration _conf;

    private readonly ITransactionFreeSql _db;
    private readonly ILogger<UserDao>    _logger;

    public UserDao(IConfiguration conf, ILogger<UserDao> logger, ITransactionFreeSql db)
    {
        _conf   = conf;
        _logger = logger;
        _db     = db;
    }

    public async Task<(uint, int)> CountUser()
    {
        int count;
        try
        {
            count = (int)await _db.Select<DbUser>()
                                  .Where(a => a.DeleteTime == 0)
                                  .CountAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, ErrMsg.DB_UNKNOWN_EXC, nameof(CountUser));
            return (AppCode.ERR_3010_DB_EXC, 0);
        }

        return (AppCode.SUCCESS, count);
    }

    public async Task<(uint, bool)> CheckIdExist(int userId)
    {
        if (userId <= 0)
        {
            _logger.LogWarning(ErrMsg.ILLEGAL_FUNC_PARAM, nameof(CheckIdExist));
            return (AppCode.ERR_2022_PARAM_RANGE, false);
        }

        long count;
        try
        {
            count = await _db.Select<DbUser>()
                             .Where(a => a.Id == userId && a.DeleteTime == 0)
                             .CountAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, ErrMsg.DB_UNKNOWN_EXC, nameof(CheckIdExist));
            return (AppCode.ERR_3010_DB_EXC, false);
        }

        return count == 0
            ? (AppCode.SUCCESS, false)
            : (AppCode.SUCCESS, true);
    }

    public async Task<(uint, bool)> CheckUsernameExist(string username)
    {
        int count;
        try
        {
            count = (int)await _db.Select<DbUser>()
                                  .Where(a => a.Username == username && a.DeleteTime == 0)
                                  .CountAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, ErrMsg.DB_UNKNOWN_EXC, nameof(CheckUsernameExist));
            return (AppCode.ERR_3010_DB_EXC, false);
        }

        return count == 0
            ? (AppCode.SUCCESS, false)
            : (AppCode.SUCCESS, true);
    }

    public async Task<(uint, bool)> CheckEmailExist(string email)
    {
        int count;
        try
        {
            count = (int)await _db.Select<DbUser>()
                                  .Where(a => a.Email == email && a.DeleteTime == 0)
                                  .CountAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, ErrMsg.DB_UNKNOWN_EXC, nameof(CheckEmailExist));
            return (AppCode.ERR_3010_DB_EXC, false);
        }

        return count == 0
            ? (AppCode.SUCCESS, false)
            : (AppCode.SUCCESS, true);
    }

    public async Task<(uint, int)> CreateUser(DbUser user)
    {
        var (appCode, isExist) = await CheckUsernameExist(user.Username);

        if (appCode != AppCode.SUCCESS)
        {
            _logger.LogError(ErrMsg.CALLEE_RTN_ERR,
                nameof(CreateUser), nameof(CheckUsernameExist), appCode);
            return (AppCode.ERR_2030_INTERNAL_CALL, 0);
        }

        if (isExist)
        {
            return (AppCode.ERR_3022_DB_INFO_DUPLICATED, 0);
        }


        user.CreateTime = MyTime.GetTimeStamp();
        user.UpdateTime = 0;

        int id;
        try
        {
            id = (int)await _db.Insert(user).ExecuteIdentityAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, ErrMsg.DB_UNKNOWN_EXC, nameof(CreateUser));
            return (AppCode.ERR_3010_DB_EXC, 0);
        }

        return (AppCode.SUCCESS, id);
    }

    public async Task<(uint, DbUser)> GetUser(int userId, string selectParams = UserConst.DEFAULT_USER_SELECT_PARAMS)
    {
        if (userId <= 0)
        {
            _logger.LogWarning(ErrMsg.ILLEGAL_FUNC_PARAM, nameof(GetUser));
            return (AppCode.ERR_2021_PARAM_EMPTY_OR_NULL, null!);
        }

        selectParams = MyUtil.SqlParamFixer(selectParams);
        List<DbUser> result;
        try
        {
            result = await _db.Select<DbUser>()
                              .Where(a => a.Id == userId && a.DeleteTime == 0)
                              .ToListAsync<DbUser>(selectParams);
        }
        catch (Exception e)
        {
            _logger.LogError(e, ErrMsg.DB_UNKNOWN_EXC, nameof(GetUser));
            return (AppCode.ERR_3010_DB_EXC, null!);
        }

        return result.Count == 0
            ? (AppCode.ERR_3021_DB_INFO_NOT_EXIST, null!)
            : (AppCode.SUCCESS, result[0]);
    }

    public async Task<(uint, int)> GetIdByUsername(string username)
    {
        int result;
        try
        {
            result = await _db.Select<DbUser>()
                              .Where(a => a.Username == username && a.DeleteTime == 0)
                              .FirstAsync(a => a.Id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, ErrMsg.DB_UNKNOWN_EXC, nameof(GetIdByUsername));
            return (AppCode.ERR_3010_DB_EXC, 0);
        }

        return result == 0
            ? (AppCode.ERR_3021_DB_INFO_NOT_EXIST, 0)
            : (AppCode.SUCCESS, result);
    }

    public async Task<(uint, string)> GetEmailById(int userId)
    {
        if (userId <= 0)
        {
            _logger.LogWarning(ErrMsg.ILLEGAL_FUNC_PARAM, nameof(GetEmailById));
            return (AppCode.ERR_2022_PARAM_RANGE, null!);
        }

        string result;
        try
        {
            result = await _db.Select<DbUser>()
                              .Where(a => a.Id == userId && a.DeleteTime == 0)
                              .FirstAsync(a => a.Email);
        }
        catch (Exception e)
        {
            _logger.LogError(e, ErrMsg.DB_UNKNOWN_EXC, nameof(GetEmailById));
            return (AppCode.ERR_3010_DB_EXC, null!);
        }

        return result == null
            ? (AppCode.ERR_3021_DB_INFO_NOT_EXIST, null!)
            : (AppCode.SUCCESS, result);
    }

    public async Task<(uint, string)> GetEmailByUsername(string username)
    {
        string result;
        try
        {
            result = await _db.Select<DbUser>()
                              .Where(a => a.Username == username && a.DeleteTime == 0)
                              .FirstAsync(a => a.Email);
        }
        catch (Exception e)
        {
            _logger.LogError(e, ErrMsg.DB_UNKNOWN_EXC, nameof(GetEmailByUsername));
            return (AppCode.ERR_3010_DB_EXC, null!);
        }

        return result == null
            ? (AppCode.ERR_3021_DB_INFO_NOT_EXIST, null!)
            : (AppCode.SUCCESS, result);
    }

    public async Task<(uint, string)> GetUsernameById(int userId)
    {
        if (userId <= 0)
        {
            _logger.LogWarning(ErrMsg.ILLEGAL_FUNC_PARAM, nameof(GetUsernameById));
            return (AppCode.ERR_2022_PARAM_RANGE, null!);
        }

        string result;
        try
        {
            result = await _db.Select<DbUser>()
                              .Where(a => a.Id == userId && a.DeleteTime == 0)
                              .FirstAsync(a => a.Username);
        }
        catch (Exception e)
        {
            _logger.LogError(e, ErrMsg.DB_UNKNOWN_EXC, nameof(GetUsernameById));
            return (AppCode.ERR_3010_DB_EXC, null!);
        }

        return result == null
            ? (AppCode.ERR_3021_DB_INFO_NOT_EXIST, null!)
            : (AppCode.SUCCESS, result);
    }

    public async Task<uint> UpdateUser(int userId, Dictionary<string, object> user)
    {
        if (userId <= 0 || user.Count == 0)
        {
            _logger.LogWarning(ErrMsg.ILLEGAL_FUNC_PARAM, nameof(UpdateUser));
            return AppCode.ERR_2021_PARAM_EMPTY_OR_NULL;
        }

        Dictionary<string, object> newUser = new(user);

        int affRow;
        try
        {
            affRow = await _db.Update<DbUser>()
                              .SetDto(newUser)
                              .Set(a => a.UpdateTime, MyTime.GetTimeStamp())
                              .Where(a => a.Id == userId && a.DeleteTime == 0)
                              .ExecuteAffrowsAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, ErrMsg.DB_UNKNOWN_EXC, nameof(UpdateUser));
            return AppCode.ERR_3010_DB_EXC;
        }

        return affRow == 0
            ? AppCode.ERR_3021_DB_INFO_NOT_EXIST
            : AppCode.SUCCESS;
    }

    public async Task<uint> DeleteUser(int userId)
    {
        if (userId <= 0)
        {
            _logger.LogWarning(ErrMsg.ILLEGAL_FUNC_PARAM, nameof(DeleteUser));
            return AppCode.ERR_2022_PARAM_RANGE;
        }

        int affRow;
        try
        {
            affRow = await _db.Update<DbUser>()
                              .Set(a => a.DeleteTime, MyTime.GetTimeStamp())
                              .Where(a => a.Id == userId && a.DeleteTime == 0)
                              .ExecuteAffrowsAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, ErrMsg.DB_UNKNOWN_EXC, nameof(DeleteUser));
            return AppCode.ERR_3010_DB_EXC;
        }

        return affRow == 0
            ? AppCode.ERR_3021_DB_INFO_NOT_EXIST
            : AppCode.SUCCESS;
    }

    public async Task<(uint, List<DbUser>)> ListUser(int pageNum, int pageSize,
        string selectParams = UserConst.DEFAULT_USER_SELECT_PARAMS)
    {
        if (pageNum is < PaginationConst.MinPageNum or > PaginationConst.MaxPageNum
            || pageSize is < PaginationConst.MinPageSize or > PaginationConst.MaxPageSize
            || string.IsNullOrEmpty(selectParams))
        {
            _logger.LogWarning(ErrMsg.ILLEGAL_FUNC_PARAM, nameof(ListUser));
            return (AppCode.ERR_2021_PARAM_EMPTY_OR_NULL, null!);
        }

        selectParams = MyUtil.SqlParamFixer(selectParams);
        List<DbUser> result;
        try
        {
            result = await _db.Select<DbUser>()
                              .Where(a => a.Id >= _db.Select<DbUser>()
                                                     .Where(b => b.DeleteTime == 0)
                                                     .OrderBy(b => b.Id)
                                                     .Offset((pageNum - 1) * pageSize)
                                                     .First(b => b.Id))
                              .Where(a => a.DeleteTime == 0)
                              .OrderBy(a => a.Id)
                              .Limit(pageSize)
                              .ToListAsync<DbUser>(selectParams);
        }
        catch (Exception e)
        {
            _logger.LogError(e, ErrMsg.DB_UNKNOWN_EXC, nameof(ListUser));
            return (AppCode.ERR_3010_DB_EXC, null!);
        }

        return result.Count == 0
            ? (AppCode.ERR_3021_DB_INFO_NOT_EXIST, null!)
            : (AppCode.SUCCESS, result);
    }
}
