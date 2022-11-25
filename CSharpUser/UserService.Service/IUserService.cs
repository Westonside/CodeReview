using UserService.Contract;
using UserService.Model.Dto.User;

namespace UserService.Service;

public interface IUserService
{
    Task<(uint, int)> CountUser();

    Task<(uint, int)> CreateUser(CreateUserReqDto dto);

    Task<(uint, UserResDto)> GetUser(int userId, string selectParams = UserConst.DEFAULT_USER_SELECT_PARAMS);

    Task<uint> UpdateUser(int userId, Dictionary<string, object> user);

    Task<uint> DeleteUser(int userId);

    Task<(uint, List<UserResDto>)> ListUser(int pageNum, int pageSize,
        string selectParams = UserConst.DEFAULT_USER_SELECT_PARAMS);
}
