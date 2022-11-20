using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UserService.Misc.Serializer;

public static class MyResult
{
    public static IActionResult Result(int statusCode = StatusCodes.Status200OK,
        uint code = StatusCodes.Status200OK,
        bool hasErr = false,
        string msg = "",
        object? data = null)
        => new ObjectResult(new ReturnData { Code = code, HasErr = hasErr, Msg = msg, Data = data ?? string.Empty })
        {
            StatusCode = statusCode
        };

    public static IActionResult Success(uint code = StatusCodes.Status200OK,
        string msg = "OK",
        object? data = null)
        => new ObjectResult(new ReturnData { Code = code, HasErr = false, Msg = msg, Data = data ?? string.Empty });

    public static IActionResult Error(int statusCode = StatusCodes.Status200OK,
        uint code = StatusCodes.Status500InternalServerError,
        string msg = "Internal Error",
        object? data = null)
        => new ObjectResult(new ReturnData { Code = code, HasErr = true, Msg = msg, Data = data ?? string.Empty })
        {
            StatusCode = statusCode
        };

    public static IActionResult Invalid(int statusCode = StatusCodes.Status400BadRequest,
        uint code = StatusCodes.Status400BadRequest,
        string msg = "Invalid Request",
        object? data = null)
        => new ObjectResult(new ReturnData { Code = code, HasErr = true, Msg = msg, Data = data ?? string.Empty })
        {
            StatusCode = statusCode
        };

    public struct ReturnData
    {
        public uint   Code   { get; set; }
        public bool   HasErr { get; set; }
        public string Msg    { get; set; }
        public object Data   { get; set; }
    }
}
