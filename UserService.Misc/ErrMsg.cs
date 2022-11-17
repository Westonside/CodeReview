namespace UserService.Misc;

public static class ErrMsg
{
    public const string ILLEGAL_FUNC_PARAM = "Func {Caller} received invalid param";

    public const string CALLEE_RTN_ERR = "In func {Caller}，invoked func {Callee} returns error AppCode {RtnData}";

    public const string DB_UNKNOWN_EXC = "Unknown exc happened during db operation in func {Caller}";
}
