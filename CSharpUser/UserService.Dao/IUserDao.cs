using UserService.Contract;
using UserService.Model.DbTable;

namespace UserService.Dao;

public interface IUserDao
{
    Task<(uint, int)> CountUser();
    Task<(uint, bool)> CheckIdExist(int userId);
    Task<(uint, bool)> CheckUsernameExist(string username);
    Task<(uint, bool)> CheckEmailExist(string email);
    Task<(uint, int)> CreateUser(DbUser user);
    Task<(uint, DbUser)> GetUser(int userId, string selectParams = UserConst.DEFAULT_USER_SELECT_PARAMS);

    Task<(uint, int)> GetIdByUsername(string username);
    Task<(uint, string)> GetEmailById(int userId);
    Task<(uint, string)> GetEmailByUsername(string username);
    Task<(uint, string)> GetUsernameById(int userId);

    Task<uint> UpdateUser(int userId, Dictionary<string, object> user);
    Task<uint> DeleteUser(int userId);

    Task<(uint, List<DbUser>)> ListUser(int pageNum, int pageSize,
        string selectParams = UserConst.DEFAULT_USER_SELECT_PARAMS);
}
